// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Service.Routing.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace OsmSharp.Service.Routing.Monitoring
{
    /// <summary>
    /// A file monitor monitoring all files relevant to one instance.
    /// </summary>
    internal class InstanceMonitor
    {
        private const int MONITOR_INTERVAL = 20 * 1000;

        /// <summary>
        /// The list of files to monitor.
        /// </summary>
        private List<FileMonitor> _filesToMonitor;

        /// <summary>
        /// Holds the has changed flag.
        /// </summary>
        private bool _hasChanged;

        /// <summary>
        /// Holds the last change.
        /// </summary>
        private long _lastChange;

        /// <summary>
        /// Holds the timer.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// Delegate to the reload-code.
        /// </summary>
        private ApiBootstrapper.InstanceLoaderDelegate _reloadDelegate;

        /// <summary>
        /// Holds the api configuration.
        /// </summary>
        private ApiConfiguration _apiConfiguration;

        /// <summary>
        /// Holds the instance configuration.
        /// </summary>
        private InstanceConfiguration _instanceConfiguration;

        /// <summary>
        /// Creates a new instance monitor.
        /// </summary>
        internal InstanceMonitor(ApiConfiguration apiConfiguration, InstanceConfiguration instanceConfiguration, ApiBootstrapper.InstanceLoaderDelegate instanceLoader)
        {
            _filesToMonitor = new List<FileMonitor>();
            _hasChanged = false;
            _lastChange = DateTime.Now.Ticks;
            _reloadDelegate = instanceLoader;
            _apiConfiguration = apiConfiguration;
            _instanceConfiguration = instanceConfiguration;

            _timer = new Timer(Tick, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Starts to monitor.
        /// </summary>
        public void Start()
        {
            _hasChanged = false;
            _lastChange = DateTime.Now.Ticks;
            _timer.Change(MONITOR_INTERVAL, MONITOR_INTERVAL);
            foreach(var monitor in _filesToMonitor)
            {
                monitor.FileChanged += monitor_FileChanged;
                monitor.Start();
            }
        }

        /// <summary>
        /// Holds the sync object.
        /// </summary>
        private object _sync = new object();

        /// <summary>
        /// Called when one for the file monitors detects a change.
        /// </summary>
        /// <param name="sender"></param>
        void monitor_FileChanged(FileMonitor sender)
        {
            lock(_sync)
            {
                _hasChanged = true;
                _lastChange = DateTime.Now.Ticks;
            }
        }

        /// <summary>
        /// Stops monitoring.
        /// </summary>
        public void Stop()
        {
            foreach(var monitor in _filesToMonitor)
            {
                monitor.Stop();
                monitor.FileChanged -= monitor_FileChanged;
            }
            _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Adds a new file to monitor.
        /// </summary>
        /// <param name="path"></param>
        public void AddFile(string path)
        {
            if (File.Exists(path))
            {
                _filesToMonitor.Add(new FileMonitor(path));
            }
        }

        /// <summary>
        /// Called when the timer ticks.
        /// </summary>
        private void Tick(object state)
        {
            lock(_sync)
            {
                if (_hasChanged)
                {
                    var timeSpan = new TimeSpan(DateTime.Now.Ticks - _lastChange);
                    if (timeSpan.TotalMilliseconds > (MONITOR_INTERVAL / 2))
                    { // more than 4 mins ago when the last change was reported.
                        bool isAvailable = true;
                        foreach (var monitor in _filesToMonitor)
                        {
                            if (!monitor.IsAvailable())
                            {
                                _hasChanged = true;
                                _lastChange = DateTime.Now.Ticks;
                                isAvailable = false;
                                break;
                            }
                        }
                        if (isAvailable)
                        { // call reload.
                            _hasChanged = false;
                            if (!_reloadDelegate.Invoke(_apiConfiguration, _instanceConfiguration))
                            { // loading failed, try again in 5 mins.
                                _hasChanged = true;
                                _lastChange = DateTime.Now.Ticks;
                            }
                        }
                    }
                }
            }
        }
    }
}