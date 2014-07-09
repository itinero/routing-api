using OsmSharp.Routing.Transit.MultiModal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.MultiModal.Wrappers
{
    /// <summary>
    /// A routing service wrapper for a multi modal router that can act as a regular routing instance.
    /// </summary>
    public class RoutingServiceWrapper : RoutingServiceWrapperBase
    {
        public RoutingServiceWrapper(MultiModalRouter multiModalRouter)
        {

        }

        public override OsmSharp.Routing.Route GetRoute(OsmSharp.Routing.Vehicle vehicle, Math.Geo.GeoCoordinate[] coordinates, bool complete)
        {
            throw new NotImplementedException();
        }

        public override List<OsmSharp.Routing.Instructions.Instruction> GetInstructions(OsmSharp.Routing.Vehicle vehicle, OsmSharp.Routing.Route route)
        {
            throw new NotImplementedException();
        }

        public override NetTopologySuite.Features.FeatureCollection GetFeatures(OsmSharp.Routing.Route route)
        {
            throw new NotImplementedException();
        }

        public override NetTopologySuite.Features.FeatureCollection GetNeworkFeatures(Math.Geo.GeoCoordinateBox box)
        {
            throw new NotImplementedException();
        }
    }
}
