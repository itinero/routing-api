// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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

using OsmSharp.Routing;
using OsmSharp.Service.Routing.Wrappers;
using System;
using System.Collections.Generic;

namespace OsmSharp.Service.Routing
{
    /// <summary>
    /// A boot strapper to bootstrap the routing API module.
    /// </summary>
    public static class Bootstrapper
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
        /// Initializes this router API with an existing router.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <param name="router"></param>
        public static void Add(string instance, Router router)
        {
            // make sure vehicle are registered.
            Vehicle.RegisterVehicles();

            Bootstrapper.Add(instance, new RouterWrapper(router));
        }
    }
}