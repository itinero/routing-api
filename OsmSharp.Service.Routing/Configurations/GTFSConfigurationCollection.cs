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
    /// Represents a collection of GTFS configurations.
    /// </summary>
    public class GTFSConfigurationCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// The default accessor.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GTFSConfiguration this[int index]
        {
            get { return BaseGet(index) as GTFSConfiguration; }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Creates a new configuration element.
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new GTFSConfiguration();
        }

        /// <summary>
        /// Gets a key for the configuration element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((GTFSConfiguration)element).Name;
        }
    }
}
