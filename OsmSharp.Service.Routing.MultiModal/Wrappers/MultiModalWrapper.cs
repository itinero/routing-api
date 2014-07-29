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

        public override Route GetRoute(DateTime departureTime, List<Vehicle> vehicles, GeoCoordinate[] coordinates, bool complete)
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
            var from = _multiModalRouter.Resolve(toFirstStop, coordinates[0]);
            var to = _multiModalRouter.Resolve(fromLastStop, coordinates[1]);

            return _multiModalRouter.CalculateTransit(departureTime, toFirstStop, interModal, fromLastStop, from, to);
        }

        public override List<Instruction> GetInstructions(List<Vehicle> vehicles, Route route)
        {
            return new List<Instruction>();
        }

        /// <summary>
        /// Converts the given route to a line string.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public override FeatureCollection GetFeatures(Route route)
        {
            return _multiModalRouter.GetFeatures(route, true);
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
    }
}
