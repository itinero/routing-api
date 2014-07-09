using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing
{
    /// <summary>
    /// An abstract representation of routing service wrapper.
    /// </summary>
    public abstract class RoutingServiceWrapperBase
    {
        /// <summary>
        /// Calculates a route along the given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile to use.</param>
        /// <param name="coordinates">The coordinates of the points to route along.</param>
        /// <param name="complete">Only output the route geometry if false.</param>
        /// <returns></returns>
        public abstract Route GetRoute(Vehicle vehicle, GeoCoordinate[] coordinates, bool complete);

        /// <summary>
        /// Calculates instructions for the given route and vehicle.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        public abstract List<Instruction> GetInstructions(Vehicle vehicle, Route route);

        /// <summary>
        /// Converts the given route to a feature collection.
        /// </summary>
        /// <param name="route"></param>
        public abstract FeatureCollection GetFeatures(Route route);

        /// <summary>
        /// Returns all networkfeatures in the given box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public abstract FeatureCollection GetNeworkFeatures(GeoCoordinateBox box);
    }
}
