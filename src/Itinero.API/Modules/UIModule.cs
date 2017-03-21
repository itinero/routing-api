using Itinero.API.Instances;
using Nancy;
using System;

namespace Itinero.API.Modules
{
    /// <summary>
    /// A module with a testing UI.
    /// </summary>
    public class UIModule : NancyModule
    {
        public UIModule()
        {
            Get("/", _ =>
            {
                var model = InstanceManager.GetMeta();
                return View["instances", model];
            });
            Get("{instance}", _ =>
            {
                // get instance and check if active.
                string instanceName = _.instance;
                IInstance instance;
                if (!InstanceManager.TryGet(instanceName, out instance))
                { // oeps, instance not active!
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                var center = instance.RouterDb.EstimateCenter();

                var siteBase = this.Request.Url.SiteBase;

                dynamic model = new {
                    Name = instanceName,
                    SiteBase = siteBase,
                    CenterLat = center.Latitude.ToInvariantString(),
                    CenterLon = center.Longitude.ToInvariantString()
                };

                return View["instance", model];
            });
        }
    }
}