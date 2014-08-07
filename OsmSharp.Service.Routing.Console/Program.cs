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

            // read the nmbs feed.
            OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Reading NMBS Feed...");
            var reader = new GTFSReader<GTFSFeed>(false);
            var nmbs = reader.Read(new GTFS.IO.GTFSDirectorySource(@"d:\work\osmsharp_data\nmbs\"));

            // prefix all ids in the feeds.
            foreach (var stop in nmbs.Stops)
            {
                stop.Id = "nmbs_" + stop.Id;
                stop.Tag = "NMBS";
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

            OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Reading De Lijn Feed...");
            var feedDeLijn = reader.Read(new GTFS.IO.GTFSDirectorySource(@"d:\work\osmsharp_data\delijn\"));

            // prefix all ids in the feeds.
            foreach (var stop in feedDeLijn.Stops)
            {
                stop.Tag = "De Lijn";
            }

            OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Creating trains instance...");
            var multiModalRouter = MultiModalRouter.CreateFrom(new FileInfo(@"d:\temp\belgium-latest.simple.flat.routing").OpenRead(),
                new OsmRoutingInterpreter());
            multiModalRouter.AddGTFSFeed(nmbs);

            OsmSharp.Service.Routing.MultiModal.SelfHost.Add("trains", multiModalRouter);

            OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Creating trainandbus instance...");
            multiModalRouter = MultiModalRouter.CreateFrom(new FileInfo(@"d:\temp\belgium-latest.simple.flat.routing").OpenRead(),
                new OsmRoutingInterpreter());
            multiModalRouter.AddGTFSFeed(nmbs);
            multiModalRouter.AddGTFSFeed(feedDeLijn);
            OsmSharp.Service.Routing.MultiModal.SelfHost.Add("trainandbus", multiModalRouter);
            OsmSharp.Service.Routing.MultiModal.SelfHost.Start(new Uri("http://localhost:1234/"));
        }
    }
}