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

using OsmSharp.Collections.Tags;
using OsmSharp.Geo;
using OsmSharp.Geo.Features;
using OsmSharp.Routing.Profiles;
using System.Collections.Generic;

namespace OsmSharp.Routing.API.Instances
{
    /// <summary>
    /// A default routing module instance implemenation.
    /// </summary>
    public class DefaultRoutingModuleInstance : IRoutingModuleInstance
    {
        private readonly IRouter _router;

        /// <summary>
        /// Creates a new default routing instance.
        /// </summary>
        public DefaultRoutingModuleInstance(IRouter router)
        {
            _router = router;
        }

        /// <summary>
        /// Returns true if the given profile is supported.
        /// </summary>
        public bool Supports(Profile profile)
        {
            return _router.SupportsAll(profile);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public Result<Route> Calculate(Profile profile, ICoordinate[] locations, 
            Dictionary<string, object> parameters)
        {
            var routerPoints = new RouterPoint[locations.Length];
            for (var i = 0; i < routerPoints.Length; i++)
            {
                var resolveResult = _router.TryResolve(profile, locations[i], 500);
                if (resolveResult.IsError)
                {
                    return resolveResult.ConvertError<Route>();
                }
                routerPoints[i] = resolveResult.Value;
            }

            return _router.TryCalculate(profile, routerPoints);
        }

        /// <summary>
        /// Calculates a route along the given locations and returns it's geometry.
        /// </summary>
        public Result<Feature> CalculateGeometry(Profile profile, ICoordinate[] locations, 
            Dictionary<string, object> parameters)
        {
            var routerPoints = new RouterPoint[locations.Length];
            for(var i = 0; i < routerPoints.Length; i++)
            {
                var resolveResult = _router.TryResolve(profile, locations[i], 500);
                if (resolveResult.IsError)
                {
                    return resolveResult.ConvertError<Feature>();
                }
                routerPoints[i] = resolveResult.Value;
            }

            var result = _router.TryCalculate(profile, routerPoints);
            if(result.IsError)
            {
                return result.ConvertError<Feature>();
            }

            var lineString = result.Value.ToLineString();
            return new Result<Feature>(new Feature(lineString, 
                new Geo.Attributes.SimpleGeometryAttributeCollection(
                    new Tag[] {
                        new Tag("time", result.Value.TotalTime.ToInvariantString()),
                        new Tag("distance", result.Value.TotalDistance.ToInvariantString()),
                    })));
        }
    }
}
