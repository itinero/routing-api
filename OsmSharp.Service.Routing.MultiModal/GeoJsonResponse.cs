using Nancy;
using Nancy.Json;
using NetTopologySuite.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.MultiModal
{
    public class GeoJsonResponse : Response
    {
        private static string contentType
        {
            get
            {
                return "application/json" + (String.IsNullOrWhiteSpace(JsonSettings.DefaultCharset) ? "" : "; charset=" + JsonSettings.DefaultCharset);
            }
        }

        public GeoJsonResponse(FeatureCollection model)
        {
            this.Contents = model == null ? NoBody : GetGeoJsonContents(model);
            this.ContentType = contentType;
            this.StatusCode = HttpStatusCode.OK;
        }

        private static Action<Stream> GetGeoJsonContents(FeatureCollection model)
        {
            return stream =>
            {
                var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();
                var geoJson = geoJsonWriter.Write(model as FeatureCollection);

                var geoJsonBytes = System.Text.Encoding.UTF8.GetBytes(geoJson);
                stream.Write(geoJsonBytes, 0, geoJsonBytes.Length);
            };
        }
    }
}
