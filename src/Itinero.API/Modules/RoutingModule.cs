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
// THE SOFTWARE

using Nancy;
using Itinero.API.Instances;
using Itinero.LocalGeo;

namespace Itinero.API.Modules
{
    /// <summary>
    /// A module responsible for routing calls.
    /// </summary>
    public class RoutingModule : NancyModule
    {
        public RoutingModule()
        {
            Get("{instance}/routing", _ =>
            {
                return this.DoRouting(_);
            });
        }

        private object DoRouting(dynamic _)
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
            string profileName = "car"; // assume car is the default.
            if (!string.IsNullOrWhiteSpace(this.Request.Query.profile))
            { // a vehicle was defined.
                profileName = this.Request.Query.profile;
            }
            if (!instance.Supports(profileName))
            {// vehicle not found or not registered.
                return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(
                    string.Format("Profile with name '{0}' not found.", profileName));
            }

            // tries to calculate the given route.
            var result = instance.Calculate(profileName, coordinates);
            if (result.IsError)
            {
                return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
            }
            return result.Value;
        }
    }
}