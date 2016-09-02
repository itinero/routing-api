using System.Collections.Generic;
using System.Threading.Tasks;
using Itinero.LocalGeo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Itinero.LocalGeo.IO;

namespace Itinero.API.Formatters
{
    public class PolygonListOutputFormatter : IOutputFormatter
    {
        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return context.Object is List<Polygon>;
            // todo check on content type :  && context.ContentType == new StringSegment("application/json")
        }

        public Task WriteAsync(OutputFormatterWriteContext context)
        {
            return context.HttpContext.Response.WriteAsync(((List<Polygon>) context.Object).ToGeoJson());
        }
    }
}
