// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Nancy;
using Itinero.API.Instances;
using System;

namespace Itinero.API.Modules
{
    /// <summary>
    /// A module responsible for service meta-data.
    /// </summary>
    public class MetaModule : NancyModule
    {
        /// <summary>
        /// Creates a new meta model.
        /// </summary>
        public MetaModule()
        {
            Get("meta", _ =>
            {
                return this.DoGetMeta(_);
            });
            Get("{instance}/meta", _ =>
            {
                return this.DoGetInstanceMeta(_);
            });
        }

        /// <summary>
        /// Executes the get meta call.
        /// </summary>
        private object DoGetMeta(dynamic _)
        {
            return InstanceManager.GetMeta();
        }

        /// <summary>
        /// Executes the get meta call for an instance.
        /// </summary>
        private object DoGetInstanceMeta(dynamic _)
        {
            // get instance and check if active.
            string instanceName = _.instance;
            IInstance instance;
            if (!InstanceManager.TryGet(instanceName, out instance))
            { // oeps, instance not active!
                return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
            }

            return Negotiate.WithContentType("application/json").WithModel(instance.GetMeta());
        }
    }
}