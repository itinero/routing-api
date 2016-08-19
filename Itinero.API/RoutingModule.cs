// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Nancy;
using Nancy.Json;
using Nancy.ModelBinding;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using Itinero.LocalGeo;

namespace Itinero.API
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
                Coordinate[] coordinates = null;
                var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
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
                    coordinates = new Coordinate[locs.Length / 2];
                    for (int idx = 0; idx < coordinates.Length; idx++)
                    {
                        float lat, lon;
                        if (float.TryParse(locs[idx * 2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lat) &&
                            float.TryParse(locs[idx * 2 + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lon))
                        { // parsing was successful.
                            coordinates[idx] = new Coordinate(lat, lon);
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
                    if (!Profile.TryGet("car.fastest", out profile))
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
                    coordinates = new Coordinate[request.locations.Length];
                    for (int idx = 0; idx < coordinates.Length; idx++)
                    {
                        coordinates[idx] = new Coordinate(request.locations[idx][1], request.locations[idx][0]);
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
                    var modalAggregator = new Itinero.Algorithms.Routes.RouteSegmentAggregator(
                        route.Value, Itinero.Algorithms.Routes.RouteSegmentAggregator.ModalAggregator);
                    modalAggregator.Run();
                    if (!modalAggregator.HasSucceeded)
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(modalAggregator.AggregatedRoute);
                }
            }
            catch (Exception ex)
            { // an unhandled exception!
                Itinero.Logging.Logger.Log("RoutingModule", Logging.TraceEventType.Error, ex.ToInvariantString());
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }
    }
}
