using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Domain.Queries
{
    class RoutingQuery
    {
        public string loc { get; set; }

        public string vehicle { get; set; }

        public string instructions { get; set; }

        public string format { get; set; }

        public string complete { get; set; }
    }
}
