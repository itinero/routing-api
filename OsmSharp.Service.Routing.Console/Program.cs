using GTFS;
using Nancy.Hosting.Self;
using OsmSharp.Osm.PBF.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Transit.MultiModal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Console
{
    internal class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
           
            SelfHost.Start(new Uri("http://localhost:1234/"));
        }
    }
}