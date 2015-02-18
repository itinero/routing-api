using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Transit.MultiModal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.MultiModal.Wrappers
{
    public class MultiModalWrapper : MultiModalRouterWrapperBase
    {
        private MultiModalRouter _multiModalRouter;

        public MultiModalWrapper(MultiModalRouter multiModalRouter)
        {
            _multiModalRouter = multiModalRouter;
        }

        public override Route GetRouteAlongOne(List<Vehicle> vehicles, GeoCoordinate[] coordinates)
        {
            var source = _multiModalRouter.Resolve(vehicles[0], coordinates[0]);
            var target = _multiModalRouter.Resolve(vehicles[1], coordinates[coordinates.Length - 1]);

            var alongs = new List<RouterPoint>();
            for(int idx = 1; idx < coordinates.Length - 1; idx++)
            {
                var along = _multiModalRouter.Resolve(vehicles[0], coordinates[idx]);
                if(along != null)
                {
                    alongs.Add(along);
                }
            }
            if(alongs.Count > 0)
            {
                return _multiModalRouter.CalculateAlongOne(vehicles[0], source, vehicles[1], target, alongs.ToArray());
            }
            return null;
        }

        public override Route GetRoute(DateTime departureTime, List<Vehicle> vehicles, GeoCoordinate[] coordinates, HashSet<string> operators, bool complete)
        {
            var toFirstStop = vehicles[0];
            var interModal = vehicles[0];
            var fromLastStop = vehicles[0];

            if (vehicles.Count == 1)
            { // the intermode is always pedestrian when only one profile given.
                interModal = Vehicle.GetByUniqueName("Pedestrian");
            }
            else if (vehicles.Count == 2)
            { // the intermode is always pedestrian when only two profiles given.
                interModal = Vehicle.GetByUniqueName("Pedestrian");
                fromLastStop = vehicles[1];
            }
            else if (vehicles.Count >= 3)
            { // ignore vehicle 4 etc...
                interModal = vehicles[1];
                fromLastStop = vehicles[2];
            }

            // resolve points with the correct profiles.
            RouterPoint from, to;
            lock (_multiModalRouter)
            {
                from = _multiModalRouter.Resolve(toFirstStop, coordinates[0]);
                to = _multiModalRouter.Resolve(fromLastStop, coordinates[1]);
            }

            HashSet<string> operatorSet = null; ;
            if(operators !=null)
            {
                operatorSet = new HashSet<string>();
                foreach(var op in operators)
                {
                    operatorSet.Add(op);
                }
            }

            return _multiModalRouter.CalculateTransit(departureTime, toFirstStop, interModal, fromLastStop, from, to, operatorSet);
        }

        public override IEnumerable<Tuple<GeoCoordinate, ulong, double>> GetWithinRange(DateTime departureTime, List<Vehicle> vehicles, GeoCoordinate location, double max, int sampleZoom)
        {
            var toFirstStop = vehicles[0];
            var interModal = vehicles[0];
            var fromLastStop = vehicles[0];

            if (vehicles.Count == 1)
            { // the intermode is always pedestrian when only one profile given.
                interModal = Vehicle.GetByUniqueName("Pedestrian");
            }
            else if (vehicles.Count == 2)
            { // the intermode is always pedestrian when only two profiles given.
                interModal = Vehicle.GetByUniqueName("Pedestrian");
                fromLastStop = vehicles[1];
            }
            else if (vehicles.Count >= 3)
            { // ignore vehicle 4 etc...
                interModal = vehicles[1];
                fromLastStop = vehicles[2];
            }

            // resolve points with the correct profiles.
            RouterPoint from;
            lock (_multiModalRouter)
            {
                from = _multiModalRouter.Resolve(toFirstStop, location);
            }

            return _multiModalRouter.CalculateTransitWithin(departureTime, toFirstStop, interModal, fromLastStop, from, max, sampleZoom);
        }

        public override List<Instruction> GetInstructions(List<Vehicle> vehicles, Route route)
        {
            return new List<Instruction>();
        }

        /// <summary>
        /// Converts the given route to a line string.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="aggregated"></param>
        /// <returns></returns>
        public override FeatureCollection GetFeatures(Route route, bool aggregated = true)
        {
            return _multiModalRouter.GetFeatures(route, aggregated);
        }

        /// <summary>
        /// Returns all network features in the given box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public override FeatureCollection GetNeworkFeatures(GeoCoordinateBox box)
        {
            return _multiModalRouter.GetNeworkFeatures(box);
        }
    }
}
