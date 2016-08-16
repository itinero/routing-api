using System.Collections.Generic;
using Itinero.API.Instances;
using Itinero.LocalGeo;
using Itinero.Profiles;
using Microsoft.AspNetCore.Mvc;

namespace Itinero.API.Controllers
{
    [Route("api/[controller]")]
    public class RoutingController : Controller
    {
        // GET api/values
        [HttpGet]
        public Coordinate[] Get()
        {
            var instance = RoutingBootstrapper.Get("belgium");

            var coordinates = new [] { new Coordinate(50.8423f, 3.2355f), new Coordinate(50.8437f, 3.2489f) };
            var profile = Osm.Vehicles.Vehicle.Car.Fastest();
            var result = instance.Calculate(profile, coordinates, new Dictionary<string, object>());

            return result.Value.Shape;
        }
    }
}
