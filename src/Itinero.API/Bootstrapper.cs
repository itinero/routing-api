using Itinero.API.FileMonitoring;
using Itinero.API.Instances;
using Itinero.Logging;
using Itinero.Osm.Vehicles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Itinero.API
{
    /// <summary>
    /// A boot strapper to load and reload instances.
    /// </summary>
    public static class Bootstrapper
    {
        private static List<IFilesMonitor> _fileMonitors = new List<IFilesMonitor>();

        /// <summary>
        /// Initializes all routing instance from the configuration in the configuration file.
        /// </summary>
        public static void BootFromConfiguration(string path)
        {
            try
            {
                // register vehicle profiles.
                Vehicle.RegisterVehicles();

                // load all .routerdb files.
                var dataDirectory = new DirectoryInfo(path);
                if (!dataDirectory.Exists)
                {
                    throw new DirectoryNotFoundException(
                        string.Format("Configured data directory doesn't exist: {0}", dataDirectory.FullName));
                }

                // load all relevant files.
                var routingFiles = dataDirectory.GetFiles("*.routerdb");
                if (routingFiles.Length == 0)
                {
                    throw new DirectoryNotFoundException(
                        string.Format("No .routerdb files found in {0}", dataDirectory.FullName));
                }

                // load each routerdb on other threads.
                foreach (var file in routingFiles)
                {
                    var thread = new Thread((state) =>
                    {
                        var localFile = state as FileInfo;

                        if (Bootstrapper.LoadInstance(localFile))
                        {
                            var monitor = new FilesMonitor<FileInfo>((f) =>
                            {
                                return Bootstrapper.LoadInstance(f);
                            }, localFile);
                            monitor.Start();
                            _fileMonitors.Add(monitor);
                        }
                    });
                    thread.Start(file);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Bootstrapper", TraceEventType.Critical,
                    "Failed to start service: {0}", ex.ToInvariantString());
            }
        }

        /// <summary>
        /// Loads an instance from a routing file.
        /// </summary>
        public static bool LoadInstance(FileInfo file)
        {
            try
            {
                RouterDb routerDb;
                
                if (!file.Exists)
                {
                    return false;
                }

                using (var stream = File.OpenRead(file.FullName))
                {
                    routerDb = RouterDb.Deserialize(stream);
                }
                var instance = new Instances.Instance(new Router(routerDb));

                InstanceManager.Register(file.Name.GetNameUntilFirstDot(), instance);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Bootstrapper", TraceEventType.Critical, "Failed to load file {0}: {1}", file,
                    ex.ToInvariantString());
            }
            return false;
        }

        /// <summary>
        /// Gets the substring until the first dot.
        /// </summary>
        private static string GetNameUntilFirstDot(this string name)
        {
            var dotIdx = name.IndexOf('.');
            if (dotIdx == 0)
            {
                throw new Exception("No '.' found in file name.");
            }
            return name.Substring(0, dotIdx);
        }
    }
}
