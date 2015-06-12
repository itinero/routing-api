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

using OsmSharp.Geo.Features;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Vehicles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Service.Routing.Tests
{
    /// <summary>
    /// A mockup of the API implementation.
    /// </summary>
    class ApiMock : ApiBase
    {

        public override Route GetRoute(Vehicle vehicle, GeoCoordinate[] coordinates, bool complete, bool sort)
        {
            throw new System.NotImplementedException();
        }

        public override Route[] GetOneToMany(Vehicle vehicle, GeoCoordinate[] coordinates, bool complete)
        {
            throw new System.NotImplementedException();
        }

        public override List<Instruction> GetInstructions(Route route)
        {
            throw new System.NotImplementedException();
        }

        public override FeatureCollection GetFeatures(Route route)
        {
            throw new System.NotImplementedException();
        }

        public override FeatureCollection GetFeaturesWithInstructions(Route route)
        {
            throw new System.NotImplementedException();
        }

        public override FeatureCollection GetNeworkFeatures(GeoCoordinateBox box)
        {
            throw new System.NotImplementedException();
        }

        public override bool SupportsVehicle(Vehicle vehicle)
        {
            throw new System.NotImplementedException();
        }

        public override Tuple<string, double[][]>[] GetMatrix(Vehicle vehicle, GeoCoordinate[] source, GeoCoordinate[] target, string[] outputs,
            out Tuple<string, int, string>[] errors)
        {
            var errorsList = new List<Tuple<string, int, string>>();
            for(var i = 0; i < source.Length;i++)
            {
                if(source[i] == null || source[i].Latitude > 180)
                { // dummy incorrect coordinates had a lat bigger than 180.
                    errorsList.Add(new Tuple<string, int, string>("source", i, "Coordinate invalid."));
                    source[i] = null;
                }
            }
            for (var i = 0; i < target.Length; i++)
            {
                if (target[i] == null || target[i].Latitude > 180)
                { // dummy incorrect coordinates had a lat bigger than 180.
                    errorsList.Add(new Tuple<string, int, string>("target", i, "Coordinate invalid."));
                    target[i] = null;
                }
            }
            errors = errorsList.ToArray();

            // remove invalid coordinates.
            var sourceList = new List<GeoCoordinate>(source);
            sourceList.RemoveAll(x => x == null);
            source = sourceList.ToArray();
            var targetList = new List<GeoCoordinate>(target);
            targetList.RemoveAll(x => x == null);
            target = targetList.ToArray();

            // build a dummy response.
            var matrices = new Tuple<string, double[][]>[outputs.Length];
            for(var i = 0; i < outputs.Length; i++)
            {
                var weights = new double[source.Length][];
                for(var x = 0; x < weights.Length; x++)
                {
                    weights[x] = new double[target.Length];
                    for(var y = 0; y < target.Length; y++)
                    {
                        weights[x][y] = 100;
                    }
                }
                matrices[i] = new Tuple<string, double[][]>(outputs[i],
                    weights);
            }
            return matrices;
        }

        public override Route[] GetTransitOneToMany(DateTime dt, List<Vehicle> vehicles, GeoCoordinate[] coordinates, HashSet<string> operators, bool complete)
        {
            throw new NotImplementedException();
        }
    }
}