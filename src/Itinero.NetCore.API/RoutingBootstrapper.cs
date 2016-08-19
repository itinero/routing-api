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

using System.Collections.Generic;

namespace Itinero.API
{
    /// <summary>
    /// The bootstrapper for the routing module.
    /// </summary>
    public class RoutingBootstrapper
    {
        /// <summary>
        /// Holds the routing service instances.
        /// </summary>
        private static Dictionary<string, IRoutingModuleInstance> _instances =
            new Dictionary<string, IRoutingModuleInstance>();

        /// <summary>
        /// Returns true if the given instance is active.
        /// </summary>
        public static bool IsActive(string name)
        {
            return _instances.ContainsKey(name);
        }
        
        /// <summary>
        /// Returns the routing module instance with the given name.
        /// </summary>
        public static IRoutingModuleInstance Get(string name)
        {
            return _instances[name];
        }

        /// <summary>
        /// Registers a new instance.
        /// </summary>
        public static void Register(string name, IRoutingModuleInstance instance)
        {
            _instances[name] = instance;
        }

        /// <summary>
        /// Get names of all registered instances
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetNamesRegistered()
        {
            return _instances.Keys;
        }
    }
}