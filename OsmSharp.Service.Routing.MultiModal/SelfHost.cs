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
        /// <param name="transitRouter"></param>
        public static void Start(Uri uri, MultiModalRouter transitRouter)
        {
            // initialize all APIs, a multi modal router should be able to support all of them.
            OsmSharp.Service.Routing.Bootstrapper.Initialize(transitRouter);
            OsmSharp.Service.Routing.Transit.Bootstrapper.Initialize(new Wrappers.TransitServiceWrapper(transitRouter));
            OsmSharp.Service.Routing.MultiModal.Bootstrapper.Initialize(new Wrappers.MultiModalWrapper(transitRouter));

            // start host.
            using (var host = new NancyHost(uri))
            {
                host.Start();
                Console.WriteLine("Service started.");
                Console.ReadLine();
            }
        }
    }
}