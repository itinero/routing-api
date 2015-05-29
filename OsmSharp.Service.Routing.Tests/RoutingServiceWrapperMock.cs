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

using NetTopologySuite.Features;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using System;
using System.Collections.Generic;

namespace OsmSharp.Service.Routing.Tests
{
    /// <summary>
    /// A mockup of the routing service wrapper.
    /// </summary>
    class RoutingServiceWrapperMock : ApiBase
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

        public override Tuple<string, double[][]>[] GetMatrix(Vehicle vehicle, GeoCoordinate[] source, GeoCoordinate[] target, string[] outputs)
        {
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
    }
}