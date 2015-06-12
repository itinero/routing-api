using OsmSharp.Routing.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Multimodal.Domain
{
    class SimpleRoute
    {
        public string Route { get; set; }

        public List<Instruction> Instructions { get; set; }
    }
}
