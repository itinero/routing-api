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

using Itinero.API.Models;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.API.Instances
{
    /// <summary>
    /// Holds and manages routing instances.
    /// </summary>
    public static class InstanceManager
    {
        /// <summary>
        /// Holds the routing service instances.
        /// </summary>
        private static readonly Dictionary<string, IInstance> _items =
            new Dictionary<string, IInstance>();

        /// <summary>
        /// Gets meta-data.
        /// </summary>
        /// <returns></returns>
        public static Meta GetMeta()
        {
            return new Meta()
            {
                Instances = _items.Keys.ToArray()
            };
        }

        /// <summary>
        /// Returns true if the given instance is active.
        /// </summary>
        public static bool IsActive(string name)
        {
            return _items.ContainsKey(name);
        }

        /// <summary>
        /// Returns true if there is at least one instance.
        /// </summary>
        public static bool HasInstances => _items.Count > 0;

        /// <summary>
        /// Returns the routing module instance with the given name.
        /// </summary>
        public static bool TryGet(string name, out IInstance instance)
        {
            return _items.TryGetValue(name, out instance);
        }

        /// <summary>
        /// Registers a new instance.
        /// </summary>
        public static void Register(string name, IInstance instance)
        {
            _items[name] = instance;
        }
    }
}