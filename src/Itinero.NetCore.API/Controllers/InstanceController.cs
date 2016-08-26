using System.Collections.Generic;
using Itinero.API.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Itinero.API.Controllers
{
    [Route("[controller]")]
    public class InstanceController : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return Instances.GetRegisteredNames();
        }
    }
}
