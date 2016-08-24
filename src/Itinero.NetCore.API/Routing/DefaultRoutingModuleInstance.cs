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

using System.Collections.Generic;
using Itinero.LocalGeo;
using Itinero.Profiles;

namespace Itinero.API.Routing
{
    /// <summary>
    /// A default routing module instance implemenation.
    /// </summary>
    public class DefaultRoutingModuleInstance : IRoutingModuleInstance
    {
        private readonly RouterBase _router;

        /// <summary>
        /// Creates a new default routing instance.
        /// </summary>
        public DefaultRoutingModuleInstance(RouterBase router)
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
        public Result<Route> Calculate(Profile profile, Coordinate[] locations, 
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
    }
}