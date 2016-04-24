using Nancy;
using Itinero.Osm.Vehicles;
using Itinero.Logging;
using System;
using System.Configuration;
using System.IO;
using System.Threading;

namespace Itinero.API
{
    /// <summary>
    /// A bootstrapper.
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// Initializes all routing instance from the configuration in the configuration file.
        /// </summary>
        public static void BootFromConfiguration()
        {
            try
            {
                // register vehicle profiles.
                Vehicle.RegisterVehicles();

                // enable logging and use the console as output.
                OsmSharp.Logging.Logger.LogAction = (origin, level, message, parameters) =>
                {
                    Console.WriteLine("{0}:{1} - {2}", origin, level, message);
                };
                Logger.LogAction = (origin, level, message, parameters) =>
                {
                    Console.WriteLine("{0}:{1} - {2}", origin, level, message);
                };

                // load all .routing files.
                var dataDirectory = new DirectoryInfo(Properties.Settings.Default.Data);
                if (!dataDirectory.Exists)
                {
                    throw new DirectoryNotFoundException(
                        string.Format("Configured data directory doesn't exist: {0}", dataDirectory.FullName));
                }

                // load all relevant files.
                var routingFiles = dataDirectory.GetFiles("*.routing");
                if (routingFiles.Length == 0)
                {
                    throw new DirectoryNotFoundException(
                        string.Format("No .routing files found in {0}", dataDirectory.FullName));
                }

                for(var i = 0; i < routingFiles.Length; i++)
                {
                    var thread = new Thread((state) =>
                    {
                        var j = (int)state;
                        var name = routingFiles[j].Name.GetNameUntilFirstDot();
                        try
                        {
                            RouterDb routerDb = null;
                            using (var stream = routingFiles[j].OpenRead())
                            {
                                routerDb = RouterDb.Deserialize(stream);
                            }
                            var instance = new Instances.DefaultRoutingModuleInstance(new Router(routerDb));
                            RoutingBootstrapper.Register(name, instance);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Bootstrapper", TraceEventType.Critical,
                                "Failed load file or create instance: {0}", ex.ToInvariantString());
                        }
                    });
                    thread.Start(i);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Bootstrapper", TraceEventType.Critical, 
                    "Failed to start service: {0}", ex.ToInvariantString());
            }
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

        /// <summary>
        /// A function to validate requests.
        /// </summary>
        public static Func<NancyModule, dynamic, bool> ValidateRequest = (m, _) =>
        {
            return true;
        };
    }
}