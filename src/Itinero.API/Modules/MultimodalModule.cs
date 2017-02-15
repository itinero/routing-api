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

using Itinero.API.Instances;
using Itinero.LocalGeo;
using Itinero.Logging;
using Itinero.Profiles;
using Nancy;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Itinero.API.Modules
{
    /// <summary>
    /// A module responsible for routing calls.
    /// </summary>
    public class MultimodalModule : NancyModule
    {
        public MultimodalModule()
        {
            Get("{instance}/multimodal", _ =>
            {
                return this.DoRouting(_);
            });
        }

        private object DoRouting(dynamic _)
        {
            try
            {
                this.EnableCors();

                // get instance and check if active.
                string instanceName = _.instance;
                IInstance instance;
                if (!InstanceManager.TryGet(instanceName, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }
                
                if (string.IsNullOrWhiteSpace(this.Request.Query.loc.ToString()))
                { // no loc parameters.
                    return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("loc parameter not found or request invalid.");
                }
                var locs = this.Request.Query.loc.ToString().Split(',');
                if (locs.Length < 4)
                { // less than two loc parameters.
                    return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("only one loc parameter found or request invalid.");
                }
                var coordinates = new Coordinate[locs.Length / 2];
                for (int idx = 0; idx < coordinates.Length; idx++)
                {
                    float lat, lon = 0f;
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

                // get profile.
                string profilesParameter = "car"; // assume profile is the default.
                if (!string.IsNullOrWhiteSpace(this.Request.Query.profile))
                { // a vehicle was defined.
                    profilesParameter = this.Request.Query.profile;
                }
                var profiles = new List<string>();
                var profileNames = profilesParameter.Split('|');
                for (var i = 0; i < profileNames.Length; i++)
                {
                    // check for support for the given vehicle.
                    if (!instance.Supports(profileNames[i]))
                    { // vehicle is not supported.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(
                            string.Format("Profile with name '{0}' is unsupported by this instance.", profileNames[i]));
                    }
                    profiles.Add(profileNames[i]);
                }

                // parse time.
                if (string.IsNullOrWhiteSpace(this.Request.Query.time))
                { // there is a format field.
                    return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("No valid time parameter found.");
                }
                DateTime dt;
                string pattern = "yyyyMMddHHmm";
                if (!DateTime.TryParseExact(this.Request.Query.time, pattern, System.Globalization.CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dt))
                { // could not parse date.
                    return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(
                        string.Format("No valid time parameter found, could not parse date: {0}. Expected to be in format yyyyMMddHHmm."));
                }

                var sourceTime = -1;
                var targetTime = -1;
                if (this.Request.Query.locTime != null)
                {
                    var locTime = this.Request.Query.locTime.Split(',');
                    if (locTime.Length >= 2)
                    {
                        if (!int.TryParse(locTime[0], out sourceTime) ||
                            !int.TryParse(locTime[locTime.Length - 1], out targetTime))
                        { // parsing failed.
                            return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("location timings present but could not be parsed.");
                        }
                    }
                }

                // build parameters.
                var parameters = new Dictionary<string, object>();
                parameters["sourceTime"] = sourceTime;
                parameters["targetTime"] = targetTime;

                // calculate route.
                var route = instance.TryEarliestArrival(dt, profiles[0], coordinates[0],
                    profiles[profiles.Count - 1], coordinates[coordinates.Length - 1],
                        parameters);
                if (route == null ||
                    route.IsError)
                { // route could not be calculated.
                    return Negotiate.WithStatusCode(HttpStatusCode.NoContent).WithModel(
                        string.Format("Route could not be calculated: {0}", route.ErrorMessage));
                }
                return route.Value;
            }
            catch (Exception)
            { // an unhandled exception!
                return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
            }
        }
    }
}
