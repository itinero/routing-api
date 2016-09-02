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
            var routingInstance = GetInstance();
            var routingProfile = GetProfile(profile);
            var coordinates = new[] {new Coordinate(fromLat, fromLon), new Coordinate(toLat, toLon)};
            var result = routingInstance.Calculate(routingProfile, coordinates, new Dictionary<string, object>());
            return result.Value;
        }

        [HttpPost]
        public Route Post(
            [FromBody] Coordinate[] coordinates, 
            [FromQuery] string profile = null, 
            [FromQuery] string instance = null)
        {
            var routingProfile = GetProfile(profile);
            var result = RoutingInstances.GetDefault().Calculate(routingProfile, coordinates, new Dictionary<string, object>());
            return result.Value;
        }

        private static Profile GetProfile(string profile)
        {
            Profile routingProfile;
            if (string.IsNullOrWhiteSpace(profile))
            {
                routingProfile = Profile.GetAllRegistered().OrderBy(p => p.Name).First();
            }
            else if (!Profile.TryGet(profile, out routingProfile))
            {
                throw new Exception("Profile was not found");
            }
            return routingProfile;
        }

        private static IRoutingModuleInstance GetInstance()
        {
            return RoutingInstances.Get(RoutingInstances.GetRegisteredNames().OrderBy(i => i).First());
        }
    }
}
