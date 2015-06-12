using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Multimodal.Domain.Queries
{
    class RoutingQuery
    {
        public string loc { get; set; }

        public string vehicle { get; set; }

        public string instructions { get; set; }

        public string format { get; set; }

        public string complete { get; set; }

        public string modi { get; set; }

        public string operators { get; set; }

        public string time { get; set; }

        public string departure { get; set; }

        public string type { get; set; }
    }
}
