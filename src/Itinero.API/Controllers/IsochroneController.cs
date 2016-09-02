using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Itinero.Algorithms.Networks.Analytics.Isochrones;
using Itinero.API.Routing;
using Itinero.LocalGeo;
using Itinero.Profiles;
using System.Linq;
using Itinero.API.Helpers;

namespace Itinero.API.Controllers
{
    [Route("[controller]")]
    public class IsochroneController : Controller
    {
        [HttpGet]
        public List<Polygon> Get(
            [FromQuery] float lat,
            [FromQuery] float lon,
            [FromQuery] float[] limits,
            [FromQuery] int detailLevel,
            [FromQuery] string profile = null)
        {
            Profile routingProfile;
            if (ProfileHelper.TryGetProfile(profile, out routingProfile))
            {
                HttpContext.Response.StatusCode = 400;
                return null;
            }
            if (routingProfile == null) return null;
            var coordinate = new Coordinate(lat, lon);
            return RoutingInstances.GetDefault().Router.CalculateIsochrones(routingProfile, coordinate,
                new List<float>(limits), detailLevel);
        }


    }
}
