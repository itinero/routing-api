// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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
using OsmSharp.Math.Geo;
using System;
using System.Collections.Generic;

namespace OsmSharp.Service.Routing.Matrix
{
    /// <summary>
    /// The Matric API nancy module.
    /// </summary>
    public class MatrixModule : NancyModule
    {
        /// <summary>
        /// Creates the matrix module.
        /// </summary>
        public MatrixModule()
        {
            Put["{instance}/matrix"] = _ =>
            {
                return this.PutInstanceMatrix(_);
            };
        }

        /// <summary>
        /// Called on PUT '/{instance}/matrix'.
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private dynamic PutInstanceMatrix(dynamic _)
        {
            this.EnableCors();

            // get instance and check if active.
            string instance = _.instance;
            if (!ApiBootstrapper.IsActive(instance))
            { // oeps, instance not active!
                return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
            }

            // bind the request if any.
            var request = this.Bind<Domain.Request>();

            // check request.
            if (request == null)
            {
                return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable);
            }
            if (request.locations == null && request.sources == null)
            {
                return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable);
            }
            if (request.locations == null && (request.sources == null || request.targets == null))
            {
                return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable);
            }
            if (request.output != null)
            { // check output contents.
                foreach(var output in request.output)
                {
                    if(!(output.Equals(Domain.Request.DistanceOutputOption) ||
                        output.Equals(Domain.Request.TimesOutputOption) || 
                        output.Equals(Domain.Request.WeightsOutputOption)))
                    { // this output-option does not equal one of the options.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(
                            string.Format("Invalid output option: {0}", output));
                    }
                }
            }

            // set defaults if needed.
            if (request.profile == null)
            { // set default profile.
                request.profile = new Domain.Profile() { vehicle = "car" };
            }
            if (request.output == null || request.output.Length == 0)
            { // set default output.
                request.output = new string[] { "times" };
            }

            // build sources and targets arrays.
            GeoCoordinate[] sources;
            GeoCoordinate[] targets;
            if(request.locations != null)
            { // NxN matrix, source and targets are identical.
                sources = new GeoCoordinate[request.locations.Length];
                for(var i = 0; i < request.locations.Length; i++)
                {
                    sources[i] = new GeoCoordinate(request.locations[i][1],
                        request.locations[i][0]);
                }
                targets = sources;
            }
            else
            { // NxM matrix, sources and targets are different.
                sources = new GeoCoordinate[request.sources.Length];
                for (var i = 0; i < request.sources.Length; i++)
                {
                    sources[i] = new GeoCoordinate(request.sources[i][1],
                        request.sources[i][0]);
                }
                targets = new GeoCoordinate[request.targets.Length];
                for (var i = 0; i < request.targets.Length; i++)
                {
                    targets[i] = new GeoCoordinate(request.targets[i][1],
                        request.targets[i][0]);
                }
            }

            // build profile.
            var vehicle = OsmSharp.Routing.Vehicles.Vehicle.GetByUniqueName(request.profile.vehicle);

            // calculate matrices.
            var api = ApiBootstrapper.Get(instance);
            Tuple<string, int, string>[] errors;
            var matrices = api.GetMatrix(vehicle, sources, targets, request.output, out errors);

            // remove target errors and adapt source errors when only a locations array was in the request.
            if(request.locations != null)
            {
                var errorsList = new List<Tuple<string, int, string>>();
                foreach(var error in errors)
                {
                    if(error.Item1 == "source")
                    {
                        errorsList.Add(new Tuple<string, int, string>(null,
                            error.Item2, error.Item3));
                    }
                }
                errors = errorsList.ToArray();
            }

            // build response.
            var response = new Matrix.Domain.Response();
            foreach(var matrix in matrices)
            {
                if(matrix.Item1 == Matrix.Domain.Request.DistanceOutputOption)
                {
                    response.distances = matrix.Item2;
                }
                else if (matrix.Item1 == Matrix.Domain.Request.TimesOutputOption)
                {
                    response.times = matrix.Item2;
                }
                else if (matrix.Item1 == Matrix.Domain.Request.WeightsOutputOption)
                {
                    response.weights = matrix.Item2;
                }
            }
            if(errors != null && errors.Length > 0)
            {
                response.errors = new Domain.Error[errors.Length];
                for(var i = 0; i < errors.Length; i++)
                {
                    response.errors[i] = new Domain.Error()
                    {
                        type = errors[i].Item1,
                        index = errors[i].Item2,
                        message = errors[i].Item3
                    };
                }
            }
            return response;
        }
    }
}