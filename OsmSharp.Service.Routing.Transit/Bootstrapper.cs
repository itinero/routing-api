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

namespace OsmSharp.Service.Routing.Transit
{
    /// <summary>
    /// A boot strapper to bootstrap the transit API module.
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// Holds the transit service instance.
        /// </summary>
        private static TransitServiceWrapperBase _transitServiceInstance;

        /// <summary>
        /// Returns true if a transit service has been initialized.
        /// </summary>
        /// <returns></returns>
        public static bool IsInitialized()
        {
            return _transitServiceInstance != null;
        }

        /// <summary>
        /// Returns the transit service instance.
        /// </summary>
        public static TransitServiceWrapperBase TransitServiceInstance
        {
            get
            {
                if(_transitServiceInstance == null)
                {
                    throw new InvalidOperationException("Bootstrapper was not initialized!");
                }
                return _transitServiceInstance;
            }
        }

        /// <summary>
        /// Initializes the transit service.
        /// </summary>
        /// <param name="transitServiceInstance"></param>
        public static void Initialize(TransitServiceWrapperBase transitServiceInstance)
        {
            _transitServiceInstance = transitServiceInstance;
        }

        /// <summary>
        /// Initializes this transit API with an existing transit router.
        /// </summary>
        /// <param name="transitRouter"></param>
        public static void Initialize(TransitRouter transitRouter)
        {
            _transitServiceInstance = new TransitRouterWrapper(transitRouter);
        }
    }
}