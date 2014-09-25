namespace OsmSharp.Service.Routing.ASP
{
    using GTFS;
    using Nancy;
    using OsmSharp.Routing.Osm.Interpreter;
    using OsmSharp.Routing.Transit.MultiModal;
    using System.IO;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        // The bootstrapper enables you to reconfigure the composition of the framework,
        // by overriding the various methods and properties.
        // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper

        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            // enable logging and use the console as output.
            OsmSharp.Logging.Log.Enable();
            OsmSharp.Logging.Log.RegisterListener(
                new OsmSharp.WinForms.UI.Logging.ConsoleTraceListener());

            // create the reader.
            var reader = new GTFSReader<GTFSFeed>(false);

            //// read nl.
            //OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Reading NL Feed...");
            //var feedNl = BuildFeed(reader, @"d:\work\osmsharp_data\nl\", "nl_", "NL");

            //// read tec.
            //OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Reading TEC Feed...");
            //var feedTec = BuildFeed(reader, @"d:\work\osmsharp_data\tec\", "tec_", "TEC");

            // read the nmbs feed.
            OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Reading NMBS/SNCB Feed...");
            var feedNmbs = BuildFeed(reader, @"d:\work\osmsharp_data\nmbs\", "nmbs_", "NMBS");

            //// read delijn feed.
            //OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Reading De Lijn Feed...");
            //var feedDeLijn = BuildFeed(reader, @"d:\work\osmsharp_data\delijn\", "delijn_", "De Lijn");

            //// read mivb.
            //OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Reading MIVB/STIB Feed...");
            //var feedMivb = BuildFeed(reader, @"d:\work\osmsharp_data\stib\", "mivb_", "MIVB");

            OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Creating trains instance...");
            var multiModalRouter = MultiModalRouter.CreateFrom(new FileInfo(@"d:\OSM\bin\belgium-latest.osm.pbf.routing").OpenRead(),
                new OsmRoutingInterpreter());

            multiModalRouter.AddGTFSFeed(feedNmbs);

            OsmSharp.Service.Routing.MultiModal.ApiBootstrapper.Add("trains", multiModalRouter);

            OsmSharp.Logging.Log.TraceEvent("Main", Logging.TraceEventType.Information, "Creating trainandbus instance...");
            multiModalRouter = MultiModalRouter.CreateFrom(new FileInfo(@"d:\OSM\bin\belgium-latest.osm.pbf.routing").OpenRead(),
                new OsmRoutingInterpreter());
            multiModalRouter.AddGTFSFeed(feedNmbs);

            OsmSharp.Service.Routing.MultiModal.ApiBootstrapper.Add("trainandbus", multiModalRouter);
        }

        /// <summary>
        /// Builds a GTFS feed from the given path and prefixes with the given prefix.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="path"></param>
        /// <param name="prefix"></param>
        /// <param name="stopTag"></param>
        /// <returns></returns>
        private static GTFSFeed BuildFeed(GTFSReader<GTFSFeed> reader, string path, string prefix, string stopTag)
        {
            var feed = reader.Read(new GTFS.IO.GTFSDirectorySource(path));

            // prefix all ids in the feeds.
            foreach (var stop in feed.GetStops())
            {
                stop.Id = prefix + stop.Id;
                stop.Tag = stopTag;
            }
            foreach (var item in feed.GetRoutes())
            {
                item.Id = prefix + item.Id;
            }
            foreach (var stopTime in feed.GetStopTimes())
            {
                stopTime.StopId = prefix + stopTime.StopId;
                stopTime.TripId = prefix + stopTime.TripId;
            }
            foreach (var trip in feed.GetTrips())
            {
                trip.Id = prefix + trip.Id;
                trip.RouteId = prefix + trip.RouteId;
            }
            return feed;
        }
    }
}