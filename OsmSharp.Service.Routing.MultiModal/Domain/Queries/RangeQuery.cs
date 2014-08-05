using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.MultiModal.Domain.Queries
{
    class RangeQuery
    {
        public string loc { get; set; }

        public string vehicle { get; set; }

        public string time { get; set; }

        public string max { get; set; }
    }
}
