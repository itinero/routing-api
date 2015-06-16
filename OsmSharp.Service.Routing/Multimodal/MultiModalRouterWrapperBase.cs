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
using OsmSharp.Service.Routing.Wrappers;
using System;
using System.Collections.Generic;

namespace OsmSharp.Service.Routing.Multimodal
{
    /// <summary>
    /// Represents an abstract wrapper of functionality around a multi modal transit routing service.
    /// </summary>
    public class MultimodalRouterWrapperBase : ApiBase
    {
        private MultimodalConnectionsDb _connectionsDb;

        private RouterWrapper _routerApi;
        
        /// <summary>
        /// Creates a new multimodal router wrapper.
        /// </summary>
        /// <param name="connectionsDb"></param>
        public MultimodalRouterWrapperBase(MultimodalConnectionsDb connectionsDb)
        {
            _connectionsDb = connectionsDb;

            _routerApi = new RouterWrapper(Router.CreateFrom(connectionsDb.Graph, new OsmRoutingInterpreter()));
        }

        /// <summary>
        /// Calculates a route from/to passing by at least one of the intermediates using two different modi.
        /// </summary>
        /// <param name="vehicles"></param>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public override Route GetRouteAlongOne(List<Vehicle> vehicles, GeoCoordinate[] coordinates)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates a route along the given points.
        /// </summary>
        /// <returns></returns>
        public override Route GetTransitRoute(DateTime departureTime, List<Vehicle> vehicles, GeoCoordinate[] coordinates, 
            HashSet<string> operators, bool complete)
        {
            var router = new EarliestArrivalRouter(_connectionsDb, new OsmRoutingInterpreter(), departureTime, vehicles[0],
                coordinates[0], vehicles[1], coordinates[1], x => x * x);
            router.Run();
            if (router.HasSucceeded)
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
            return _routerApi.GetOneToMany(vehicles[0], coordinates, complete);
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
            return _routerApi.GetWithinRange(departureTime, vehicles, location, (int)max, sampleZoom);
        }

        /// <summary>
        /// Calculates instructions for the given route and vehicle.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public override List<Instruction> GetInstructions(Route route)
        {
            return _routerApi.GetInstructions(route);
        }

        /// <summary>
        /// Converts the given route to a feature collection augmented with instructions.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public override FeatureCollection GetFeaturesWithInstructions(Route route)
        {
            return _routerApi.GetFeaturesWithInstructions(route);
        }

        /// <summary>
        /// Returns all networkfeatures in the given box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public override FeatureCollection GetNeworkFeatures(GeoCoordinateBox box)
        {
            return _routerApi.GetNeworkFeatures(box);
        }

        public override bool TransitSupport
        {
            get { return true; }
        }

        public override Route GetRoute(Vehicle vehicle, GeoCoordinate[] coordinates, bool complete, bool sort)
        {
            return _routerApi.GetRoute(vehicle, coordinates, complete, sort);
        }

        public override Route[] GetOneToMany(Vehicle vehicle, GeoCoordinate[] coordinates, bool complete)
        {
            return _routerApi.GetOneToMany(vehicle, coordinates, complete);
        }

        public override Tuple<string, double[][]>[] GetMatrix(Vehicle vehicle, GeoCoordinate[] source, GeoCoordinate[] target, string[] outputs, out Tuple<string, int, string>[] errors)
        {
            return _routerApi.GetMatrix(vehicle, source, target, outputs, out errors);
        }

        public override FeatureCollection GetFeatures(Route route)
        {
            return _routerApi.GetFeatures(route);
        }

        public override bool SupportsVehicle(Vehicle vehicle)
        {
            return true;
        }

        public override Route[] GetTransitOneToMany(DateTime dt, List<Vehicle> vehicles, GeoCoordinate[] coordinates, HashSet<string> operators, bool complete)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Tuple<GeoCoordinate, ulong, double>> GetWithinRange(DateTime dt, List<Vehicle> vehicles, GeoCoordinate geoCoordinate, int max, int zoom)
        {
            throw new NotImplementedException();
        }

        public override FeatureCollection GetTransitFeatures(Route route, bool aggregate)
        {
            throw new NotImplementedException();
        }
    }
}
