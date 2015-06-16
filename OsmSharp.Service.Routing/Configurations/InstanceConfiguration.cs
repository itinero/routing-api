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

namespace OsmSharp.Service.Routing.Configurations
{
    /// <summary>
    /// Represents a configuration for one routing instance inside an existing API.
    /// </summary>
    public class InstanceConfiguration : ConfigurationElement
    {
        /// <summary>
        /// Returns the graph, the file to load the routing graph from.
        /// </summary>
        /// <remarks>Take the graph from the API-configuration if not set here.</remarks>
        [ConfigurationProperty("graph", IsRequired = false)]
        public string Graph
        {
            get { return this["graph"] as string; }
        }

        /// <summary>
        /// Returns the graph type.
        /// </summary>
        /// <remarks>Allowed values:
        ///          - raw: Raw osm-data.
        ///          - simple: Serialized routing graph.
        ///          - contracted: Contracted serialized data.
        ///              -> a vehicle type is required in this case!
        /// </remarks>
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
        }

        /// <summary>
        /// Returns the graph format.
        /// </summary>
        /// <remarks>Allowed values:
        ///          - osm-xml: Raw osm-data in the form of OSM-XML.
        ///          - osm-pbf: Raw osm-data in the form of OSM-PBF.
        ///          - flat: Serialized routing graph.
        /// </remarks>
        [ConfigurationProperty("format", IsRequired = true)]
        public string Format
        {
            get { return this["format"] as string; }
        }

        /// <summary>
        /// Returns the vehicle type.
        /// </summary>
        /// <remarks>Allowed values: Any vehicle type currently registered.
        /// </remarks>
        [ConfigurationProperty("vehicle", IsRequired = false)]
        public string Vehicle
        {
            get { return this["vehicle"] as string; }
        }

        /// <summary>
        /// Returns the name of this instance.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
        }

        /// <summary>
        /// Returns the monitor flag.
        /// </summary>
        [ConfigurationProperty("monitor")]
        public bool Monitor
        {
            get
            {
                var monitorValue = this["monitor"];
                if (monitorValue != null &&
                    monitorValue is bool)
                {
                    return ((bool)monitorValue);
                }
                return false;
            }
        }

        /// <summary>
        /// Returns the collection of GTFS configurations.
        /// </summary>
        [ConfigurationProperty("feeds", IsRequired = false)]
        public GTFSConfigurationCollection Feeds
        {
            get { return this["feeds"] as GTFSConfigurationCollection; }
        }
    }
}