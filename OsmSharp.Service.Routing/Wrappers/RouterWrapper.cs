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
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Osm.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Service.Routing.Wrappers
{
    /// <summary>
    /// A routing service wrapper class around a router.
    /// </summary>
    public class RouterWrapper : RoutingServiceWrapperBase
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
            var resolved = _router.Resolve(vehicle, coordinates);

            var routes = new Route[resolved.Length - 1];
            if (sort)
            { // TODO: sort the via-points.
                throw new NotSupportedException();
            }
            else
            { // concatenate routes.
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
                        if(idx > 0 &&
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
            }

            // concatenate the routes.
            var route = routes[0];
            for(int idx = 1; idx < routes.Length; idx++)
            {
                route = Route.Concatenate(route, routes[idx]);
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
        /// Calculates instructions for a given route.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        public override List<Instruction> GetInstructions(Vehicle vehicle, Route route)
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
            var coordinates = route.GetPoints();
            var ntsCoordinates = coordinates.Select(x => { return new Coordinate(x.Longitude, x.Latitude); });
            var geometryFactory = new GeometryFactory();
            var lineString = geometryFactory.CreateLineString(ntsCoordinates.ToArray());
            var featureCollection = new FeatureCollection();

            var attributes = new AttributesTable();
            attributes.AddAttribute("osmsharp:total_time", route.TotalTime.ToInvariantString());
            attributes.AddAttribute("osmsharp:total_distance", route.TotalDistance.ToInvariantString());

            var feature = new Feature(lineString, attributes);

            featureCollection.Add(feature);
            return featureCollection;
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
    }
}
