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

using Itinero.LocalGeo;
using Itinero.LocalGeo.IO;
using Nancy;
using Nancy.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Itinero.API.Reponses
{
    /// <summary>
    /// A GeoJSON response.
    /// </summary>
    public class PolygonsGeoJsonResponse : Response
    {
        /// <summary>
        /// Holds the default content type.
        /// </summary>
        private static string DefaultContentType
        {
            get
            {
                return "application/json";
            }
        }

        /// <summary>
        /// Creates a new GeoJSON response.
        /// </summary>
        public PolygonsGeoJsonResponse(List<Polygon> model)
        {
            this.Contents = model == null ? NoBody : GetGeoJsonContents(model);
            this.ContentType = DefaultContentType;
            this.StatusCode = HttpStatusCode.OK;
        }

        /// <summary>
        /// Gets a stream for a given feature collection.
        /// </summary>
        /// <returns></returns>
        private static Action<Stream> GetGeoJsonContents(List<Polygon> model)
        {
            return stream =>
            {
                var geoJson = model.ToGeoJson();

                var geoJsonBytes = System.Text.Encoding.UTF8.GetBytes(geoJson);
                stream.Write(geoJsonBytes, 0, geoJsonBytes.Length);
            };
        }
    }
}