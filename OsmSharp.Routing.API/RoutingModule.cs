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
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.API
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
                return this.DoRouting(_);
            };
            Put["{instance}/routing"] = _ =>
            {
                return this.DoRouting(_);
            };
        }

        /// <summary>
        /// Executes a routing request.
        /// </summary>
        /// <returns></returns>
        private dynamic DoRouting(dynamic _)
        {
            try
            {
                this.EnableCors();

                // validate requests.
                if(!Bootstrapper.ValidateRequest(this, _))
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.Forbidden);
                }

                // get instance and check if active.
                string instance = _.instance;
                if (!RoutingBootstrapper.IsActive(instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                // try and get all this data from the request-data.
                GeoCoordinate[] coordinates = null;
                var profile = OsmSharp.Routing.Osm.Vehicles.Vehicle.Car.Fastest();
                var sort = false;
                var fullFormat = false;

                // bind the query if any.
                if (this.Request.Body == null || this.Request.Body.Length == 0)
                { // there is no body.
                    var urlParameterRequest = this.Bind<Domain.UrlParametersRequest>();
                    if (string.IsNullOrWhiteSpace(urlParameterRequest.loc))
                    { // no loc parameters.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("loc parameter not found or request invalid.");
                    }
                    var locs = urlParameterRequest.loc.Split(',');
                    if (locs.Length < 4)
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
                    string profileName = "car.fastest"; // assume car is the default.
                    if (!string.IsNullOrWhiteSpace(urlParameterRequest.profile))
                    { // a vehicle was defined.
                        profileName = urlParameterRequest.profile;
                    }
                    if (!Profile.TryGet(profileName, out profile))
                    {// vehicle not found or not registered.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(
                            string.Format("Profile with name '{0}' not found.", profileName));
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
                    var request = this.Bind<Domain.Request>();

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
                    string profileName = "car"; // assume car is the default.
                    if (request.profile != null && !string.IsNullOrWhiteSpace(request.profile.name))
                    { // a vehicle was defined.
                        profileName = request.profile.name;
                    }
                    if (!Profile.TryGet(profileName, out profile))
                    { // vehicle not found or not registered.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(
                            string.Format("Profile with name '{0}' not found.", profileName));
                    }

                    if (!string.IsNullOrWhiteSpace(request.format))
                    { // there is a format field.
                        fullFormat = request.format == "osmsharp";
                    }
                }

                // check for support for the given vehicle.
                if (!RoutingBootstrapper.Get(instance).Supports(profile))
                { // vehicle is not supported.
                    return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(
                        string.Format("Profile with name '{0}' is unsupported by this instance.", profile.Name));
                }

                // calculate route.
                var route = RoutingBootstrapper.Get(instance).Calculate(profile, coordinates, new Dictionary<string,object>());
                if (route == null ||
                    route.IsError)
                { // route could not be calculated.
                    return null;
                }

                if (fullFormat)
                { // return a complete route but no instructions.
                    return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(route.Value);
                }
                else
                { // return a GeoJSON object.
                    var featureCollection = route.Value.ToFeatureCollection(
                        OsmSharp.Routing.Algorithms.Routes.RouteSegmentAggregator.ModalAggregator);

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
