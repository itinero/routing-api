using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.MultiModal.Domain.Queries
{
    class BoxQuery
    {
        public string left { get; set; }

        public string right { get; set; }

        public string top { get; set; }

        public string bottom { get; set; }
    }
}
