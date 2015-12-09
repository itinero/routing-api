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

using OsmSharp.Routing.API.Configurations;
using OsmSharp.Routing.Osm.Vehicles;
using OsmSharp.Routing.Profiles;
using System.Configuration;
using System.IO;

namespace OsmSharp.Routing.API
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
            // register vehicle profiles.
            Vehicle.RegisterVehicles();

            // enable logging and use the console as output.
            OsmSharp.Logging.Log.Enable();
            OsmSharp.Logging.Log.RegisterListener(
                new OsmSharp.WinForms.UI.Logging.ConsoleTraceListener());

            // get the api configuration.
            var apiConfiguration = (ApiConfiguration)ConfigurationManager.GetSection("ApiConfiguration");

            // load all relevant routers.
            foreach (InstanceConfiguration instanceConfiguration in apiConfiguration.Instances)
            {
                var thread = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                {                   
                    // create routing instance.
                    OsmSharp.Logging.Log.TraceEvent("Bootstrapper", OsmSharp.Logging.TraceEventType.Information,
                        string.Format("Creating {0} instance...", instanceConfiguration.Name));

                    // load data.
                    RouterDb routerDb = null;
                    using(var stream = new FileInfo(instanceConfiguration.RouterDb).OpenRead())
                    {
                        routerDb = RouterDb.Deserialize(stream);
                    }

                    // register instance.
                    Routing.RoutingBootstrapper.Register(instanceConfiguration.Name,
                        new Routing.Instances.DefaultRoutingModuleInstance(new Router(routerDb)));

                    OsmSharp.Logging.Log.TraceEvent("Bootstrapper", OsmSharp.Logging.TraceEventType.Information,
                        string.Format("Instance {0} created successfully!", instanceConfiguration.Name));
                }));
                thread.Start();
            }
        }
    }
}