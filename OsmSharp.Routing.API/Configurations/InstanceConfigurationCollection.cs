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
    /// Represents a collection of instance configurations.
    /// </summary>
    public class InstanceConfigurationCollection : ConfigurationElementCollection  
    {
        /// <summary>
        /// Returns the configuration instance at the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public InstanceConfiguration this[int index]
        {
            get { return BaseGet(index) as InstanceConfiguration; }
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
        /// Creates a new element of the correct type.
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new InstanceConfiguration();
        }

        /// <summary>
        /// Returns the key of the given element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((InstanceConfiguration)element).Name;
        }  
    }
}