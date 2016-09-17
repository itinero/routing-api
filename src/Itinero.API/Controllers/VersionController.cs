using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Itinero.API.Controllers
{
    [Route("[controller]")]
    public class VersionController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return Version;
        }

        static string Version
        {
            get
            {
                var assembly = typeof(VersionController).GetTypeInfo().Assembly;
                // In some PCL profiles the above line is: var assembly = typeof(MyType).Assembly;
                var assemblyName = new AssemblyName(assembly.FullName);
                return assemblyName.Version.Major + "." + assemblyName.Version.Minor;
            }
        }
    }
}
