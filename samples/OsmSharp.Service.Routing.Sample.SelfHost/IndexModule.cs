using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Sample.SelfHost
{
    /// <summary>
    /// A nancy module serving just the test page.
    /// </summary>
    public class IndexModule : NancyModule
    {
        /// <summary>
        /// Creates a new instance of the tile module.
        /// </summary>
        public IndexModule()
        {
            Get["default"] = parameters =>
            {
                return View["index"];
            };
        }
    }
}
