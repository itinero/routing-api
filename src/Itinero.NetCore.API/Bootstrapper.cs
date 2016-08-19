// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Itinero.Osm.Vehicles;
using Itinero.Logging;
using System;
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
        public static void BootFromConfiguration(string dataPath)
        {
            try
            {
                // register vehicle profiles.
                Vehicle.RegisterVehicles();

                // load all .routing files.
               
                var dataDirectory = new DirectoryInfo(dataPath);
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
                    var thread = new Thread(state =>
                    {
                        var j = (int)state;
                        var name = routingFiles[j].Name.GetNameUntilFirstDot();
                        try
                        {
                            RouterDb routerDb;
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
    }
}