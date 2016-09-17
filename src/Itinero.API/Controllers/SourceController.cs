using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Itinero.API.Controllers
{
    [Route("[controller]")]
    public class SourceController
    {
        // This controller is for development purposes
        private readonly IOptions<SourceOptions> _optionsAccessor;

        public SourceController(IOptions<SourceOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        [HttpGet]
        public string Get()
        {
            return _optionsAccessor.Value.FileLocation;
        }
    }
}
