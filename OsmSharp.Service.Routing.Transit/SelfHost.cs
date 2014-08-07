// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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

using Nancy.Hosting.Self;
using OsmSharp.Routing.Transit;
using OsmSharp.Routing.Transit.GTFS;
using System;

namespace OsmSharp.Service.Routing.Transit
{
    /// <summary>
    /// Defines a few self hosting facility methods.
    /// </summary>
    public static class SelfHost
    {
        /// <summary>
        /// Starts a self-hosted instance of the transit API.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="instance">The instance name.</param>
        /// <param name="transitServiceWrapper"></param>
        public static void Start(Uri uri, string instance, TransitServiceWrapperBase transitServiceWrapper)
        {
            // initialize API.
            Bootstrapper.Add(instance, transitServiceWrapper);

            // start host.
            using (var host = new NancyHost(uri))
            {
                host.Start();
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Starts a self-hosted instance of the transit API.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="transitRouter"></param>
        public static void Start(Uri uri, string instance, TransitRouter transitRouter)
        {
            // initialize API.
            Bootstrapper.Add(instance, transitRouter);

            // start host.
            using (var host = new NancyHost(uri))
            {
                host.Start();
                Console.ReadLine();
            }
        }
        
        /// <summary>
        /// Starts a self-hosted instance of the transit APi.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="feed"></param>
        public static void Start(Uri uri, string instance, GTFS.GTFSFeed feed)
        {
            // create the transit router.
            var transitRouter = GTFSGraphReader.CreateRouter(feed);

            SelfHost.Start(uri, instance, transitRouter);
        }
    }
}
