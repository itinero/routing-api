using System.Collections.Generic;
using Itinero.Profiles;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Itinero.API.Controllers
{
    [Route("api/[controller]")]
    public class ProfileController : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return Profile.GetAllRegistered().Select(p => p.Name);
        }
    }
}
