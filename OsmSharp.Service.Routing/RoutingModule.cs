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
using OsmSharp.Routing.Vehicles;
using OsmSharp.Service.Routing.Domain;
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
                return this.GetInstanceRouting(_);
            };
            Put["{instance}/routing"] = _ =>
            {
                return this.PutInstanceRouting(_);
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
                    return OsmSharp.Geo.Streams.GeoJson.GeoJsonConverter.ToGeoJson(features);
                }
                catch (Exception)
                { // an unhandled exception!
                    return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
                }
            };
        }

        /// <summary>
        /// Called on Get '/{instance}/routing'.
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic GetInstanceRouting(dynamic _)
        {
            return this.DoRouting(_);
        }

        /// <summary>
        /// Called on Put '/{instance}/routing'.
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic PutInstanceRouting(dynamic _)
        {
            return this.DoRouting(_);
        }

        /// <summary>
        /// Executes a routing request.
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic DoRouting(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                string instance = _.instance;
                if (!ApiBootstrapper.IsActive(instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                // try and get all this data from the request-data.
                GeoCoordinate[] coordinates = null;
                var vehicle = Vehicle.Car;
                var sort = false;
                var fullFormat = false;

                // bind the query if any.
                if(this.Request.Body == null || this.Request.Body.Length == 0)
                { // there is no body.
                    var urlParameterRequest = this.Bind<UrlParametersRequest>();
                    if (!string.IsNullOrWhiteSpace(urlParameterRequest.loc))
                    { // no loc parameters.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("loc parameter not found or request invalid.");
                    }
                    var locs = urlParameterRequest.loc.Split(',');
                    if (locs.Length < 2)
                    { // less than two loc parameters.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("only one loc parameter found or request invalid.");
                    }
                    coordinates = new GeoCoordinate[locs.Length / 2];
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
                    if (!string.IsNullOrWhiteSpace(urlParameterRequest.vehicle))
                    { // a vehicle was defined.
                        vehicleName = urlParameterRequest.vehicle;
                    }
                    vehicle = Vehicle.GetByUniqueName(vehicleName);
                    if (vehicle == null)
                    { // vehicle not found or not registered.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(string.Format("vehicle with name '{0}' not found.", vehicleName));
                    }

                    if (!string.IsNullOrWhiteSpace(urlParameterRequest.sort))
                    { // there is a sort flag.
                        sort = urlParameterRequest.sort.ToLowerInvariant() == "true";
                    }

                    if (!string.IsNullOrWhiteSpace(urlParameterRequest.format))
                    { // there is a format field.
                        fullFormat = urlParameterRequest.format == "osmsharp";
                    }
                }
                else
                { // this should be a request with a json-body.
                    var request = this.Bind<OsmSharp.Service.Routing.Domain.Request>();

                    if (request.locations == null || request.locations.Length < 2)
                    { // less than two loc parameters.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("only one location found or request invalid.");
                    }
                    coordinates = new GeoCoordinate[request.locations.Length];
                    for (int idx = 0; idx < coordinates.Length; idx++)
                    {
                        coordinates[idx] = new GeoCoordinate(request.locations[idx][1], request.locations[idx][0]);
                    }

                    // get vehicle.
                    string vehicleName = "car"; // assume car is the default.
                    if (request.profile != null && !string.IsNullOrWhiteSpace(request.profile.vehicle))
                    { // a vehicle was defined.
                        vehicleName = request.profile.vehicle;
                    }
                    vehicle = Vehicle.GetByUniqueName(vehicleName);
                    if (vehicle == null)
                    { // vehicle not found or not registered.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(
                            string.Format("vehicle with name '{0}' not found.", vehicleName));
                    }

                    if (!string.IsNullOrWhiteSpace(request.sort))
                    { // there is a sort flag.
                        sort = request.sort.ToLowerInvariant() == "true";
                    }

                    if (!string.IsNullOrWhiteSpace(request.format))
                    { // there is a format field.
                        fullFormat = request.format == "osmsharp";
                    }
                }

                // check for support for the given vehicle.
                if (!ApiBootstrapper.Get(instance).SupportsVehicle(vehicle))
                { // vehicle is not supported.
                    return Negotiate.WithStatusCode(HttpStatusCode.BadRequest).WithModel(
                        string.Format("Vehicle with name '{0}' cannot be use with this routing instance.", vehicle.UniqueName));
                }

                // calculate route.
                var route = ApiBootstrapper.Get(instance).GetRoute(vehicle, coordinates, false, sort);
                if (route == null)
                { // route could not be calculated.
                    return null;
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
        }
    }
}
