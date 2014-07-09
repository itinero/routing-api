// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using Nancy;
using Nancy.ModelBinding;
using OsmSharp.Service.Routing.Transit;
using OsmSharp.Service.Routing.Transit.Domain;
using OsmSharp.Service.Routing.Transit.Domain.Queries;
using System.Linq;

namespace OsmSharp.Service.Routing
{
    public class TransitModule : NancyModule
    {
        public TransitModule()
        {
            Get["/transit/status"] = _ =>
                {
                    if(Bootstrapper.IsInitialized())
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(new Status()
                        {
                            Available = true,
                            Info = "Initialized."
                        });
                    }
                    return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(new Status()
                    {
                        Available = false,
                        Info = "Not initialized."
                    });
                };
            Get["/transit/operators"] = _ =>
                {
                    // check if initialized.
                    if(!Bootstrapper.IsInitialized())
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.ServiceUnavailable);
                    }

                    // bind the query if any.
                    var query = this.Bind<SearchQuery>();

                    var operators = new Operator[0];
                    if (!string.IsNullOrWhiteSpace(query.q))
                    { // there is a 'q' property, this is a search request.
                        operators = Bootstrapper.TransitServiceInstance.GetOperators(query.q).ToArray();
                    }
                    else
                    { // get all operators.
                        operators = Bootstrapper.TransitServiceInstance.GetOperators().ToArray();
                    }
                    return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(operators);
                };
            Get["/transit/operators/{id}"] = _ =>
            {
                // check if initialized.
                if (!Bootstrapper.IsInitialized())
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.ServiceUnavailable);
                }

                // bind the id query if any.
                var query = this.Bind<IdQuery>();

                if (query != null && !string.IsNullOrWhiteSpace(query.id))
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(
                        Bootstrapper.TransitServiceInstance.GetOperator(query.id));
                }
                return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
            };
            Get["/transit/stops"] = _ =>
            {
                // check if initialized.
                if (!Bootstrapper.IsInitialized())
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.ServiceUnavailable);
                }

                // bind the query if any.
                var query = this.Bind<SearchQuery>();

                var stops = new Stop[0];
                if (!string.IsNullOrWhiteSpace(query.q))
                { // there is a 'q' property, this is a search request.
                    stops = Bootstrapper.TransitServiceInstance.GetStops(query.q).ToArray();
                }
                else
                { // get all operators.
                    stops = Bootstrapper.TransitServiceInstance.GetStops().ToArray();
                }
                return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(stops);
            };
            Get["/transit/stops/{operatorid}"] = _ =>
            {
                // check if initialized.
                if (!Bootstrapper.IsInitialized())
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.ServiceUnavailable);
                }

                // bind the id query if any.
                var query = this.Bind<IdAndSearchQuery>();

                var stops = new Stop[0];
                if (query != null)
                { // there is a query.
                    if(!string.IsNullOrWhiteSpace(query.q))
                    { // there is a search.
                        stops = Bootstrapper.TransitServiceInstance.GetStopsForOperator(query.id, query.q).ToArray();
                    }
                    else
                    {
                        stops = Bootstrapper.TransitServiceInstance.GetStopsForOperator(query.id).ToArray();
                    }
                    return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(stops);
                }
                return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
            };
        }
    }
}