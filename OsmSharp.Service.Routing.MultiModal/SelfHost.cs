using Nancy.Hosting.Self;
using OsmSharp.Routing.Transit.MultiModal;
using System;
using System.Collections.Generic;

namespace OsmSharp.Service.Routing.MultiModal
{
    /// <summary>
    /// Defines a few self-hosting facility methods.
    /// </summary>
    public class SelfHost
    {
        /// <summary>
        /// Starts a self-hosted instance of the transit API.
        /// </summary>
        /// <param name="uri"></param>
        public static void Start(Uri uri)
        {
            // start host.
            using (var host = new NancyHost(uri))
            {
                host.Start();
                Console.WriteLine("Service started.");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Starts a self-hosted instance of the transit API.
        /// </summary>
        /// <param name="instance">The instance name.</param>
        /// <param name="transitRouter"></param>
        public static void Add(string instance, MultiModalRouter transitRouter)
        {
            // initialize all APIs, a multi modal router should be able to support all of them.
            OsmSharp.Service.Routing.Bootstrapper.Add(instance, transitRouter);
            OsmSharp.Service.Routing.Transit.Bootstrapper.Add(instance, new Wrappers.TransitServiceWrapper(transitRouter));
            OsmSharp.Service.Routing.MultiModal.Bootstrapper.Add(instance, new Wrappers.MultiModalWrapper(transitRouter));
        }
    }
}