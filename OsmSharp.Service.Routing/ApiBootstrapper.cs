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

using OsmSharp.Osm.PBF.Streams;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.CH;
using OsmSharp.Routing.CH.Serialization;
using OsmSharp.Routing.CH.Serialization.Sorted;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Osm.Graphs.Serialization;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Service.Routing.Configurations;
using OsmSharp.Service.Routing.Monitoring;
using OsmSharp.Service.Routing.Wrappers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace OsmSharp.Service.Routing
{
    /// <summary>
    /// A boot strapper to bootstrap the routing API module.
    /// </summary>
    public static class ApiBootstrapper
    {
        /// <summary>
        /// Holds the routing service instances.
        /// </summary>
        private static Dictionary<string, RoutingServiceWrapperBase> _routingServiceInstances = new Dictionary<string,RoutingServiceWrapperBase>();

        /// <summary>
        /// Returns true if a routing service has been initialized.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <returns></returns>
        public static bool IsActive(string instance)
        {
            return _routingServiceInstances != null &&
                _routingServiceInstances.ContainsKey(instance);
        }

        /// <summary>
        /// Returns the routing service instance.
        /// </summary>
        public static RoutingServiceWrapperBase Get(string instance)
        {
            return _routingServiceInstances[instance];
        }

        /// <summary>
        /// Initializes the routing service.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <param name="routingServiceInstance"></param>
        public static void Add(string instance, RoutingServiceWrapperBase routingServiceInstance)
        {
            _routingServiceInstances.Add(instance, routingServiceInstance);
        }

        /// <summary>
        /// Initializes or updates the routing service.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <param name="routingServiceInstance"></param>
        public static void AddOrUpdate(string instance, RoutingServiceWrapperBase routingServiceInstance)
        {
            _routingServiceInstances[instance] = routingServiceInstance;
        }

        /// <summary>
        /// Initializes this router API with an existing router.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <param name="router"></param>
        public static void Add(string instance, Router router)
        {
            // make sure vehicle are registered.
            Vehicle.RegisterVehicles();

            ApiBootstrapper.Add(instance, new RouterWrapper(router));
        }

        /// <summary>
        /// Initializes or updates this router API with an existing router.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <param name="router"></param>
        public static void AddOrUpdate(string instance, Router router)
        {
            // make sure vehicle are registered.
            Vehicle.RegisterVehicles();

            ApiBootstrapper.AddOrUpdate(instance, new RouterWrapper(router));
        }

        /// <summary>
        /// Holds all instance monitors.
        /// </summary>
        private static List<InstanceMonitor> _instanceMonitors = new List<InstanceMonitor>();

        /// <summary>
        /// A delegate to load a new instance configuration.
        /// </summary>
        /// <param name="apiConfiguration"></param>
        /// <param name="instanceConfiguration"></param>
        /// <returns></returns>
        internal delegate bool InstanceLoaderDelegate(ApiConfiguration apiConfiguration, InstanceConfiguration instanceConfiguration);

        /// <summary>
        /// Initializes all routing instance from the configuration in the configuration file.
        /// </summary>
        public static void BootFromConfiguration()
        {
            // register vehicle profiles.
            OsmSharp.Routing.Vehicle.RegisterVehicles();

            // enable logging and use the console as output.
            OsmSharp.Logging.Log.Enable();
            OsmSharp.Logging.Log.RegisterListener(
                new OsmSharp.WinForms.UI.Logging.ConsoleTraceListener());
#if DEBUG
            OsmSharp.Logging.Log.RegisterListener(
                new Logging.DebugTraceListener());
#endif

            // get the api configuration.
            var apiConfiguration = (ApiConfiguration)ConfigurationManager.GetSection("ApiConfiguration");

            // load all relevant routers.
            foreach (InstanceConfiguration instanceConfiguration in apiConfiguration.Instances)
            {
                var thread = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                {
                    // load instance.
                    if (LoadInstance(apiConfiguration, instanceConfiguration))
                    { // instance loaded correctly.
                        // start monitoring files...
                        if (instanceConfiguration.Monitor)
                        { // ...but only when configured as such.
                            var monitor = new InstanceMonitor(apiConfiguration, instanceConfiguration, LoadInstance);
                            _instanceMonitors.Add(monitor);

                            // get graph configuration.
                            var graph = instanceConfiguration.Graph;
                            monitor.AddFile(graph);
                            monitor.Start();
                        }
                    }
                }));
                thread.Start();
            }
        }

        /// <summary>
        /// Holds a sync object.
        /// </summary>
        private static object _sync = new object();

        /// <summary>
        /// Loads a new instance.
        /// </summary>
        /// <param name="apiConfiguration"></param>
        /// <param name="instanceConfiguration"></param>
        /// <returns></returns>
        private static bool LoadInstance(ApiConfiguration apiConfiguration, InstanceConfiguration instanceConfiguration)
        {
            try
            {
                // get graph configuration.
                var graph = instanceConfiguration.Graph;
                var type = instanceConfiguration.Type;
                var format = instanceConfiguration.Format;
                var vehicleName = instanceConfiguration.Vehicle;

                try
                {
                    // create routing instance.
                    OsmSharp.Logging.Log.TraceEvent("Bootstrapper", OsmSharp.Logging.TraceEventType.Information,
                        string.Format("Creating {0} instance...", instanceConfiguration.Name));
                    Router router = null;

                    var graphFile = new FileInfo(graph);
                    switch (type)
                    {
                        case "raw":
                            switch (format)
                            {
                                case "osm-xml":
                                    using(var graphStream = graphFile.OpenRead())
                                    {
                                        var graphSource = new XmlOsmStreamSource(graphStream);
                                        router = Router.CreateLiveFrom(graphSource, new OsmRoutingInterpreter());
                                    }
                                    break;
                                case "osm-pbf":
                                    using (var graphStream = graphFile.OpenRead())
                                    {
                                        var graphSource = new PBFOsmStreamSource(graphStream);
                                        router = Router.CreateLiveFrom(graphSource, new OsmRoutingInterpreter());
                                    }
                                    break;
                                default:
                                    throw new Exception(string.Format("Invalid format {0} for type {1}.",
                                        format, type));
                            }
                            break;
                        case "contracted":
                            switch(format)
                            {
                                case "osm-xml":
                                    if (string.IsNullOrWhiteSpace(vehicleName))
                                    { // invalid configuration.
                                        throw new Exception("Invalid configuration, a vehicle type is required when building contracted graphs on-the-fly.");
                                    }
                                    using (var graphStream = graphFile.OpenRead())
                                    {
                                        var graphSource = new XmlOsmStreamSource(graphStream);
                                        router = Router.CreateCHFrom(graphSource, new OsmRoutingInterpreter(), Vehicle.GetByUniqueName(vehicleName));
                                    }
                                    break;
                                case "osm-pbf":
                                    if (string.IsNullOrWhiteSpace(vehicleName))
                                    { // invalid configuration.
                                        throw new Exception("Invalid configuration, a vehicle type is required when building contracted graphs on-the-fly.");
                                    }
                                    using (var graphStream = graphFile.OpenRead())
                                    {
                                        var graphSource = new PBFOsmStreamSource(graphStream);
                                        router = Router.CreateCHFrom(graphSource, new OsmRoutingInterpreter(), Vehicle.GetByUniqueName(vehicleName));
                                    }
                                    break;
                                case "flat":
                                    using (var graphStream = graphFile.OpenRead())
                                    {
                                        var routingSerializer = new CHEdgeFlatfileSerializer();
                                        var graphInstance = routingSerializer.Deserialize(graphStream);
                                        router = Router.CreateCHFrom(graphInstance, new CHRouter(), new OsmRoutingInterpreter());
                                    }
                                    break;
                                case "mobile":
                                    var mobileRoutingSerializer = new CHEdgeDataDataSourceSerializer();
                                    // keep this stream open, it is used while routing!
                                    var mobileGraphInstance = mobileRoutingSerializer.Deserialize(graphFile.OpenRead());
                                    router = Router.CreateCHFrom(mobileGraphInstance, new CHRouter(), new OsmRoutingInterpreter());
                                    break;
                                default:
                                    throw new Exception(string.Format("Invalid format {0} for type {1}.",
                                        format, type));
                            }
                            break;
                        case "simple":
                            switch (format)
                            {
                                case "flat":
                                    using (var graphStream = graphFile.OpenRead())
                                    {
                                        var routingSerializer = new LiveEdgeFlatfileSerializer();
                                        var graphInstance = routingSerializer.Deserialize(graphStream);
                                        router = Router.CreateLiveFrom(graphInstance, new Dykstra(), new OsmRoutingInterpreter());
                                    }
                                    break;
                                default:
                                    throw new Exception(string.Format("Invalid format {0} for type {1}.",
                                        format, type));
                            }
                            break;
                    }

                    lock (_sync)
                    {
                        OsmSharp.Service.Routing.ApiBootstrapper.AddOrUpdate(instanceConfiguration.Name, router);
                    }

                    OsmSharp.Logging.Log.TraceEvent("Bootstrapper", OsmSharp.Logging.TraceEventType.Information,
                        string.Format("Instance {0} created successfully!", instanceConfiguration.Name));
                }
                catch(Exception ex)
                {
                    OsmSharp.Logging.Log.TraceEvent("Bootstrapper", OsmSharp.Logging.TraceEventType.Error,
                        string.Format("Exception occured while creating instance {0}:{1}", 
                        instanceConfiguration.Name, ex.ToInvariantString()));
                    throw;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}