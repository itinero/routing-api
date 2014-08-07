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

using OsmSharp.Routing.Transit.MultiModal;
using OsmSharp.Service.Routing.MultiModal.Wrappers;
using System;
using System.Collections.Generic;

namespace OsmSharp.Service.Routing.MultiModal
{
    /// <summary>
    /// A boot strapper to bootstrap the multi modal API module.
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// Holds the multi modal router service instances.
        /// </summary>
        private static Dictionary<string, MultiModalRouterWrapperBase> _multiModalWrapperInstances = new Dictionary<string,MultiModalRouterWrapperBase>();

        /// <summary>
        /// Returns true if a multi modal router has been initialized.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <returns></returns>
        public static bool IsActive(string instance)
        {
            return _multiModalWrapperInstances != null &&
                _multiModalWrapperInstances.ContainsKey(instance);
        }

        /// <summary>
        /// Returns the transit service instance.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        public static MultiModalRouterWrapperBase Get(string instance)
        {
            return _multiModalWrapperInstances[instance];
        }

        /// <summary>
        /// Initializes the multi modal router service.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <param name="multiModalWrapperInstance"></param>
        public static void Add(string instance, MultiModalRouterWrapperBase multiModalWrapperInstance)
        {
            _multiModalWrapperInstances.Add(instance, multiModalWrapperInstance);
        }

        /// <summary>
        /// Initializes this multi modal API with an existing multi modal router.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <param name="transitRouter"></param>
        public static void Initialize(string instance, MultiModalRouter multiModalRouter)
        {
            Bootstrapper.Add(instance, new MultiModalWrapper(multiModalRouter));
        }
    }
}