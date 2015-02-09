using NetTopologySuite.Features;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using OsmSharp.Service.Routing.Transit.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.MultiModal
{
    /// <summary>
    /// Represents an abstract wrapper of functionality around a multi modal transit routing service.
    /// </summary>
    public abstract class MultiModalRouterWrapperBase
    {
        /// <summary>
        /// Calculates a route from/to passing by at least one of the intermediates using two different modi.
        /// </summary>
        /// <param name="vehicles"></param>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public abstract Route GetRouteAlongOne(List<Vehicle> vehicles, GeoCoordinate[] coordinates);

        /// <summary>
        /// Calculates a route along the given points.
        /// </summary>
        /// <param name="departureTime">The departure time at the first location.</param>
        /// <param name="vehicles">The vehicle profiles to use.</param>
        /// <param name="coordinates">The coordinates of the points to route along.</param>
        /// <param name="complete">Only output the route geometry if false.</param>
        /// <returns></returns>
        public abstract Route GetRoute(DateTime departureTime, List<Vehicle> vehicles, GeoCoordinate[] coordinates, bool complete);

        /// <summary>
        /// Calculates the weight to all the points within the given range.
        /// </summary>
        /// <param name="departureTime"></param>
        /// <param name="vehicles"></param>
        /// <param name="location"></param>
        /// <param name="max"></param>
        /// <param name="sampleZoom"></param>
        /// <returns></returns>
        public abstract IEnumerable<Tuple<GeoCoordinate, ulong, double>> GetWithinRange(DateTime departureTime, List<Vehicle> vehicles, GeoCoordinate location, double max, int sampleZoom);

        /// <summary>
        /// Calculates instructions for the given route and vehicle.
        /// </summary>
        /// <param name="vehicles"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        public abstract List<Instruction> GetInstructions(List<Vehicle> vehicles, Route route);

        /// <summary>
        /// Converts the given route to a feature collection.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="aggregated"></param>
        public abstract FeatureCollection GetFeatures(Route route, bool aggregated = true);

        /// <summary>
        /// Returns all networkfeatures in the given box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public abstract FeatureCollection GetNeworkFeatures(GeoCoordinateBox box);
    }
}
