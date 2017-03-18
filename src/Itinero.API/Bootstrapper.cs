using Itinero.API.FileMonitoring;
using Itinero.API.Instances;
using Itinero.Logging;
using Itinero.Osm.Vehicles;
using Itinero.Transit;
using Itinero.Transit.Data;
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
                Logger.Log("Bootstrapper", TraceEventType.Information,
                    "Loading all routerdb's from path: {0}", dataDirectory.FullName);

                // load all relevant files.
                var routingFiles = dataDirectory.GetFiles("*.routerdb").Concat(
                    dataDirectory.GetFiles("*.multimodaldb"));
                if (routingFiles.Count() == 0)
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
                if (!file.Exists)
                {
                    return false;
                }

                if (file.Name.EndsWith("routerdb"))
                {
                    RouterDb routerDb;
                    using (var stream = File.OpenRead(file.FullName))
                    {
                        routerDb = RouterDb.Deserialize(stream);
                    }
                    var multimodalDb = new MultimodalDb(routerDb, new TransitDb());
                    var multimodalRouter = new MultimodalRouter(multimodalDb,
                        Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());
                    var instance = new Instances.Instance(multimodalRouter);
                    InstanceManager.Register(file.Name.GetNameUntilFirstDot(), instance);
                }
                else if (file.Name.EndsWith("multimodaldb"))
                {
                    MultimodalDb routerDb;
                    using (var stream = File.OpenRead(file.FullName))
                    {
                        routerDb = MultimodalDb.Deserialize(stream);
                    }
                    var multimodalRouter = new MultimodalRouter(routerDb,
                        Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());
                    var instance = new Instances.Instance(multimodalRouter);
                    InstanceManager.Register(file.Name.GetNameUntilFirstDot(), instance);
                }

                Logger.Log("Bootstrapper", TraceEventType.Information,
                    "Loaded instance {1} from: {0}", file.FullName, file.Name.GetNameUntilFirstDot());

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
