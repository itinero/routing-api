using System;
using System.Collections.Generic;
using Itinero.LocalGeo;
using Itinero.Profiles;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Itinero.API.Routing;

namespace Itinero.API.Controllers
{
    [Route("[controller]")]
    public class RoutingController : Controller
    {
        [HttpGet]
        [Produces("application/json", "application/vnd.geo+json")]
        public Route Get(
            [FromQuery] float fromLat, 
            [FromQuery] float fromLon,
            [FromQuery] float toLat, 
            [FromQuery] float toLon, 
            [FromQuery] string profile = null)
        {
            if (!HasCorrectConfiguration()) return null;
            var routingProfile = GetProfile(profile);
            if (routingProfile == null) return null;

            var parameters = new Dictionary<string, object>(); // empty but compulsory
            var coordinates = new[] {new Coordinate(fromLat, fromLon), new Coordinate(toLat, toLon)};
            var result = RoutingInstances.GetDefault().Calculate(routingProfile, coordinates, parameters);
            return result.Value;
        }

        [HttpPost]
        public Route Post(
            [FromBody] Coordinate[] coordinates, 
            [FromQuery] string profile = null)
        {
            if (!HasCorrectConfiguration()) return null;
            var routingProfile = GetProfile(profile);
            if (routingProfile == null) return null;

            var parameters = new Dictionary<string, object>(); // empty but compulsory
            var result = RoutingInstances.GetDefault().Calculate(routingProfile, coordinates, parameters);
            return result.Value; // There is a custom RouteOutputFormatter registered for the Route class
        }

        private bool HasCorrectConfiguration()
        {
            if (!RoutingInstances.HasInstances || !HasProfiles())
            {
                HttpContext.Response.StatusCode = 500;
                return false;
            }
            return true;
        }

        private Profile GetProfile(string profile)
        {
            Profile routingProfile;
            if (string.IsNullOrWhiteSpace(profile))
            {
                routingProfile = Profile.GetAllRegistered().OrderBy(p => p.Name).First();
            }
            else if (!Profile.TryGet(profile, out routingProfile))
            {
                HttpContext.Response.StatusCode = 400;
                return null;
            }
            return routingProfile;
        }
        
        private static bool HasProfiles() => Profile.GetAllRegistered().Any();
    }
}
