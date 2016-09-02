
using Itinero.API.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Itinero.API.Controllers
{
    [Route("[controller]")]
    public class MetaController : Controller
    {
        [HttpGet]
        public RouterDb Get()
        {
            var instance = RoutingInstances.Get(RoutingInstances.GetRegisteredNames().First());
            return instance.Router.Db;
        }
    }
}
