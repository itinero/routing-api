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

using NUnit.Framework;
using OsmSharp.Service.Routing.Configurations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Service.Routing.Tests.Configurations
{
    /// <summary>
    /// Containts tests for the custom configuration files.
    /// </summary>
    [TestFixture]
    class ConfigurationsTests
    {
        /// <summary>
        /// Tests loading the app.config file with several configurations.
        /// </summary>
        [Test]
        public void TestAppConfig()
        {
            var apiConfiguration = (ApiConfiguration)ConfigurationManager.GetSection("ApiConfiguration");
            Assert.IsNotNull(apiConfiguration);
            Assert.AreEqual(4, apiConfiguration.Instances.Count);

            foreach (InstanceConfiguration instance in apiConfiguration.Instances)
            {
                if(instance.Name == "dummy1")
                {
                    Assert.AreEqual("notanactualfile.osm.pbf", instance.Graph);
                    Assert.AreEqual("raw", instance.Type);
                    Assert.AreEqual("osm-pbf", instance.Format);
                }
                else if (instance.Name == "dummy2")
                {
                    Assert.AreEqual("notanactualfile.osm.pbf", instance.Graph);
                    Assert.AreEqual("raw", instance.Type);
                    Assert.AreEqual("osm-xml", instance.Format);
                }
                else if (instance.Name == "dummy3")
                {
                    Assert.AreEqual("notanactualfile.osm.pbf", instance.Graph);
                    Assert.AreEqual("raw", instance.Type);
                    Assert.AreEqual("osm-xml", instance.Format);
                    Assert.AreEqual("car", instance.Vehicle);
                }
                else if (instance.Name == "dummy4")
                {
                    Assert.AreEqual("notanactualfile.osm.pbf", instance.Graph);
                    Assert.AreEqual("flat", instance.Type);
                    Assert.AreEqual("osm-xml", instance.Format);

                    var feeds = instance.Feeds;
                    Assert.IsNotNull(feeds);
                    foreach (GTFSConfiguration feed in feeds)
                    {
                        if (feed.Name == "feed1")
                        {
                            Assert.AreEqual(@"not\an\actual\path1\", feed.Path);
                        }
                        else if (feed.Name == "feed2")
                        {
                            Assert.AreEqual(@"not\an\actual\path2\", feed.Path);
                        }
                        else
                        {
                            Assert.Fail("Unexpected feed name: {0}.", instance.Name);
                        }
                    }
                }
                else
                {
                    Assert.Fail("Unexpected instance name: {0}.", instance.Name);
                }
            }
        }
    }
}