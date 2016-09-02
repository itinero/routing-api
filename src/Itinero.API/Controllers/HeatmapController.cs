using Itinero.API.Routing;
using Itinero.LocalGeo;
using Microsoft.AspNetCore.Mvc;
using Itinero.Algorithms.Networks.Analytics.Heatmaps;
using Itinero.API.Helpers;
using Itinero.Profiles;

namespace Itinero.API.Controllers
{
    [Route("[controller]")]
    public class HeatmapController : Controller
    {
        [HttpGet]
        public HeatmapResult Get(
            [FromQuery] float lat,
            [FromQuery] float lon,
            [FromQuery] int limit,
            [FromQuery] int detailLevel,
            [FromQuery] string profile = null)
        {
            Profile routingProfile;
            if (!ProfileHelper.TryGetProfile(profile, out routingProfile))
            {
                HttpContext.Response.StatusCode = 400;
                return null;
            }
            
            var coordinate = new Coordinate(lat, lon);
            return RoutingInstances.GetDefault().Router.CalculateHeatmap(routingProfile, coordinate,
                limit, detailLevel);
        }
    }
}
