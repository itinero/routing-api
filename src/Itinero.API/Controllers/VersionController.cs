using Microsoft.AspNetCore.Mvc;

namespace Itinero.API.Controllers
{
    [Route("[controller]")]
    public class VersionController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "0.9.0";

        }
    }
}
