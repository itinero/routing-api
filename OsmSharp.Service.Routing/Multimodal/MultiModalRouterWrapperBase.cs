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

using OsmSharp.Geo.Features;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Transit.Multimodal;
using OsmSharp.Routing.Transit.Multimodal.Data;
using OsmSharp.Routing.Vehicles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Service.Routing.Multimodal
{
    /// <summary>
    /// Represents an abstract wrapper of functionality around a multi modal transit routing service.
    /// </summary>
    public class MultimodalRouterWrapperBase
    {
        private MultimodalConnectionsDb _connectionsDb;

        /// <summary>
        /// Calculates a route from/to passing by at least one of the intermediates using two different modi.
        /// </summary>
        /// <param name="vehicles"></param>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public Route GetRouteAlongOne(List<Vehicle> vehicles, GeoCoordinate[] coordinates)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates a route along the given points.
        /// </summary>
        /// <param name="departureTime">The departure time at the first location.</param>
        /// <param name="vehicles">The vehicle profiles to use.</param>
        /// <param name="coordinates">The coordinates of the points to route along.</param>
        /// <param name="operators">The operators to allow. All operators are allowed when null, none when empty.</param>
        /// <param name="complete">Only output the route geometry if false.</param>
        /// <returns></returns>
        public Route GetRoute(DateTime departureTime, List<Vehicle> vehicles, GeoCoordinate[] coordinates, HashSet<string> operators, bool complete)
        {
            var router = new EarliestArrivalRouter(_connectionsDb, new OsmRoutingInterpreter(), departureTime, vehicles[0], 
                coordinates[0], vehicles[1], coordinates[1], x => x * x);
            router.Run();
            if(router.HasSucceeded)
            {
                return router.BuildRoute();
            }
            return null;
        }

        /// <summary>
        /// Calculates routes from one point to a number of given other points.
        /// </summary>
        /// <param name="departureTime">The departure time at the first location.</param>
        /// <param name="vehicles">The vehicle profiles to use.</param>
        /// <param name="coordinates">The coordinates of the points to route along.</param>
        /// <param name="operators">The operators to allow. All operators are allowed when null, none when empty.</param>
        /// <param name="complete">Only output the route geometry if false.</param>
        /// <returns></returns>
        public Route[] GetOneToMany(DateTime departureTime, List<Vehicle> vehicles, GeoCoordinate[] coordinates, HashSet<string> operators, bool complete)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the weight to all the points within the given range.
        /// </summary>
        /// <param name="departureTime"></param>
        /// <param name="vehicles"></param>
        /// <param name="location"></param>
        /// <param name="max"></param>
        /// <param name="sampleZoom"></param>
        /// <returns></returns>
        public IEnumerable<Tuple<GeoCoordinate, ulong, double>> GetWithinRange(DateTime departureTime, List<Vehicle> vehicles, GeoCoordinate location, double max, int sampleZoom)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates instructions for the given route and vehicle.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public List<Instruction> GetInstructions(Route route)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the given route to a feature collection.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="aggregated"></param>
        public FeatureCollection GetFeatures(Route route, bool aggregated = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the given route to a feature collection augmented with instructions.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public FeatureCollection GetFeaturesWithInstructions(Route route)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns all networkfeatures in the given box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public FeatureCollection GetNeworkFeatures(GeoCoordinateBox box)
        {
            throw new NotImplementedException();
        }
    }
}
