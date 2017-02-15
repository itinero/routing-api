// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

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

using Itinero.LocalGeo;
using Itinero.Profiles;
using System.Collections.Generic;
using Itinero.API.Models;
using System.Linq;
using Itinero.Algorithms.Networks.Analytics.Heatmaps;
using Itinero.Algorithms.Networks.Analytics.Isochrones;
using Itinero.Algorithms.Networks.Analytics.Trees;
using Itinero.Transit;

namespace Itinero.API.Instances
{
    /// <summary>
    /// Representation of a routing instance. Wraps a router and a router db.
    /// </summary>
    public class Instance : IInstance
    {
        private readonly MultimodalRouter _router;

        /// <summary>
        /// Creates a new routing instances.
        /// </summary>
        public Instance(MultimodalRouter router)
        {
            _router = router;
        }

        /// <summary>
        /// Gets meta-data about this instance.
        /// </summary>
        /// <returns></returns>
        public InstanceMeta GetMeta()
        {
            var meta = new InstanceMeta();
            meta.Id = _router.Router.Db.Guid.ToString();
            meta.Meta = _router.Router.Db.Meta;
            
            var metaProfiles = new List<ProfileMeta>();
            foreach(var vehicle in _router.Router.Db.GetSupportedVehicles())
            {
                foreach(var profile in vehicle.GetProfiles())
                {
                    var metric = "custom";
                    switch(profile.Metric)
                    {
                        case ProfileMetric.DistanceInMeters:
                            metric = "distance";
                            break;
                        case ProfileMetric.TimeInSeconds:
                            metric = "time";
                            break;
                    }
                    metaProfiles.Add(new ProfileMeta()
                    {
                        Metric = metric,
                        Name = profile.FullName
                    });
                }
            }
            meta.Profiles = metaProfiles.ToArray();

            meta.Contracted = _router.Router.Db.GetContractedProfiles().ToArray();

            return meta;
        }

        /// <summary>
        /// Returns true if the given profile is supported.
        /// </summary>
        public bool Supports(string profile)
        {
            return _router.Router.Db.SupportProfile(profile);
        }

        /// <summary>
        /// Calculates a routing along the given coordinates.
        /// </summary>
        public Result<Route> Calculate(string profileName, Coordinate[] coordinates)
        {
            var profile = _router.Router.Db.GetSupportedProfile(profileName);

            var points = new RouterPoint[coordinates.Length];
            for(var i = 0; i < coordinates.Length; i++)
            {
                points = _router.Router.Resolve(profile, coordinates, 200);
            }

            return _router.Router.TryCalculate(profile, points);
        }

        /// <summary>
        /// Calculates a heatmap.
        /// </summary>
        public Result<HeatmapResult> CalculateHeatmap(string profileName, Coordinate coordinate, int max)
        {
            var profile = _router.Router.Db.GetSupportedProfile(profileName);

            var point = _router.Router.Resolve(profile, coordinate, 200);

            return new Result<HeatmapResult>(_router.Router.CalculateHeatmap(profile, point, max));
        }

        /// <summary>
        /// Calculates isochrones.
        /// </summary>
        public Result<List<Polygon>> CalculateIsochrones(string profileName, Coordinate coordinate, float[] limits)
        {
            var profile = _router.Router.Db.GetSupportedProfile(profileName);

            var point = _router.Router.Resolve(profile, coordinate, 200);

            return new Result<List<Polygon>>(_router.Router.CalculateIsochrones(profile, point, limits.ToList()));
        }

        /// <summary>
        /// Calculates a tree.
        /// </summary>
        public Result<Algorithms.Networks.Analytics.Trees.Models.Tree> CalculateTree(string profileName, Coordinate coordinate, int max)
        {
            var profile = _router.Router.Db.GetSupportedProfile(profileName);

            lock (_router)
            {
                var point = _router.Router.Resolve(profile, coordinate, 200);

                return new Result<Algorithms.Networks.Analytics.Trees.Models.Tree>(_router.Router.CalculateTree(profile, point, max));
            }
        }
    }
}