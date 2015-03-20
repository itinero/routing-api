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

using NetTopologySuite.Features;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using System.Collections.Generic;

namespace OsmSharp.Service.Routing
{
    /// <summary>
    /// An abstract representation of routing service wrapper.
    /// </summary>
    public abstract class RoutingServiceWrapperBase
    {
        /// <summary>
        /// Calculates a route along the given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile to use.</param>
        /// <param name="coordinates">The coordinates of the points to route along.</param>
        /// <param name="complete">Only output the route geometry if false.</param>
        /// <returns></returns>
        public abstract Route GetRoute(Vehicle vehicle, GeoCoordinate[] coordinates, bool complete);

        /// <summary>
        /// Calculates instructions for the given route and vehicle.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        public abstract List<Instruction> GetInstructions(Vehicle vehicle, Route route);

        /// <summary>
        /// Converts the given route to a feature collection.
        /// </summary>
        /// <param name="route"></param>
        public abstract FeatureCollection GetFeatures(Route route);

        /// <summary>
        /// Returns all networkfeatures in the given box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public abstract FeatureCollection GetNeworkFeatures(GeoCoordinateBox box);

        /// <summary>
        /// Returns true when the given vehicle is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public abstract bool SupportsVehicle(Vehicle vehicle);
    }
}
