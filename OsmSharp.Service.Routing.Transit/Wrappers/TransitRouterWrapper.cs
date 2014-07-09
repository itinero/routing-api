using OsmSharp.Routing.Transit;
using OsmSharp.Service.Routing.Transit.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Transit.Wrappers
{
    /// <summary>
    /// A wrapper for a TransitRouter.
    /// </summary>
    public class TransitRouterWrapper : TransitServiceWrapperBase
    {
        /// <summary>
        /// Holds the transit router.
        /// </summary>
        private TransitRouter _transitRouter;

        /// <summary>
        /// Creates a transit router wrapper.
        /// </summary>
        /// <param name="transitRouter"></param>
        public TransitRouterWrapper(TransitRouter transitRouter)
        {
            _transitRouter = transitRouter;
        }

        /// <summary>
        /// Returns the operator with the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Operator GetOperator(string id)
        {
            var agency = _transitRouter.GetAgency(id);

            return new Operator()
            {
                Id = agency.Id,
                Name = agency.Name
            };
        }

        /// <summary>
        /// Returns all the operators.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Operator> GetOperators()
        {
            return _transitRouter.GetAgencies().Select(x => { return new Operator() { Id = x.Id, Name = x.Name }; });
        }

        /// <summary>
        /// Returns all the operators for the given search string.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Operator> GetOperators(string query)
        {
            return _transitRouter.GetAgencies(query).Select(x => { return new Operator() { Id = x.Id, Name = x.Name }; });
        }

        /// <summary>
        /// Returns all the stops.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Stop> GetStops()
        {
            return _transitRouter.GetStops().Select(x => { return new Stop() { Id = x.Id, Name = x.Name, OperatorId = string.Empty }; });
        }

        /// <summary>
        /// Returns all the stops for the given search string.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public override IEnumerable<Stop> GetStops(string query)
        {
            return _transitRouter.GetStops(query).Select(x => { return new Stop() { Id = x.Id, Name = x.Name, OperatorId = string.Empty }; });
        }

        /// <summary>
        /// Returns all the stops for the given operator.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <returns></returns>
        public override IEnumerable<Stop> GetStopsForOperator(string operatorId)
        {
            return _transitRouter.GetStopsForAgency(operatorId).Select(x => { return new Stop() { Id = x.Id, Name = x.Name, OperatorId = string.Empty }; });
        }

        /// <summary>
        /// Returns all the stops for the given operator and the given search string.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public override IEnumerable<Domain.Stop> GetStopsForOperator(string operatorId, string query)
        {
            return _transitRouter.GetStopsForAgency(operatorId, query).Select(x => { return new Stop() { Id = x.Id, Name = x.Name, OperatorId = string.Empty }; });
        }
    }
}
