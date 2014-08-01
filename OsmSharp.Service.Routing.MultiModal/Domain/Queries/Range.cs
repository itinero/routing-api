using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.MultiModal.Domain.Queries
{
    class Range
    {
        public double max { get; set; }

        public VertexAndTime[] data { get; set; }
    }
}
