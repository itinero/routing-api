using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Multimodal.Domain.Queries
{
    class VertexAndTime
    {
        public double lat { get; set; }

        public double lon { get; set; }

        public double value { get; set; }

        public ulong id { get; set; }
    }
}
