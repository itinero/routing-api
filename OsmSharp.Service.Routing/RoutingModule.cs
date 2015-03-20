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

using Nancy;
using Nancy.Json;
using Nancy.ModelBinding;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Service.Routing.Domain;
using OsmSharp.Service.Routing.Domain.Queries;
using System;

namespace OsmSharp.Service.Routing
{
    /// <summary>
    /// The routing module.
    /// </summary>
    public class RoutingModule : NancyModule
    {
        /// <summary>
        /// Creates a new routing module.
        /// </summary>
        public RoutingModule()
        {
            JsonSettings.MaxJsonLength = Int32.MaxValue;

            Get["{instance}/routing"] = _ =>
            {
                try
                {
                    this.EnableCors();

                    // get instance and check if active.
                    string instance = _.instance;
                    if(!ApiBootstrapper.IsActive(instance))
                    { // oeps, instance not active!
                        return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                    }

                    // bind the query if any.
                    var query = this.Bind<RoutingQuery>();

                    // parse location.
                    if (string.IsNullOrWhiteSpace(query.loc))
                    { // no loc parameters.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("loc parameter not found or request invalid.");
                    }
                    var locs = query.loc.Split(',');
                    if (locs.Length < 2)
                    { // less than two loc parameters.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("only one loc parameter found or request invalid.");
                    }
                    var coordinates = new GeoCoordinate[locs.Length / 2];
                    for (int idx = 0; idx < coordinates.Length; idx++)
                    {
                        double lat, lon;
                        if (double.TryParse(locs[idx * 2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lat) &&
                            double.TryParse(locs[idx * 2 + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lon))
                        { // parsing was successful.
                            coordinates[idx] = new GeoCoordinate(lat, lon);
                        }
                        else
                        { // invalid formatting.
                            return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("location coordinates are invalid.");
                        }
                    }

                    // get vehicle.
                    string vehicleName = "car"; // assume car is the default.
                    if (!string.IsNullOrWhiteSpace(query.vehicle))
                    { // a vehicle was defined.
                        vehicleName = query.vehicle;
                    }
                    var vehicle = Vehicle.GetByUniqueName(vehicleName);
                    if (vehicle == null)
                    { // vehicle not found or not registered.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(string.Format("vehicle with name '{0}' not found.", vehicleName));
                    }

                    bool instructions = false;
                    if (!string.IsNullOrWhiteSpace(query.instructions))
                    { // there is an instruction flag.
                        instructions = query.instructions == "true";
                    }

                    bool complete = false;
                    if (!string.IsNullOrWhiteSpace(query.complete))
                    { // there is a complete flag.
                        complete = query.complete == "true";
                    }

                    bool fullFormat = false;
                    if (!string.IsNullOrWhiteSpace(query.format))
                    { // there is a format field.
                        fullFormat = query.format == "osmsharp";
                    }

                    // check conflicting parameters.
                    if (!complete && instructions)
                    { // user wants an incomplete route but instructions, this is impossible. 
                        complete = true;
                    }

                    // check for support for the given vehicle.
                    if(!ApiBootstrapper.Get(instance).SupportsVehicle(vehicle))
                    { // vehicle is not supported.
                        return Negotiate.WithStatusCode(HttpStatusCode.BadRequest).WithModel(string.Format("Vehicle with name '{0}' cannot be use with this routing instance.", vehicleName));
                    }

                    // calculate route.
                    var route = ApiBootstrapper.Get(instance).GetRoute(vehicle, coordinates, complete);
                    if (route == null)
                    { // route could not be calculated.
                        return null;
                    }
                    if (route != null && instructions)
                    { // also calculate instructions.
                        var instruction = ApiBootstrapper.Get(instance).GetInstructions(vehicle, route);

                        if (fullFormat)
                        {
                            return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(new CompleteRoute()
                                {
                                    Route = route,
                                    Instructions = instruction
                                });
                        }
                        else
                        {
                            var featureCollection = ApiBootstrapper.Get(instance).GetFeatures(route);
                            var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();
                            var geoJson = geoJsonWriter.Write(featureCollection);

                            return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(new SimpleRoute()
                            {
                                Route = geoJson,
                                Instructions = instruction
                            });
                        }
                    }

                    if (fullFormat)
                    { // return a complete route but no instructions.
                        return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(route);
                    }
                    else
                    { // return a GeoJSON object.
                        var featureCollection = ApiBootstrapper.Get(instance).GetFeatures(route);

                        return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(featureCollection);
                    }
                }
                catch (Exception)
                { // an unhandled exception!
                    return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
                }
            };
            Get["{instance}/routing/network"] = _ =>
            {
                try
                {
                    // get instance and check if active.
                    string instance = _.instance;
                    if (!ApiBootstrapper.IsActive(instance))
                    { // oeps, instance not active!
                        return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                    }

                    // bind the query if any.
                    var query = this.Bind<BoxQuery>();

                    double left, right, top, bottom;
                    if (string.IsNullOrWhiteSpace(query.left) ||
                        !double.TryParse(query.left, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out left))
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("box coordinates are invalid.");
                    }
                    if (string.IsNullOrWhiteSpace(query.right) ||
                        !double.TryParse(query.right, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out right))
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("box coordinates are invalid.");
                    }
                    if (string.IsNullOrWhiteSpace(query.top) ||
                        !double.TryParse(query.top, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out top))
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("box coordinates are invalid.");
                    }
                    if (string.IsNullOrWhiteSpace(query.bottom) ||
                        !double.TryParse(query.bottom, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out bottom))
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("box coordinates are invalid.");
                    }

                    var features = ApiBootstrapper.Get(instance).GetNeworkFeatures(new GeoCoordinateBox(new GeoCoordinate(top, left), new GeoCoordinate(bottom, right)));
                    var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();
                    return geoJsonWriter.Write(features);
                }
                catch (Exception)
                { // an unhandled exception!
                    return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
                }
            };
        }
    }
}
