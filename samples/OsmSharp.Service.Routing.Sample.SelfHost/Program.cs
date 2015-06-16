using GTFS;
using GTFS.IO;
using Nancy.Hosting.Self;
using OsmSharp.Math.Geo.Projections;
using OsmSharp.Osm.PBF.Streams;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.Multimodal.Data;
using OsmSharp.Routing.Vehicles;
using OsmSharp.Service.Tiles;
using OsmSharp.UI.Map.Layers;
using OsmSharp.UI.Map.Styles.MapCSS;
using OsmSharp.UI.Map.Styles.Streams;
using OsmSharp.UI.Renderer.Scene;
using OsmSharp.UI.Renderer.Scene.Simplification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            using (var source = new FileInfo(@"D:\Dropbox\Dropbox\SharpSoftware\Projects\Eurostation ReLive\Server_Dropbox\OSM\relive_kortrijk\kortrijk.osm").OpenRead())
            {
                var data = OsmSharp.Routing.Osm.Streams.GraphOsmStreamTarget.Preprocess(
                    new XmlOsmStreamSource(source), new OsmRoutingInterpreter());

                var reader = new GTFSReader<GTFSFeed>();
                var gtfsFeed = reader.Read<GTFSFeed>(new GTFSDirectorySource(@"D:\Dropbox\Dropbox\SharpSoftware\Projects\Eurostation ReLive\Server_Dropbox\GTFS\relive_kortrijk\delijn_kortrijk_2015_05-06-07"));
                var connectionsDb = new GTFSConnectionsDb(gtfsFeed);
                var multimodalConnectionsDb = new MultimodalConnectionsDb(data, connectionsDb, new OsmRoutingInterpreter(), Vehicle.Pedestrian);

                ApiBootstrapper.AddOrUpdate("default", new OsmSharp.Service.Routing.Multimodal.MultimodalRouterWrapperBase(multimodalConnectionsDb));
            }

            // initialize mapcss interpreter.
            var mapCSSInterpreter = new MapCSSInterpreter(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Service.Routing.Sample.SelfHost.custom.mapcss"),
                new MapCSSDictionaryImageSource());

            using (var source = new FileInfo(@"D:\Dropbox\Dropbox\SharpSoftware\Projects\Eurostation ReLive\Server_Dropbox\OSM\relive_kortrijk\kortrijk.osm").OpenRead())
            {
                var pbfSource = new XmlOsmStreamSource(source);
                var scene = new Scene2D(new OsmSharp.Math.Geo.Projections.WebMercator(), new List<float>(new float[] {
                16, 14, 12, 10 }));
                var target = new StyleOsmStreamSceneTarget(
                    mapCSSInterpreter, scene, new WebMercator());
                var progress = new OsmStreamFilterProgress();
                progress.RegisterSource(pbfSource);
                target.RegisterSource(progress);
                target.Pull();

                // create a new instance (with a cache).
                var instance = new RenderingInstance();
                instance.Map.AddLayer(new LayerScene(scene));

                // add a default test instance.
                OsmSharp.Service.Tiles.ApiBootstrapper.AddInstance("default", instance);
            }

            var uri = new Uri("http://localhost:1234");
            using (var host = new NancyHost(uri))
            {
                host.Start();

                OsmSharp.Logging.Log.TraceEvent("Program", OsmSharp.Logging.TraceEventType.Information, "Nancyhost now listening @ http://localhost:1234");
                System.Diagnostics.Process.Start("http://localhost:1234/default");

                Console.ReadLine();
            }
        }
    }
}
