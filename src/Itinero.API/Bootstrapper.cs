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
using Itinero.API.Routing;

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
        public static void BootFromConfiguration(string routingFilePath)
        {
            try
            {
                // register vehicle profiles.
                Vehicle.RegisterVehicles();

                LoadRouterDbOnThread(routingFilePath);
            }
            catch (Exception ex)
            {
                Logger.Log("Bootstrapper", TraceEventType.Critical, 
                    "Failed to start service: {0}", ex.ToInvariantString());
            }
        }

        public static void LoadRouterDbOnThread(string routingFilePath)
        {
            var thread = new Thread(state =>
            {
                try
                {
                    RouterDb routerDb;

                    using (var stream = File.OpenRead(routingFilePath))
                    {
                        routerDb = RouterDb.Deserialize(stream);
                    }
                    var instance = new DefaultRoutingModuleInstance(new Router(routerDb));
                    RoutingInstances.Register(routingFilePath, instance);
                }
                catch (Exception ex)
                {
                    Logger.Log("Bootstrapper", TraceEventType.Critical,
                        "Failed load file or create instance: {0}", ex.ToInvariantString());
                }
            });
            thread.Start();
        }
    }
}