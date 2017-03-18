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
using System;

namespace Itinero.API.Instances
{
    /// <summary>
    /// Representation of a routing instance. Wraps a router and a router db.
    /// </summary>
    public class Instance : IInstance
    {
        private readonly MultimodalRouter _router;
        private readonly Dictionary<string, int> _defaultSeconds;

        /// <summary>
        /// Creates a new routing instances.
        /// </summary>
        public Instance(MultimodalRouter router, int carTime = 15 * 60,
            int pedestrianTime = 10 * 60, int bicycleTime = 5 * 60)
        {
            _router = router;

            _defaultSeconds = new Dictionary<string, int>();
            _defaultSeconds.Add("car", carTime);
            _defaultSeconds.Add("pedestrian", pedestrianTime);
            _defaultSeconds.Add("bicycle", bicycleTime);
        }

        /// <summary>
        /// Gets the routerdb.
        /// </summary>
        public RouterDb RouterDb
        {
            get
            {
                return _router.Router.Db;
            }
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
                var result = _router.Router.TryResolve(profile, coordinates[i], 200);
                if (result.IsError)
                {
                    result = _router.Router.TryResolve(profile, coordinates[i], 2000);
                }

                points[i] = result.Value;
            }

            if (!_router.Router.Db.HasContractedFor(profile))
            {
                Logging.Logger.Log("Instance", Logging.TraceEventType.Warning, 
                    "RouterDb is not optimized for profile {0}, it doesn't contain a contracted graph for this profile.", profileName);
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

        /// <summary>
        /// Tries to calculate an earliest arrival route.
        /// </summary>
        public Result<Route> TryEarliestArrival(DateTime departureTime, string sourceProfileName, Coordinate sourceLocation, 
            string targetProfileName, Coordinate targetLocation, Dictionary<string, object> parameters)
        {
            var sourceProfile = _router.Router.Db.GetSupportedProfile(sourceProfileName);
            var targetProfile = _router.Router.Db.GetSupportedProfile(targetProfileName);

            var sourcePoint = _router.Router.TryResolve(sourceProfile, sourceLocation, 1000);
            if (sourcePoint.IsError)
            {
                return sourcePoint.ConvertError<Route>();
            }
            var targetPoint = _router.Router.TryResolve(targetProfile, targetLocation, 1000);
            if (targetPoint.IsError)
            {
                return targetPoint.ConvertError<Route>();
            }

            int maxSecondsSource = 0;
            if (parameters.ContainsKey("sourceTime") &&
                parameters["sourceTime"] is int &&
                (int)parameters["sourceTime"] > 0)
            { // override the default source time.
                maxSecondsSource = (int)parameters["sourceTime"];
            }
            else
            { // get the default source time.
                if (!_defaultSeconds.TryGetValue(sourceProfile.Name, out maxSecondsSource))
                {
                    maxSecondsSource = 30 * 60;
                }
            }

            int maxSecondsTarget = 0;
            if (parameters.ContainsKey("targetTime") &&
                parameters["targetTime"] is int &&
                (int)parameters["targetTime"] > 0)
            { // override the default target time.
                maxSecondsTarget = (int)parameters["targetTime"];
            }
            else
            { // get the default target time.
                if (!_defaultSeconds.TryGetValue(targetProfile.Name, out maxSecondsTarget))
                {
                    maxSecondsTarget = 30 * 60;
                }
            }

            return _router.TryEarliestArrival(departureTime,
                sourcePoint.Value, sourceProfile,
                    targetPoint.Value, targetProfile,
                        new EarliestArrivalSettings()
                        {
                            MaxSecondsSource = maxSecondsSource,
                            MaxSecondsTarget = maxSecondsTarget
                        });
        }
    }
}