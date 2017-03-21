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

namespace Itinero.API
{
    /// <summary>
    /// Contains some extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Estimates the center location of the given routerdb.
        /// </summary>
        public static Coordinate EstimateCenter(this RouterDb db)
        {
            var latitudeTotal = 0d;
            var longitudeTotal = 0d;
            var count = 0;
            uint stepSize = 25;
            for(uint v = 0; v < db.Network.VertexCount; v += stepSize)
            {
                var coordinate = db.Network.GetVertex(v);

                count++;
                latitudeTotal += coordinate.Latitude;
                longitudeTotal += coordinate.Longitude;
            }

            return new Coordinate((float)(latitudeTotal / count),
                (float)(longitudeTotal / count));
        }
    }
}