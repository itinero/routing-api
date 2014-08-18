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
            // enable logging and use the console as output.
            OsmSharp.Logging.Log.Enable();
            OsmSharp.Logging.Log.RegisterListener(
                new OsmSharp.WinForms.UI.Logging.ConsoleTraceListener());

            // read the nmbs feed.
            OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Reading NMBS/SNCB Feed...");
            var reader = new GTFSReader<GTFSFeed>(false);
            var nmbs = reader.Read(new GTFS.IO.GTFSDirectorySource(@"c:\work\osmsharp_data\nmbs\"));

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
            var feedDeLijn = reader.Read(new GTFS.IO.GTFSDirectorySource(@"c:\work\osmsharp_data\delijn\"));

            // prefix all ids in the feeds.
            foreach (var stop in feedDeLijn.Stops)
            {
                stop.Id = "delijn_" + stop.Id;
                stop.Tag = "De Lijn";
            }
            foreach (var item in feedDeLijn.Routes)
            {
                item.Id = "delijn_" + item.Id;
            }
            foreach (var stopTime in feedDeLijn.StopTimes)
            {
                stopTime.StopId = "delijn_" + stopTime.StopId;
                stopTime.TripId = "delijn_" + stopTime.TripId;
            }
            foreach (var trip in feedDeLijn.Trips)
            {
                trip.Id = "delijn_" + trip.Id;
                trip.RouteId = "delijn_" + trip.RouteId;
            }

            OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Reading MIVB/STIB Feed...");
            var feedMivb = reader.Read(new GTFS.IO.GTFSDirectorySource(@"c:\work\osmsharp_data\stib\"));

            // prefix all ids in the feeds.
            foreach (var stop in feedMivb.Stops)
            {
                stop.Id = "mivb_" + stop.Id;
                stop.Tag = "MIVB";
            }
            foreach (var item in feedMivb.Routes)
            {
                item.Id = "mivb_" + item.Id;
            }
            foreach (var stopTime in feedMivb.StopTimes)
            {
                stopTime.StopId = "mivb_" + stopTime.StopId;
                stopTime.TripId = "mivb_" + stopTime.TripId;
            }
            foreach (var trip in feedMivb.Trips)
            {
                trip.Id = "mivb_" + trip.Id;
                trip.RouteId = "mivb_" + trip.RouteId;
            }

            OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Creating trains instance...");
            var multiModalRouter = MultiModalRouter.CreateFrom(new FileInfo(@"c:\temp\belgium-latest.osm.pbf.routing").OpenRead(),
                new OsmRoutingInterpreter());
            //var multiModalRouter = MultiModalRouter.CreateFrom(new PBFOsmStreamSource(new FileInfo(@"c:\OSM\bin\belgium-latest.osm.pbf").OpenRead()),
            //    new OsmRoutingInterpreter());

            multiModalRouter.AddGTFSFeed(nmbs);

            OsmSharp.Service.Routing.MultiModal.SelfHost.Add("trains", multiModalRouter);

            OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Creating trainandbus instance...");
            multiModalRouter = MultiModalRouter.CreateFrom(new FileInfo(@"c:\temp\belgium-latest.osm.pbf.routing").OpenRead(),
                new OsmRoutingInterpreter());
            //multiModalRouter = MultiModalRouter.CreateFrom(new PBFOsmStreamSource(new FileInfo(@"c:\OSM\bin\waarschoot.osm.pbf").OpenRead()),
            //    new OsmRoutingInterpreter());
            multiModalRouter.AddGTFSFeed(nmbs);
            multiModalRouter.AddGTFSFeed(feedDeLijn);
            multiModalRouter.AddGTFSFeed(feedMivb);

            OsmSharp.Service.Routing.MultiModal.SelfHost.Add("trainandbus", multiModalRouter);
            SelfHost.Start(new Uri("http://localhost:1234/"));
        }
    }
}