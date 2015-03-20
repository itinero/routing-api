using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Osm.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Wrappers
{
    /// <summary>
    /// A routing service wrapper class around a router.
    /// </summary>
    public class RouterWrapper : RoutingServiceWrapperBase
    {
        /// <summary>
        /// Holds the router.
        /// </summary>
        private Router _router;

        /// <summary>
        /// Creates a new router wrapper.
        /// </summary>
        /// <param name="router"></param>
        public RouterWrapper(Router router)
        {
            _router = router;
        }

        /// <summary>
        /// Calculates a router along the given coordinates.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinates"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public override Route GetRoute(Vehicle vehicle, GeoCoordinate[] coordinates, bool complete)
        {
            // resolve all points.
            var resolved = _router.Resolve(vehicle, 0.0075f, coordinates);
            
            // TODO: implement complete boolean.
            // TODO: implement a route with via points.
            return _router.Calculate(vehicle, resolved[0], resolved[1]);
        }

        /// <summary>
        /// Calculates instructions for a given route.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        public override List<Instruction> GetInstructions(Vehicle vehicle, Route route)
        {
            return InstructionGenerator.Generate(route, new OsmRoutingInterpreter());
        }

        /// <summary>
        /// Converts the given route to a line string.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public override FeatureCollection GetFeatures(Route route)
        {
            var coordinates = route.GetPoints();
            var ntsCoordinates = coordinates.Select(x => { return new Coordinate(x.Longitude, x.Latitude); });
            var geometryFactory = new GeometryFactory();
            var lineString = geometryFactory.CreateLineString(ntsCoordinates.ToArray());
            var featureCollection = new FeatureCollection();

            var attributes = new AttributesTable();
            attributes.AddAttribute("osmsharp:total_time", route.TotalTime.ToInvariantString());
            attributes.AddAttribute("osmsharp:total_distance", route.TotalDistance.ToInvariantString());

            var feature = new Feature(lineString, attributes);

            featureCollection.Add(feature);
            return featureCollection;
        }

        /// <summary>
        /// Returns all network features in the given box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public override FeatureCollection GetNeworkFeatures(GeoCoordinateBox box)
        {
            // TODO: find a way to get nework features.
            return new FeatureCollection();
        }

        /// <summary>
        /// Returns true when the given vehicle is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public override bool SupportsVehicle(Vehicle vehicle)
        {
            return _router.SupportsVehicle(vehicle);
        }
    }
}
