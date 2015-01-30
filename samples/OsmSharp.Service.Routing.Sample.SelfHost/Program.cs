using Nancy.Hosting.Self;
using OsmSharp.Osm.PBF.Streams;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Routing;
using OsmSharp.Routing.Osm.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Sample.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            // enable logging and use the console as output.
            OsmSharp.Logging.Log.Enable();
            OsmSharp.Logging.Log.RegisterListener(
                new OsmSharp.WinForms.UI.Logging.ConsoleTraceListener());

            // create router.
            using(var source = new FileInfo(@"kempen-big.osm.pbf").OpenRead())
            {
                var pbfSource = new PBFOsmStreamSource(source);
                var progress = new OsmStreamFilterProgress();
                progress.RegisterSource(pbfSource);
                var router = Router.CreateCHFrom(progress, new OsmRoutingInterpreter(), Vehicle.Car);

                OsmSharp.Service.Routing.ApiBootstrapper.Add("default", router);
            }

            var uri = new Uri("http://localhost:1234");
            using (var host = new NancyHost(uri))
            {
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();
            }
        }
    }
}
