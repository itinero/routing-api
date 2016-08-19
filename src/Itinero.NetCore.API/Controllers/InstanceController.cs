using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Itinero.API.Controllers
{
    [Route("api/[controller]")]
    public class InstanceController : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return RoutingBootstrapper.GetNamesRegistered();
        }
    }
}
