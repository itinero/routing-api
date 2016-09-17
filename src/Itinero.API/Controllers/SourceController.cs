using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Itinero.API.Controllers
{
    [Route("[controller]")]
    public class SourceController
    {
        // This controller is for development purposes
        private readonly IOptions<SourceOptions> _options;

        public SourceController(IOptions<SourceOptions> options)
        {
            _options = options;
        }

        [HttpGet]
        public string Get()
        {
            return _options.Value.Source;
        }
    }
}
