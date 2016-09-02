using System.Collections.Generic;
using Itinero.API.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Itinero.API.Models;
using Itinero.Profiles;

namespace Itinero.API.Controllers
{
    [Route("[controller]")]
    public class MetaController : Controller
    {
        [HttpGet]
        public MetaModel Get()
        {
            if (!RoutingInstances.HasInstances)
            {
                HttpContext.Response.StatusCode = 500;
                return null;
            }

            var instance = RoutingInstances.Get(RoutingInstances.GetRegisteredNames().First());
            return Create(instance.Router.Db, Profile.GetAllRegistered().Select(p => p.Name));
        }

        public static MetaModel Create(RouterDb routerDb, IEnumerable<string> profiles)
        {
            return new MetaModel(
                routerDb.Guid.ToString(),
                routerDb.Meta,
                profiles,
                routerDb.GetContractedProfiles());
        }
    }
}
