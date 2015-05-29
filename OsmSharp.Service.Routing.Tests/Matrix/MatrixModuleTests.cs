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

namespace OsmSharp.Service.Routing.Tests.Matrix
{
    /// <summary>
    /// Contains tests for the matrix module.
    /// </summary>
    [TestFixture]
    class MatrixModuleTests
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
            ApiBootstrapper.Add("mock", new ApiMock());

            // when empty request
            var result = browser.Put("mock/matrix/", with =>
            {
                with.Body(string.Empty);
                with.Header("content-type", "application/json");
                with.HttpRequest();
            });

            // then not acceptable
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);

            // only sources but no targets.
            var request = new OsmSharp.Service.Routing.Matrix.Domain.Request()
            {
                sources = new double[2][]
            };
            result = browser.Put("mock/matrix/", with =>
            {
                with.JsonBody(request);
                with.HttpRequest();
            });

            // not acceptable
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);

            // invalid output options
            request = new OsmSharp.Service.Routing.Matrix.Domain.Request()
            {
                locations = new double[2][],
                output = new string[] 
                { 
                    "someinvalidweight"
                }
            };
            result = browser.Put("mock/matrix/", with =>
            {
                with.JsonBody(request);
                with.HttpRequest();
            });

            // not acceptable
            Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);
        }

        /// <summary>
        /// Tests NxN calculation requests/responses.
        /// </summary>
        [Test]
        public void TestNxN()
        {
            // given
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/json"));
            ApiBootstrapper.Add("mock", new ApiMock());

            // form a valid request.
            var locations = new double[4][];
            for(var i = 0; i < 4; i++)
            {
                locations[i] = new double[4];
            }
            var request = new OsmSharp.Service.Routing.Matrix.Domain.Request()
            {
                locations = locations,
                output = new string[] 
                { 
                    "times"
                }
            };
            var result = browser.Put("mock/matrix/", with =>
            {
                with.JsonBody(request);
                with.HttpRequest();
            });

            // not acceptable
            Assert.IsNotNull(result);
            var response = result.Body.DeserializeJson<OsmSharp.Service.Routing.Matrix.Domain.Response>();
            Assert.IsNotNull(response);
            Assert.IsNull(response.weights);
            Assert.IsNull(response.distances);
            Assert.IsNotNull(response.times);
            foreach(var times in response.times)
            {
                foreach(var weight in times)
                {
                    Assert.AreEqual(100, weight);
                }
            }
            request = new OsmSharp.Service.Routing.Matrix.Domain.Request()
            {
                locations = locations,
                output = new string[] 
                { 
                    "distances"
                }
            };
            result = browser.Put("mock/matrix/", with =>
            {
                with.JsonBody(request);
                with.HttpRequest();
            });

            // check result.
            Assert.IsNotNull(result);
            response = result.Body.DeserializeJson<OsmSharp.Service.Routing.Matrix.Domain.Response>();
            Assert.IsNotNull(response);
            Assert.IsNull(response.weights);
            Assert.IsNull(response.times);
            Assert.IsNotNull(response.distances);
            foreach (var distances in response.distances)
            {
                foreach (var weight in distances)
                {
                    Assert.AreEqual(100, weight);
                }
            }

            request = new OsmSharp.Service.Routing.Matrix.Domain.Request()
            {
                locations = locations,
                output = new string[] 
                { 
                    "weights"
                }
            };
            result = browser.Put("mock/matrix/", with =>
            {
                with.JsonBody(request);
                with.HttpRequest();
            });

            // check result.
            Assert.IsNotNull(result);
            response = result.Body.DeserializeJson<OsmSharp.Service.Routing.Matrix.Domain.Response>();
            Assert.IsNotNull(response);
            Assert.IsNull(response.distances);
            Assert.IsNull(response.times);
            Assert.IsNotNull(response.weights);
            foreach (var weights in response.weights)
            {
                foreach (var weight in weights)
                {
                    Assert.AreEqual(100, weight);
                }
            }
        }

        /// <summary>
        /// Tests error reporting.
        /// </summary>
        [Test]
        public void TestErrors()
        {
            // given
            var bootstrapper = new DefaultNancyBootstrapper();
            var browser = new Browser(bootstrapper, defaults: to => to.Accept("application/json"));
            ApiBootstrapper.Add("mock", new ApiMock());

            // form an invalid location array.
            var locations = new double[4][];
            for (var i = 0; i < 4; i++)
            {
                locations[i] = new double[2];
            }
            locations[2][1] = 182;
            var request = new OsmSharp.Service.Routing.Matrix.Domain.Request()
            {
                locations = locations,
                output = new string[] 
                { 
                    "times"
                }
            };
            var result = browser.Put("mock/matrix/", with =>
            {
                with.JsonBody(request);
                with.HttpRequest();
            });

            // check result.
            Assert.IsNotNull(result);
            var response = result.Body.DeserializeJson<OsmSharp.Service.Routing.Matrix.Domain.Response>();
            Assert.IsNotNull(response);
            Assert.IsNull(response.weights);
            Assert.IsNull(response.distances);
            Assert.IsNotNull(response.times);
            Assert.AreEqual(3, response.times.Length);
            Assert.AreEqual(1, response.errors.Length);
            Assert.AreEqual(2, response.errors[0].index);

            // form an invalid sources and targets array.
            var sources = new double[4][];
            for (var i = 0; i < 4; i++)
            {
                sources[i] = new double[2];
            }
            sources[1][1] = 182;
            var targets = new double[4][];
            for (var i = 0; i < 4; i++)
            {
                targets[i] = new double[2];
            }
            targets[0][1] = 182;
            request = new OsmSharp.Service.Routing.Matrix.Domain.Request()
            {
                sources = sources,
                targets = targets,
                output = new string[] 
                { 
                    "times"
                }
            };
            result = browser.Put("mock/matrix/", with =>
            {
                with.JsonBody(request);
                with.HttpRequest();
            });

            // check result.
            Assert.IsNotNull(result);
            response = result.Body.DeserializeJson<OsmSharp.Service.Routing.Matrix.Domain.Response>();
            Assert.IsNotNull(response);
            Assert.IsNull(response.weights);
            Assert.IsNull(response.distances);
            Assert.IsNotNull(response.times);
            Assert.AreEqual(3, response.times.Length);
            Assert.AreEqual(2, response.errors.Length);
            Assert.AreEqual(1, response.errors[0].index);
            Assert.AreEqual("source", response.errors[0].type);
            Assert.AreEqual(0, response.errors[1].index);
            Assert.AreEqual("target", response.errors[1].type);
        }
    }
}