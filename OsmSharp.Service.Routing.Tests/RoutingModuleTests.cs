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
using Nancy.Testing;
using NUnit.Framework;

namespace OsmSharp.Service.Routing.Tests
{
    /// <summary>
    /// Contains tests for the routing module.
    /// </summary>
    [TestFixture]
    class RoutingModuleTests
    {
        /// <summary>
        /// Tests the validation of the request.
        /// </summary>
        [Test]
        public void TestRequestValidation()
        {
            // given
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/json"));
            ApiBootstrapper.AddOrUpdate("mock", new ApiMock());

            // when incorrect instance request
            var result = browser.Put("notmock/routing/", with =>
            {
                with.Body(string.Empty);
                with.Header("content-type", "application/json");
                with.HttpRequest();
            });

            // then not found
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            // when empty request
            result = browser.Get("mock/routing/", with =>
            {
                with.Body(string.Empty);
                with.Header("content-type", "application/json");
                with.HttpRequest();
            });

            // then not acceptable
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);

            // when request with incorrect locations.
            result = browser.Get("mock/routing", with =>
            {
                with.Body(string.Empty);
                with.Header("content-type", "application/json");
                with.Query("vehicle", "car");
                with.Query("loc", "1,1");
                with.HttpRequest();
            });

            // then not acceptable
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);

            // when request with incorrect locations.
            result = browser.Get("mock/routing", with =>
            {
                with.Body(string.Empty);
                with.Header("content-type", "application/json");
                with.Query("vehicle", "car");
                with.Query("loc", "1;1");
                with.HttpRequest();
            });

            // then not acceptable
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);

            // when request with incorrect locations.
            result = browser.Get("mock/routing", with =>
            {
                with.Body(string.Empty);
                with.Header("content-type", "application/json");
                with.Query("vehicle", "car");
                with.Query("loc", "a,1");
                with.Query("loc", "1,1");
                with.HttpRequest();
            });

            // then not acceptable
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);

            // when request with incorrect vehicle.
            result = browser.Get("mock/routing", with =>
            {
                with.Body(string.Empty);
                with.Header("content-type", "application/json");
                with.Query("vehicle", "novehiclehere");
                with.Query("loc", "1,1");
                with.Query("loc", "1,1");
                with.HttpRequest();
            });

            // then not acceptable
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);

            // when request with 0 locations.
            var request = new Domain.Request()
            {
                locations = new double[][] {
                    new double[] {1, 1}
                },
                profile = new Domain.Profile()
                {
                    vehicle = "car"
                }
            };
            result = browser.Get("mock/routing", with =>
            {
                with.JsonBody(request);
                with.HttpRequest();
            });

            // then not acceptable
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);

            // when request with invalid vehicle.
            request = new Domain.Request()
            {
                locations = new double[][] {
                    new double[] {1, 1},
                    new double[] {2, 2}
                },
                profile = new Domain.Profile()
                {
                    vehicle = "novehiclehere"
                }
            };
            result = browser.Get("mock/routing", with =>
            {
                with.JsonBody(request);
                with.HttpRequest();
            });

            // then not acceptable
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);

            // when request with unsupported vehicle.
            request = new Domain.Request()
            {
                locations = new double[][] {
                    new double[] {1, 1},
                    new double[] {2, 2}
                },
                profile = new Domain.Profile()
                {
                    vehicle = "pedestrian"
                }
            };
            result = browser.Get("mock/routing", with =>
            {
                with.JsonBody(request);
                with.HttpRequest();
            });

            // then not acceptable
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);
        }

        /// <summary>
        /// Tests a simple one-to-one.
        /// </summary>
        [Test]
        public void Test1OneToOne()
        {
            // given
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/json"));
            ApiBootstrapper.AddOrUpdate("mock", new ApiMock());

            // request simple route.
            var result = browser.Get("mock/routing", with =>
            {
                with.Body(string.Empty);
                with.Header("content-type", "application/json");
                with.Query("vehicle", "car");
                with.Query("loc", "1,1");
                with.Query("loc", "3,4");
                with.HttpRequest();
            });

            // then not acceptable
            Assert.IsNotNull(result);
            var response = OsmSharp.Geo.Streams.GeoJson.GeoJsonConverter.ToFeatureCollection(result.Body.AsString());
            Assert.IsNotNull(response);
            Assert.AreEqual(1, response.Count);

            // request simple route but return full format.
            result = browser.Get("mock/routing", with =>
            {
                with.Body(string.Empty);
                with.Header("content-type", "application/json");
                with.Query("vehicle", "car");
                with.Query("loc", "1,1");
                with.Query("loc", "3,4");
                with.Query("format", "osmsharp");
                with.HttpRequest();
            });

            // then not acceptable
            Assert.IsNotNull(result);
            var route = result.Body.DeserializeJson<OsmSharp.Routing.Route>();
            Assert.IsNotNull(response);
            Assert.AreEqual(1, response.Count);
        }
    }
}