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

using Nancy;
using Nancy.Responses.Negotiation;
using OsmSharp.Geo.Features;
using OsmSharp.Routing.API.Reponses;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.API.Responses
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
            if (model is FeatureCollection)
            { // the model is a feature collection, only then can this GeoJson processor be used.
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
            if (model is FeatureCollection)
            { // the model is a feature collection, only then can this GeoJson processor be used.
                return new GeoJsonResponse(model as FeatureCollection);
            }
            throw new ArgumentOutOfRangeException("GeoJsonResponseProcessor can only process FeatureCollections.");
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
            if (!requestedContentType.Type.IsWildcard && !string.Equals("application", requestedContentType.Type, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (requestedContentType.Subtype.IsWildcard)
            {
                return true;
            }

            var subtypeString = requestedContentType.Subtype.ToString();

            return (subtypeString.StartsWith("vnd", StringComparison.InvariantCultureIgnoreCase) &&
                    subtypeString.EndsWith("+json", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}