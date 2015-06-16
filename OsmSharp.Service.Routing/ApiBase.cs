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
using OsmSharp.Routing.Vehicles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Service.Routing
{
    /// <summary>
    /// An abstract representation of the entire API.
    /// </summary>
    public abstract class ApiBase
    {
        /// <summary>
        /// Returns true if transit queries are supported.
        /// </summary>
        public abstract bool TransitSupport { get; }

        /// <summary>
        /// Calculates a route along the given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile to use.</param>
        /// <param name="coordinates">The coordinates of the points to route along.</param>
        /// <param name="complete">Only output the route geometry if false.</param>
        /// <param name="sort">Sorts the via-points.</param>
        /// <returns></returns>
        public abstract Route GetRoute(Vehicle vehicle, GeoCoordinate[] coordinates, bool complete, bool sort);

        /// <summary>
        /// Returns a concatenated route along all coordinates with the given vehicles.
        /// </summary>
        /// <param name="vehicles"></param>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public abstract Route GetRouteAlongOne(List<Vehicle> vehicles, GeoCoordinate[] coordinates);

        /// <summary>
        /// Calculates routes from one source to many targets.
        /// </summary>
        /// <param name="vehicle">The vehicle profile to use.</param>
        /// <param name="coordinates">The coordinates of the points to route along.</param>
        /// <param name="complete">Only output the route geometry if false.</param>
        /// <returns></returns>
        public abstract Route[] GetOneToMany(Vehicle vehicle, GeoCoordinate[] coordinates, bool complete);

        /// <summary>
        /// Calculates weight matrices.
        /// </summary>
        /// <returns></returns>
        public abstract Tuple<string, double[][]>[] GetMatrix(Vehicle vehicle, GeoCoordinate[] source, GeoCoordinate[] target, string[] outputs,
            out Tuple<string, int, string>[] errors);

        /// <summary>
        /// Calculates instructions for the given route and vehicle.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public abstract List<Instruction> GetInstructions( Route route);

        /// <summary>
        /// Converts the given route to a feature collection.
        /// </summary>
        /// <param name="route"></param>
        public abstract FeatureCollection GetFeatures(Route route);

        /// <summary>
        /// Converts the given route to a feature collection augmented with instructions.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public abstract FeatureCollection GetFeaturesWithInstructions(Route route);

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

        /// <summary>
        /// Returns a number of transit routes from one point to many others.
        /// </summary>
        /// <returns></returns>
        public abstract Route[] GetTransitOneToMany(DateTime dt, List<Vehicle> vehicles, GeoCoordinate[] coordinates, HashSet<string> operators, bool complete);

        /// <summary>
        /// Returns all points on a grid within a given range.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<Tuple<GeoCoordinate, ulong, double>> GetWithinRange(DateTime dt, List<Vehicle> vehicles, GeoCoordinate geoCoordinate, int max, int zoom);

        /// <summary>
        /// Returns a transit route.
        /// </summary>
        public abstract Route GetTransitRoute(DateTime dt, List<Vehicle> vehicles, GeoCoordinate[] coordinates, HashSet<string> operators, bool complete);

        /// <summary>
        /// Returns features for a route taking into account transit-data.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        public abstract FeatureCollection GetTransitFeatures(Route route, bool aggregate);
    }
}
