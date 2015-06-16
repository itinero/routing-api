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

namespace OsmSharp.Service.Routing.Matrix.Domain
{
    /// <summary>
    /// Defines a request
    /// </summary>
    public class Request
    {
        /// <summary>
        /// The default name of the times-option.
        /// </summary>
        public const string TimesOutputOption = "times";
        /// <summary>
        /// The default name of the distance-option.
        /// </summary>
        public const string DistanceOutputOption = "distances";
        /// <summary>
        /// The default name of weight-option.
        /// </summary>
        public const string WeightsOutputOption = "weights";

        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        public double[][] locations { get; set; }

        /// <summary>
        /// Gets or sets the sources.
        /// </summary>
        public double[][] sources { get; set; }

        /// <summary>
        /// Gets or sets the targets.
        /// </summary>
        public double[][] targets { get; set; }
        
        /// <summary>
        /// Gets or sets the profile.
        /// </summary>
        public Profile profile { get; set; }

        /// <summary>
        /// Gets or sets the output options.
        /// </summary>
        public string[] output { get; set; }
    }
}