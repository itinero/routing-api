using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.API.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            // start from configuration.
            Bootstrapper.BootFromConfiguration();

            // start listening.
            var uri = new Uri("http://localhost:1234");
            using (var host = new NancyHost(uri))
            {
                host.Start();

                Console.WriteLine("The OsmSharp routing service is running at " + uri);
                Console.WriteLine("Press [Enter] to close.");
                Console.ReadLine();
            }
        }
    }
}
