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
using Nancy.Responses.Negotiation;
using Itinero.API.Reponses;
using System;
using System.Collections.Generic;
using Itinero.LocalGeo;

namespace Itinero.API.Responses
{
    /// <summary>
    /// A response processor to format GeoJSON.
    /// </summary>
    public class GeoJsonResponseProcessor : IResponseProcessor
    {
        private static readonly IEnumerable<Tuple<string, MediaRange>> extensionMappings =
            new[] { new Tuple<string, MediaRange>("json", new MediaRange("application/json")) };

        /// <summary>
        /// Creates a new GeoJSON repsonse processor.
        /// </summary>
        public GeoJsonResponseProcessor()
        {

        }

        /// <summary>
        /// Gets a set of mappings that map a given extension (such as .json)
        /// to a media range that can be sent to the client in a vary header.
        /// </summary>
        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get { return extensionMappings; }
        }

        /// <summary>
        /// Determines whether the the processor can handle a given content type and model
        /// </summary>
        /// <param name="requestedMediaRange">Content type requested by the client</param>
        /// <param name="model">The model for the given media range</param>
        /// <param name="context">The nancy context</param>
        /// <returns>A ProcessorMatch result that determines the priority of the processor</returns>
        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (model is Route)
            {
                if (IsExactJsonContentType(requestedMediaRange))
                {
                    return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.ExactMatch
                    };
                }

                if (IsWildcardJsonContentType(requestedMediaRange))
                {
                    return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.NonExactMatch
                    };
                }
            }
            if (model is List<Polygon>)
            {
                if (IsExactJsonContentType(requestedMediaRange))
                {
                    return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.ExactMatch
                    };
                }

                if (IsWildcardJsonContentType(requestedMediaRange))
                {
                    return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.NonExactMatch
                    };
                }
            }
            if (model is List<Tuple<float, float, List<Coordinate>>>)
            {
                if (IsExactJsonContentType(requestedMediaRange))
                {
                    return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.ExactMatch
                    };
                }

                if (IsWildcardJsonContentType(requestedMediaRange))
                {
                    return new ProcessorMatch
                    {
                        ModelResult = MatchResult.ExactMatch,
                        RequestedContentTypeResult = MatchResult.NonExactMatch
                    };
                }
            }
            return new ProcessorMatch
            {
                ModelResult = MatchResult.DontCare,
                RequestedContentTypeResult = MatchResult.NoMatch
            };
        }

        /// <summary>
        /// Process the response
        /// </summary>
        /// <param name="requestedMediaRange">Content type requested by the client</param>
        /// <param name="model">The model for the given media range</param>
        /// <param name="context">The nancy context</param>
        /// <returns>A response</returns>
        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (model is Route)
            {
                return new RouteGeoJsonResponse(model as Route);
            }
            if (model is List<Polygon>)
            {
                return new PolygonsGeoJsonResponse(model as List<Polygon>);
            }
            if (model is List<Tuple<float, float, List<Coordinate>>>)
            {
                return new LinesGeoJsonResponse(model as List<Tuple<float, float, List<Coordinate>>>);
            }
            throw new ArgumentOutOfRangeException("GeoJsonResponseProcessor can only process Routes.");
        }

        private static bool IsExactJsonContentType(MediaRange requestedContentType)
        {
            if (requestedContentType.Type.IsWildcard && requestedContentType.Subtype.IsWildcard)
            {
                return true;
            }

            return requestedContentType.Matches("application/json") || requestedContentType.Matches("text/json");
        }

        private static bool IsWildcardJsonContentType(MediaRange requestedContentType)
        {
            if (!requestedContentType.Type.IsWildcard && !string.Equals("application", requestedContentType.Type, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (requestedContentType.Subtype.IsWildcard)
            {
                return true;
            }

            var subtypeString = requestedContentType.Subtype.ToString();

            return (subtypeString.StartsWith("vnd", StringComparison.OrdinalIgnoreCase) &&
                    subtypeString.EndsWith("+json", StringComparison.OrdinalIgnoreCase));
        }
    }
}