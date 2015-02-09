using Nancy;
using Nancy.IO;
using Nancy.Json;
using Nancy.Responses.Negotiation;
using NetTopologySuite.Features;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing
{
    public class GeoJsonResponseProcessor : IResponseProcessor 
    {

        private static readonly IEnumerable<Tuple<string, MediaRange>> extensionMappings =
            new[] { new Tuple<string, MediaRange>("json", MediaRange.FromString("application/json")) };

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonProcessor"/> class,
        /// with the provided <paramref name="serializers"/>.
        /// </summary>
        /// <param name="serializers">The serializes that the processor will use to process the request.</param>
        public GeoJsonResponseProcessor()
        {

        }

        /// <summary>
        /// Gets a set of mappings that map a given extension (such as .json)
        /// to a media range that can be sent to the client in a vary header.
        /// </summary>
        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get {  return extensionMappings; }
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
