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

using OsmSharp.Routing.Transit;
using OsmSharp.Service.Routing.Transit.Wrappers;
using System;
using System.Collections.Generic;

namespace OsmSharp.Service.Routing.Transit
{
    /// <summary>
    /// A boot strapper to bootstrap the transit API module.
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// Holds the transit service instances.
        /// </summary>
        private static Dictionary<string, TransitServiceWrapperBase> _transitServiceInstances = new Dictionary<string,TransitServiceWrapperBase>();

        /// <summary>
        /// Returns true if a transit service has been initialized.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <returns></returns>
        public static bool IsActive(string instance)
        {
            return _transitServiceInstances != null &&
                _transitServiceInstances.ContainsKey(instance);
        }

        /// <summary>
        /// Returns the transit service instance.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        public static TransitServiceWrapperBase Get(string instance)
        {
            return _transitServiceInstances[instance];
        }

        /// <summary>
        /// Initializes the transit service.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <param name="transitServiceInstance"></param>
        public static void Add(string instance, TransitServiceWrapperBase transitServiceInstance)
        {
            _transitServiceInstances.Add(instance, transitServiceInstance);
        }

        /// <summary>
        /// Initializes this transit API with an existing transit router.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <param name="transitRouter"></param>
        public static void Add(string instance, TransitRouter transitRouter)
        {
            Bootstrapper.Add(instance, new TransitRouterWrapper(transitRouter));
        }
    }
}