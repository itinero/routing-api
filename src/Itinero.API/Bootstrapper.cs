using Itinero.API.FileMonitoring;
using Itinero.API.Instances;
using Itinero.IO.Osm;
using Itinero.Logging;
using Itinero.Osm.Vehicles;
using Itinero.Transit;
using Itinero.Transit.Data;
using OsmSharp.Streams;
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
                            monitor.AddFile(file.FullName);
                            _fileMonitors.Add(monitor);
                        }
                    });
                    thread.Start(file);
                }

                // check if there are folder with OSM-XML or OSM-PBF file.
                var subDirs = dataDirectory.EnumerateDirectories();
                foreach(var subDir in subDirs)
                {
                    var osmFiles = subDir.EnumerateFiles("*.osm").Concat(
                        subDir.EnumerateFiles("*.osm.pbf")).ToArray();
                    if (osmFiles.Length > 0)
                    {
                        var thread = new Thread((state) =>
                        {
                            var localDirectory = state as DirectoryInfo;

                            if (Bootstrapper.LoadInstanceFromFolder(localDirectory))
                            {
                                var monitor = new FilesMonitor<DirectoryInfo>((f) =>
                                {
                                    return Bootstrapper.LoadInstanceFromFolder(f);
                                }, localDirectory);
                                monitor.Start();
                                // add osm and osm-pbf files.
                                foreach(var osmFile in osmFiles)
                                {
                                    monitor.AddFile(osmFile.FullName);
                                }
                                foreach(var luaFile in subDir.EnumerateFiles("*.lua"))
                                {
                                    monitor.AddFile(luaFile.FullName);
                                }
                                _fileMonitors.Add(monitor);
                            }
                        });
                        thread.Start(subDir);
                    }
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

                Logger.Log("Bootstrapper", TraceEventType.Information,
                    "Loading instance {1} from: {0}", file.FullName, file.Name.GetNameUntilFirstDot());

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
        /// Loads an instance from a folder.
        /// </summary>
        /// <returns></returns>
        public static bool LoadInstanceFromFolder(DirectoryInfo folder)
        {
            try
            {
                if (!folder.Exists)
                {
                    return false;
                }

                Logger.Log("Bootstrapper", TraceEventType.Information,
                    "Loading instance {1} from: {0}", folder.FullName, folder.Name);

                var profiles = new List<Itinero.Profiles.Vehicle>();
                foreach (var luaFile in folder.EnumerateFiles("*.lua"))
                {
                    try
                    {
                        using (var stream = luaFile.OpenRead())
                        {
                            profiles.Add(Itinero.Profiles.DynamicVehicle.LoadFromStream(stream));
                        }

                        Logger.Log("Bootstrapper", TraceEventType.Information,
                            "Loaded profile {0}.", luaFile.FullName);
                    }
                    catch(Exception ex)
                    {
                        Logger.Log("Bootstrapper", TraceEventType.Error,
                            "Failed loading profile {0}:{1}", luaFile.FullName, ex.ToInvariantString());
                    }
                }

                if (profiles.Count == 0)
                {
                    Logger.Log("Bootstrapper", TraceEventType.Information,
                        "Loading instance {1} from: {0}, no vehicle profiles found or they could not be loaded.", folder.FullName, 
                        folder.Name);
                    return true;
                }

                var osmFile = folder.EnumerateFiles("*.osm").Concat(
                        folder.EnumerateFiles("*.osm.pbf")).First();
                var routerDb = new RouterDb();
                using (var osmFileStream = osmFile.OpenRead())
                {
                    OsmStreamSource source;
                    if (osmFile.FullName.EndsWith(".osm"))
                    {
                        source = new XmlOsmStreamSource(osmFileStream);
                    }
                    else
                    {
                        source = new PBFOsmStreamSource(osmFileStream);
                    }

                    routerDb.LoadOsmData(source, profiles.ToArray());
                }
                var multimodalDb = new MultimodalDb(routerDb, new TransitDb());
                var multimodalRouter = new MultimodalRouter(multimodalDb,
                    Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());
                var instance = new Instances.Instance(multimodalRouter);
                InstanceManager.Register(folder.Name, instance);

                Logger.Log("Bootstrapper", TraceEventType.Information,
                    "Loaded instance {1} from: {0}", folder.FullName, folder.Name);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Bootstrapper", TraceEventType.Critical,
                    "Failed to load instance {1} from: {0}, {2}", folder.FullName, folder.Name, ex.ToInvariantString());
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
