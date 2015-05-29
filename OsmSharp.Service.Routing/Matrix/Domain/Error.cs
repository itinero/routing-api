using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Matrix.Domain
{
    public class Error
    {
        public string type { get; set; }
        public int index { get; set; }
        public string message { get; set; }
    }
}
