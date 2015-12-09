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

using System.Configuration;

namespace OsmSharp.Routing.API.Configurations
{
    /// <summary>
    /// Represents a configuration for one API-instance.
    /// </summary>
    public class ApiConfiguration : ConfigurationSection
    {        
        /// <summary>
        /// Returns the collection of instance configurations.
        /// </summary>
        [ConfigurationProperty("instances", IsRequired = false)]
        public InstanceConfigurationCollection Instances
        {
            get { return this["instances"] as InstanceConfigurationCollection; }
        }
    }
}