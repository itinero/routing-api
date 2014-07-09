using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Domain
{
    class CompleteRoute
    {
        public Route Route { get; set; }

        public List<Instruction> Instructions { get; set; }
    }
}
