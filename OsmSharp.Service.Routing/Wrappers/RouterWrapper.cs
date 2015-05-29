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

using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Math.VRP.Core.Routes;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.TSP.Genetic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Service.Routing.Wrappers
{
    /// <summary>
    /// A routing service wrapper class around a router.
    /// </summary>
    public class RouterWrapper : ApiBase
    {
        /// <summary>
        /// Holds the router.
        /// </summary>
        private Router _router;

        /// <summary>
        /// Creates a new router wrapper.
        /// </summary>
        /// <param name="router"></param>
        public RouterWrapper(Router router)
        {
            _router = router;
        }

        /// <summary>
        /// Calculates a number of routes from one source to many targets.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinates"></param>
        /// <param name="complete"></param>
        public override Route[] GetOneToMany(Vehicle vehicle, GeoCoordinate[] coordinates, bool complete)
        {
            // resolve all points.
            var resolved = _router.Resolve(vehicle, coordinates);

            // calculate one-to-many routes.
            if (resolved[0] != null)
            {
                var targets = new List<RouterPoint>();
                foreach(var point in resolved)
                {
                    if (point != null)
                    {
                        targets.Add(point);
                    }
                }

                return _router.CalculateOneToMany(vehicle, resolved[0], targets.ToArray());
            }
            return new Route[coordinates.Length - 1];
        }

        /// <summary>
        /// Calculates a router along the given coordinates.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinates"></param>
        /// <param name="complete"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public override Route GetRoute(Vehicle vehicle, GeoCoordinate[] coordinates, bool complete, bool sort)
        {
            // resolve all points.
            var resolved = _router.Resolve(vehicle, 0.0075f, coordinates);

            // add id's to each resolved point.
            for(int idx = 0; idx < resolved.Length; idx++)
            {
                if(resolved[idx] != null)
                {
                    if (resolved[idx].Tags == null)
                    {
                        resolved[idx].Tags = new List<KeyValuePair<string, string>>();
                    }
                    resolved[idx].Tags.Add(new KeyValuePair<string, string>("point_id", idx.ToInvariantString()));
                }
            }

            Route route;
            if (sort)
            { 
                // filter out all points that failed to resolve.
                var nonNullResolved = new List<RouterPoint>();
                var nonNullOffset = new List<int>();
                for (var idx = 0; idx < resolved.Length; idx++)
                {
                    if(resolved[idx] != null)
                    {
                        nonNullResolved.Add(resolved[idx]);
                        nonNullOffset.Add(nonNullOffset.Count - idx);
                    }
                }

                // calculate weights.
                var nonNullResolvedArray = nonNullResolved.ToArray();
                var nonNullWeights = _router.CalculateManyToManyWeight(vehicle, nonNullResolvedArray, nonNullResolvedArray);

                // expand matrix.
                var weights = new double[resolved.Length][];
                for (int nonNullRow = 0; nonNullRow < nonNullResolvedArray.Length; nonNullRow++)
                {
                    var row = nonNullRow + nonNullOffset[nonNullRow];
                    weights[row] = new double[resolved.Length];
                    for (int nonNullColumn = 0; nonNullColumn < nonNullResolvedArray.Length; nonNullColumn++)
                    {
                        var column = nonNullColumn + nonNullOffset[nonNullColumn];
                        weights[row][column] = nonNullWeights[nonNullRow][nonNullColumn];
                    }
                }

                // augment the empty weights with weights 'as the crow flies' but with a penalty of a factor 10.
                for(var row = 0; row < resolved.Length;row++)
                {
                    if(weights[row] == null)
                    { // possible that there isn't a row here yet!
                        weights[row] = new double[resolved.Length];
                    }
                    for (var column = 0; column < resolved.Length; column++)
                    {
                        if(resolved[row] == null || resolved[column] == null)
                        {
                            var distance = coordinates[row].DistanceReal(coordinates[column]).Value;
                            weights[row][column] = distance / (vehicle.MaxSpeed().Value) * 3.6 * 10;
                        }
                    }
                }

                // build the locations array.
                var locations = new GeoCoordinate[resolved.Length];
                for (int idx = 0; idx < resolved.Length; idx++)
                {
                    if(resolved[idx] != null)
                    {
                        locations[idx] = resolved[idx].Location;
                    }
                    else
                    {
                        locations[idx] = coordinates[idx];
                    }
                } 
                
                // sort points.
                var routerTSP = new RouterTSPAEXGenetic();
                var isRound = true; //locations[0] == locations[locations.Length - 1];
                var tspSolution = routerTSP.CalculateTSP(weights, locations, isRound);

                // build route.
                route = this.BuildRoute(vehicle, resolved, coordinates, tspSolution, isRound);
            }
            else
            { // concatenate routes.
                route = this.BuildRoute(vehicle, resolved, coordinates);
            }

            // TODO: implement complete boolean.
            return route;
        }

        /// <summary>
        /// Builds a dummy route (as the crow flies) for segments of a route not found.
        /// </summary>
        /// <param name="coordinate1">The start-coordinate.</param>
        /// <param name="coordinate2">The end-coordinate.</param>
        /// <returns></returns>
        public virtual Route BuildDummyRoute(Vehicle vehicle, GeoCoordinate coordinate1, GeoCoordinate coordinate2)
        {
            var route = new Route();
            route.Vehicle = vehicle.UniqueName;

            var segments = new RouteSegment[2];
            segments[0] = new RouteSegment();
            segments[0].Distance = 0;
            segments[0].Time = 0;
            segments[0].Type = RouteSegmentType.Start;
            segments[0].Vehicle = vehicle.UniqueName;
            segments[0].Latitude = (float)coordinate1.Latitude;
            segments[0].Longitude = (float)coordinate1.Longitude;
            
            var distance = coordinate1.DistanceReal(coordinate2).Value;
            var timeEstimage = distance / (vehicle.MaxSpeed().Value) * 3.6;
            var tags = new TagsCollection();
            tags.Add("route", "not_found");
            segments[1] = new RouteSegment();
            segments[1].Distance = distance;
            segments[1].Time = timeEstimage;
            segments[1].Type = RouteSegmentType.Stop;
            segments[1].Vehicle = vehicle.UniqueName;
            segments[1].Latitude = (float)coordinate2.Latitude;
            segments[1].Longitude = (float)coordinate2.Longitude;
            segments[1].Tags = RouteTagsExtensions.ConvertFrom(tags);

            route.Segments = segments;

            return route;
        }

        /// <summary>
        /// Builds a route along all the given points.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="resolved"></param>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public Route BuildRoute(Vehicle vehicle, RouterPoint[] resolved, GeoCoordinate[] coordinates)
        {
            var routes = new Route[resolved.Length - 1];
            for (int idx = 1; idx < resolved.Length; idx++)
            {
                if (resolved[idx - 1] == null || resolved[idx] == null)
                { // failed to resolve point(s), replace with a dummy route.
                    routes[idx - 1] = null;
                }
                else
                { // both points are resolved, calculate route.
                    var localRoute = _router.Calculate(vehicle, resolved[idx - 1], resolved[idx]);
                    if (localRoute != null)
                    { // route was found.
                        routes[idx - 1] = localRoute;
                    }
                    else
                    { // failed to calculate route, replace with a dummy route.
                        routes[idx - 1] = null;
                    }
                }
            }

            // add dummy routes in place of routes that have not been found.
            for (int idx = 0; idx < routes.Length; idx++)
            {
                if (routes[idx] == null)
                { // the route is null here.
                    GeoCoordinate coordinate1, coordinate2;
                    if (idx > 0 &&
                        routes[idx - 1] != null)
                    { // there is a route before this one, take the last point of it as start of this one.
                        coordinate1 = new GeoCoordinate(
                            routes[idx - 1].Segments[routes[idx - 1].Segments.Length - 1].Latitude,
                            routes[idx - 1].Segments[routes[idx - 1].Segments.Length - 1].Longitude);
                    }
                    else
                    { // take the coordinate itself.
                        coordinate1 = coordinates[idx];
                    }
                    if (routes.Length > idx + 1 &&
                        routes[idx + 1] != null)
                    { // there is a route before this one, take the last point of it as start of this one.
                        coordinate2 = new GeoCoordinate(
                            routes[idx + 1].Segments[0].Latitude,
                            routes[idx + 1].Segments[0].Longitude);
                    }
                    else
                    { // take the coordinate itself.
                        coordinate2 = coordinates[idx + 1];
                    }

                    // build the dummy route.
                    routes[idx] = this.BuildDummyRoute(vehicle, coordinate1, coordinate2);
                }
            }

            // concatenate the routes.
            var route = routes[0];
            for (int idx = 1; idx < routes.Length; idx++)
            {
                route = Route.Concatenate(route, routes[idx]);
            }
            return route;
        }

        /// <summary>
        /// Builds a route along all the given point in the order given by the tsp solution.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="resolved"></param>
        /// <param name="coordinates"></param>
        /// <param name="tspSolution"></param>
        /// <param name="isRound"></param>
        /// <returns></returns>
        public Route BuildRoute(Vehicle vehicle, RouterPoint[] resolved, GeoCoordinate[] coordinates, IRoute tspSolution, bool isRound)
        {
            // sort resolved and coordinates.
            var solution = tspSolution.ToArray();
            var size = isRound ? solution.Length + 1 : solution.Length;
            var sortedResolved = new RouterPoint[size];
            var sortedCoordinates = new GeoCoordinate[size];
            for (int idx = 0; idx < solution.Length; idx++)
            {
                sortedResolved[idx] = resolved[solution[idx]];
                sortedCoordinates[idx] = coordinates[solution[idx]];
            }

            // make round if needed.
            if (isRound)
            {
                sortedResolved[size - 1] = sortedResolved[0];
                sortedCoordinates[size - 1] = sortedCoordinates[0];
            }

            // build the route.
            return this.BuildRoute(vehicle, sortedResolved, sortedCoordinates);
        }

        /// <summary>
        /// Calculates instructions for a given route.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public override List<Instruction> GetInstructions(Route route)
        {
            return InstructionGenerator.Generate(route, new OsmRoutingInterpreter());
        }

        /// <summary>
        /// Converts the given route to a line string.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public override FeatureCollection GetFeatures(Route route)
        {
            var featureCollection = new FeatureCollection();
            var coordinates = route.GetPoints();
            if (coordinates.Count > 1)
            {
                var ntsCoordinates = coordinates.Select(x => { return new Coordinate(x.Longitude, x.Latitude); });
                var geometryFactory = new GeometryFactory();
                var lineString = geometryFactory.CreateLineString(ntsCoordinates.ToArray());

                var attributes = new AttributesTable();
                attributes.AddAttribute("osmsharp:total_time", route.TotalTime.ToInvariantString());
                attributes.AddAttribute("osmsharp:total_distance", route.TotalDistance.ToInvariantString());

                var feature = new Feature(lineString, attributes);

                featureCollection.Add(feature);
            }
            return featureCollection;
        }

        /// <summary>
        /// Converts the given route to a line string.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="aggregated"></param>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public FeatureCollection GetFeatures(Route route, bool aggregated, IList<Instruction> instructions)
        {
            if (route == null) { throw new ArgumentNullException("route"); }
            var featureCollection = new FeatureCollection();
            var pointsCollection = new FeatureCollection();
            if (aggregated && instructions != null && instructions.Count > 0)
            { // aggregate the route per instruction.
                for (int i = 0; i < instructions.Count; i++)
                {
                    var instruction = instructions[i];
                    var coordinates = new List<GeoAPI.Geometries.Coordinate>();
                    for (int segmentIdx = instruction.FirstSegmentIdx; segmentIdx <= instruction.LastSegmentIdx; segmentIdx++)
                    {
                        coordinates.Add(new GeoAPI.Geometries.Coordinate(route.Segments[segmentIdx].Longitude, route.Segments[segmentIdx].Latitude));
                    }

                    // build attributes.
                    var currentArcTags = instruction.MetaData;
                    var attributesTable = new AttributesTable();
                    if (currentArcTags != null)
                    { // there are tags.
                        foreach (var tag in currentArcTags)
                        {
                            attributesTable.AddAttribute(tag.Key, tag.Value);
                        }
                    }

                    // build feature.
                    var lineString = new LineString(coordinates.ToArray());
                    featureCollection.Add(new Feature(lineString, attributesTable));

                    // build poi-features if any.
                    if (instruction.Pois != null)
                    {
                        foreach (var poi in instruction.Pois)
                        {
                            // build attributes.
                            var poiTags = poi.Tags;
                            var poiAttributesTable = new AttributesTable();
                            if (poiTags != null)
                            { // there are tags.
                                foreach (var tag in poiTags)
                                {
                                    poiAttributesTable.AddAttribute(tag.Key, tag.Value);
                                }
                            }

                            // build feature.
                            var point = new Point(new GeoAPI.Geometries.Coordinate(poi.Location.Longitude, poi.Location.Latitude));
                            featureCollection.Add(new Feature(point, poiAttributesTable));
                        }
                    }
                }
            }
            else if (aggregated)
            { // aggregate all, just return geometry.
                var coordinates = route.GetPoints();
                var ntsCoordinates = coordinates.Select(x => { return new GeoAPI.Geometries.Coordinate(x.Longitude, x.Latitude); });
                var geometryFactory = new GeometryFactory();
                var lineString = geometryFactory.CreateLineString(ntsCoordinates.ToArray());
                var feature = new Feature(lineString, new AttributesTable());
                featureCollection.Add(feature);
            }
            else
            { // just keep segments as they are.
                var geometryFactory = new GeometryFactory();
                for (int i = 0; i < route.Segments.Length - 1; i++)
                {
                    // create a line string for the current segment.
                    var segmentLineString = geometryFactory.CreateLineString(new GeoAPI.Geometries.Coordinate[]{
                        new GeoAPI.Geometries.Coordinate(route.Segments[i].Longitude, route.Segments[i].Latitude),
                        new GeoAPI.Geometries.Coordinate(route.Segments[i+1].Longitude, route.Segments[i+1].Latitude)
                    });
                    var segmentTags = route.Segments[i].Tags;
                    var attributesTable = new AttributesTable();
                    if (segmentTags != null)
                    { // there are tags.
                        foreach (var tag in segmentTags)
                        {
                            attributesTable.AddAttribute(tag.Key, tag.Value);
                        }
                    }
                    attributesTable.AddAttribute("segment:time", route.Segments[i].Time);
                    attributesTable.AddAttribute("segment:distance", route.Segments[i].Distance);
                    if (route.Segments[i].Vehicle != null)
                    {
                        attributesTable.AddAttribute("vehicle_unique_name", route.Segments[i].Vehicle);
                    }
                    featureCollection.Add(new Feature(segmentLineString, attributesTable));

                    // create points.
                    if (route.Segments[i].Points != null)
                    {
                        foreach (var point in route.Segments[i].Points)
                        {
                            // build attributes.
                            var currentPointTags = point.Tags;
                            attributesTable = new AttributesTable();
                            if (currentPointTags != null)
                            { // there are tags.
                                foreach (var tag in currentPointTags)
                                {
                                    attributesTable.AddAttribute(tag.Key, tag.Value);
                                }
                            }

                            // build feature.
                            var pointGeometry = new Point(new GeoAPI.Geometries.Coordinate(point.Longitude, point.Latitude));
                            pointsCollection.Add(new Feature(pointGeometry, attributesTable));
                        }
                    }
                }
            }

            // add points
            foreach (var point in pointsCollection.Features)
            {
                featureCollection.Add(point);
            }
            return featureCollection;
        }

        /// <summary>
        /// Converts the given route to a feature collection augmented with instructions.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public override FeatureCollection GetFeaturesWithInstructions(Route route)
        {
            var instructions = this.GetInstructions(route);
            return this.GetFeatures(route, true, instructions);
        }

        /// <summary>
        /// Returns all network features in the given box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public override FeatureCollection GetNeworkFeatures(GeoCoordinateBox box)
        {
            // TODO: find a way to get nework features.
            return new FeatureCollection();
        }

        /// <summary>
        /// Returns true when the given vehicle is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public override bool SupportsVehicle(Vehicle vehicle)
        {
            return _router.SupportsVehicle(vehicle);
        }

        /// <summary>
        /// Calculates matrices of distances/times.
        /// </summary>
        /// <returns></returns>
        public override Tuple<string, double[][]>[] GetMatrix(Vehicle vehicle, GeoCoordinate[] source, GeoCoordinate[] target, 
            string[] outputs, out Tuple<string, int, string>[] errors)
        {
            // resolve all points.
            var resolvedSources = _router.Resolve(vehicle, 0.0075f, source);
            var resolvedTargets = _router.Resolve(vehicle, 0.0075f, target);

            // check for errors.
            var errorList = new List<Tuple<string, int, string>>();
            var resolvedSourcesList = new List<RouterPoint>();
            var resolvedTargetsList = new List<RouterPoint>();
            for(var i = 0; i < resolvedSources.Length; i++)
            {
                if(resolvedSources[i] == null)
                {
                    errorList.Add(new Tuple<string, int, string>("source", i, "Point could not be resolved."));
                }
                else
                {
                    resolvedSourcesList.Add(resolvedSources[i]);
                }
            }
            for (var i = 0; i < resolvedTargets.Length; i++)
            {
                if (resolvedTargets[i] == null)
                {
                    errorList.Add(new Tuple<string, int, string>("target", i, "Point could not be resolved."));
                }
                else
                {
                    resolvedTargetsList.Add(resolvedTargets[i]);
                }
            }
            errors = errorList.ToArray();
            resolvedSources = resolvedSourcesList.ToArray();
            resolvedTargets = resolvedTargetsList.ToArray();

            // calculates the routes.
            var matrices = new Tuple<string, double[][]>[outputs.Length];
            if (outputs.Length == 1 && outputs[0] == "weights")
            { // calculate weights only.
                matrices[0] = new Tuple<string, double[][]>(outputs[0],
                    _router.CalculateManyToManyWeight(vehicle, resolvedSources, resolvedTargets));
                return matrices;
            }
            else
            { // calculate complete routes and extract information from there.
                var routes = _router.CalculateManyToMany(vehicle, resolvedSources, resolvedTargets, float.MaxValue, false);

                // TODO: if weights are requested now the time-value is taken because in OsmSharp at the moment weights equal time for now.
                // this will change and at that time this needs to be updated.
                var times = new double[routes.Length][];
                for(var x = 0; x < times.Length; x++)
                {
                    times[x] = new double[routes[x].Length];
                    for(var y = 0; y < times[x].Length; y++)
                    {
                        times[x][y] = routes[x][y].TotalTime;
                    }
                }
                var distances = new double[routes.Length][];
                for (var x = 0; x < distances.Length; x++)
                {
                    distances[x] = new double[routes[x].Length];
                    for (var y = 0; y < distances[x].Length; y++)
                    {
                        distances[x][y] = routes[x][y].TotalDistance;
                    }
                }
                for(var i = 0; i < outputs.Length; i++)
                {
                    if(outputs[i] == "weights" ||
                        outputs[i] == "times")
                    {
                        matrices[i] = new Tuple<string, double[][]>(
                            outputs[i], times);
                    }
                    else if (outputs[i] == "distances")
                    {
                        matrices[i] = new Tuple<string, double[][]>(
                            outputs[i], distances);
                    }
                }
            }
            return matrices;
        }
    }
}