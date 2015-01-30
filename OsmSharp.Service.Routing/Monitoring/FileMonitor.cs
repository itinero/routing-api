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

using System.IO;
using System.Threading;

namespace OsmSharp.Service.Routing.Monitoring
{
    /// <summary>
    /// Represents a file monitor that continually monitors a file for changes.
    /// </summary>
    public class FileMonitor
    {
        private const int MONITOR_INTERVAL = 9 * 1000;

        /// <summary>
        /// Holds the fileinfo of the file to monitor.
        /// </summary>
        private FileInfo _fileInfo;

        /// <summary>
        /// Holds the timer to poll file changes.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// A delegate for the file changed event.
        /// </summary>
        public delegate void FileMonitorDelegate(FileMonitor sender);

        /// <summary>
        /// An event to report that the file has changed and is accessible.
        /// </summary>
        public event FileMonitorDelegate FileChanged;

        /// <summary>
        /// Creates a new file monitor for the given file.
        /// </summary>
        /// <param name="path"></param>
        public FileMonitor(string path) 
        {
            _fileInfo = new FileInfo(path);
            _timestamp = _fileInfo.LastAccessTime.Ticks;

            _timer = new Timer(Tick, null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);   
        }

        /// <summary>
        /// Starts monitoring.
        /// </summary>
        public void Start()
        {
            _fileInfo.Refresh();
            _timestamp = _fileInfo.LastAccessTime.Ticks;
            _timer.Change(MONITOR_INTERVAL, MONITOR_INTERVAL);
        }

        /// <summary>
        /// Stops monitoring.
        /// </summary>
        public void Stop()
        {
            _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// Returns true if the file is available for reading and not in a state that is changing.
        /// </summary>
        /// <returns></returns>
        public bool IsAvailable()
        {
            lock (_sync)
            {
                _fileInfo.Refresh();
                if(!_fileInfo.Exists)
                {
                    return false;
                }
                return !IsFileLocked(_fileInfo);
            }
        }

        /// <summary>
        /// Holds the last modified timestamp.
        /// </summary>
        private long _timestamp;

        /// <summary>
        /// Holds an object that is used to sync the timer.
        /// </summary>
        private object _sync = new object();

        /// <summary>
        /// Called when the timer ticks.
        /// </summary>
        /// <param name="state"></param>
        private void Tick(object state)
        {
            lock(_sync)
            {
                if (this.FileChanged != null)
                {
                    _fileInfo.Refresh();
                    if (_fileInfo.Exists)
                    {
                        if (_timestamp != _fileInfo.LastAccessTime.Ticks)
                        { // file has been written to.
                            _timestamp = _fileInfo.LastAccessTime.Ticks;
                            this.FileChanged(this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Not so nice code to check if a file is locked.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read);
            }
            catch
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}