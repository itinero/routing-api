using GTFS;
using Nancy.Hosting.Self;
using OsmSharp.Osm.PBF.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Transit.GTFS;
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
            // enable logging and use the console as output.
            OsmSharp.Logging.Log.Enable();
            OsmSharp.Logging.Log.RegisterListener(
                new OsmSharp.WinForms.UI.Logging.ConsoleTraceListener());

            //// read the sample feed.
            //var reader = new GTFSReader<GTFSFeed>(false);
            //reader.DateTimeReader = (dateString) =>
            //{
            //    var year = int.Parse(dateString.Substring(0, 4));
            //    var month = int.Parse(dateString.Substring(4, 2));
            //    var day = int.Parse(dateString.Substring(6, 2));
            //    return new System.DateTime(year, month, day);
            //};
            //var feed = reader.Read(new GTFS.IO.GTFSDirectorySource(@"d:\work\osmsharp_data\nmbs\"));

            //SelfHost.Start(new Uri("http://localhost:1234/"), feed);

            //// start host.
            //using (var host = new NancyHost(new Uri("http://localhost:1234")))
            //{
            //    host.Start();
            //    System.Console.ReadLine();
            //}

            //SelfHost.Start(new Uri("http://localhost:1234/"), Router.CreateLiveFrom(
            //    new PBFOsmStreamSource(new FileInfo(@"D:\OSM\bin\kempen-big.osm.pbf").OpenRead()), new OsmRoutingInterpreter()));

            // read the sample feed.
            var reader = new GTFSReader<GTFSFeed>(false);
            reader.DateTimeReader = (dateString) =>
            {
                var year = int.Parse(dateString.Substring(0, 4));
                var month = int.Parse(dateString.Substring(4, 2));
                var day = int.Parse(dateString.Substring(6, 2));
                return new System.DateTime(year, month, day);
            };
            var nmbs = reader.Read(new GTFS.IO.GTFSDirectorySource(@"d:\work\osmsharp_data\nmbs\"));

            // prefix all ids in the feeds.
            foreach (var stop in nmbs.Stops)
            {
                stop.Id = "nmbs_" + stop.Id;
            }
            foreach (var item in nmbs.Routes)
            {
                item.Id = "nmbs_" + item.Id;
            }
            foreach (var stopTime in nmbs.StopTimes)
            {
                stopTime.StopId = "nmbs_" + stopTime.StopId;
                stopTime.TripId = "nmbs_" + stopTime.TripId;
            }
            foreach (var trip in nmbs.Trips)
            {
                trip.Id = "nmbs_" + trip.Id;
                trip.RouteId = "nmbs_" + trip.RouteId;
            }

            var feedDeLijn = reader.Read(new GTFS.IO.GTFSDirectorySource(@"d:\work\osmsharp_data\delijn\"));
            var multiModalRouter = MultiModalRouter.CreateFrom(new FileInfo(@"d:\temp\belgium-latest.simple.flat.routing").OpenRead(),
                new OsmRoutingInterpreter());
            multiModalRouter.AddGTFSFeed(nmbs);
            multiModalRouter.AddGTFSFeed(feedDeLijn);

            OsmSharp.Service.Routing.MultiModal.SelfHost.Start(new Uri("http://localhost:1234/"), multiModalRouter);
        }
    }
}