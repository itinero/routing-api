using OsmSharp.Routing.Transit.MultiModal;
using OsmSharp.Service.Routing.Transit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.MultiModal.Wrappers
{
    /// <summary>
    /// Transit service wrapper for a multi modal router that can act as a transit-only router.
    /// </summary>
    public class TransitServiceWrapper : TransitServiceWrapperBase
    {
        public TransitServiceWrapper(MultiModalRouter multiModalRouter)
        {

        }

        public override Transit.Domain.Operator GetOperator(string id)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Transit.Domain.Operator> GetOperators()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Transit.Domain.Operator> GetOperators(string query)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Transit.Domain.Stop> GetStops()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Transit.Domain.Stop> GetStops(string query)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Transit.Domain.Stop> GetStopsForOperator(string operatorId)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Transit.Domain.Stop> GetStopsForOperator(string operatorId, string query)
        {
            throw new NotImplementedException();
        }
    }
}
