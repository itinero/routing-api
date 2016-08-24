using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Itinero.API.Formatters
{
    public class RouteOutputFormatter : IOutputFormatter
    {
        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return context.Object is Route;
            // todo check on content type :  && context.ContentType == new StringSegment("application/json")
        }

        public Task WriteAsync(OutputFormatterWriteContext context)
        {
            return context.HttpContext.Response.WriteAsync(((Route) context.Object).ToJson());
        }
    }
}
