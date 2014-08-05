using Nancy;
using Nancy.Json;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Service.Routing.MultiModal.Domain;
using OsmSharp.Service.Routing.MultiModal.Domain.Queries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OsmSharp.Service.Routing.MultiModal
{
    public class MultiModalModule : NancyModule
    {
        public MultiModalModule()
        {
            JsonSettings.MaxJsonLength = Int32.MaxValue;

            Get["/multimodal"] = _ =>
            {
                this.EnableCors();

                OsmSharp.Logging.Log.TraceEvent("MultiModalModul", Logging.TraceEventType.Information, "New multimodal request.");
                try
                {
                    // bind the query if any.
                    var query = this.Bind<RoutingQuery>();

                    // parse location.
                    if (string.IsNullOrWhiteSpace(query.loc))
                    { // no loc parameters.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("loc parameter not found or request invalid.");
                    }
                    var locs = query.loc.Split(',');
                    if (locs.Length < 2)
                    { // less than two loc parameters.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("only one loc parameter found or request invalid.");
                    }
                    var coordinates = new GeoCoordinate[locs.Length / 2];
                    for (int idx = 0; idx < coordinates.Length; idx++)
                    {
                        double lat, lon;
                        if (double.TryParse(locs[idx * 2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lat) &&
                            double.TryParse(locs[idx * 2 + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lon))
                        { // parsing was successful.
                            coordinates[idx] = new GeoCoordinate(lat, lon);
                        }
                        else
                        { // invalid formatting.
                            return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("location coordinates are invalid.");
                        }
                    }

                    // get vehicle.
                    string vehicleName = "car"; // assume car is the default.
                    if (!string.IsNullOrWhiteSpace(query.vehicle))
                    { // a vehicle was defined.
                        vehicleName = query.vehicle;
                    }
                    var vehicles = new List<Vehicle>();
                    var vehicleNames = vehicleName.Split('|');
                    for (int idx = 0; idx < vehicleNames.Length; idx++)
                    {
                        var vehicle = Vehicle.GetByUniqueName(vehicleNames[idx]);
                        if (vehicle == null)
                        { // vehicle not found or not registered.
                            return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(string.Format("vehicle with name '{0}' not found.", vehicleName));
                        }
                        vehicles.Add(vehicle);
                    }

                    bool instructions = false;
                    if (!string.IsNullOrWhiteSpace(query.instructions))
                    { // there is an instruction flag.
                        instructions = query.instructions == "true";
                    }

                    bool complete = false;
                    if (!string.IsNullOrWhiteSpace(query.complete))
                    { // there is a complete flag.
                        complete = query.complete == "true";
                    }

                    bool fullFormat = false;
                    if (!string.IsNullOrWhiteSpace(query.format))
                    { // there is a format field.
                        fullFormat = query.format == "osmsharp";
                    }

                    bool departure = false;
                    if (!string.IsNullOrWhiteSpace(query.departure))
                    { // there is a format field.
                        departure = query.departure == "true";
                    }

                    // check conflicting parameters.
                    if (!complete && instructions)
                    { // user wants an incomplete route but instructions, this is impossible. 
                        complete = true;
                    }

                    // parse time.
                    if (string.IsNullOrWhiteSpace(query.time))
                    { // there is a format field.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("No valid time parameter found.");
                    }
                    DateTime dt;
                    string pattern = "yyyyMMddHHmm";
                    if (!DateTime.TryParseExact(query.time, pattern, CultureInfo.InvariantCulture,
                                               DateTimeStyles.None,
                                               out dt))
                    { // could not parse date.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(
                            string.Format("No valid time parameter found, could not parse date: {0}. Expected to be in format yyyyMMddHHmm."));
                    }
                    OsmSharp.Logging.Log.TraceEvent("MultiModalModul", Logging.TraceEventType.Information, "Request accepted.");

                    // calculate route.
                    var route = Bootstrapper.MultiModalServiceInstance.GetRoute(dt, vehicles, coordinates, complete);
                    OsmSharp.Logging.Log.TraceEvent("MultiModalModul", Logging.TraceEventType.Information, "Request finished.");
                    if (route == null)
                    { // route could not be calculated.
                        return null;
                    }
                    if (route != null && instructions)
                    { // also calculate instructions.
                        var instruction = Bootstrapper.MultiModalServiceInstance.GetInstructions(vehicles, route);

                        if (fullFormat)
                        {
                            return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(new CompleteRoute()
                            {
                                Route = route,
                                Instructions = instruction
                            });
                        }
                        else
                        {
                            var featureCollection = Bootstrapper.MultiModalServiceInstance.GetFeatures(route);
                            var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();
                            var geoJson = geoJsonWriter.Write(featureCollection);

                            return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(new SimpleRoute()
                            {
                                Route = geoJson,
                                Instructions = instruction
                            });
                        }
                    }

                    if (fullFormat)
                    { // return a complete route but no instructions.
                        return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(route);
                    }
                    else
                    { // return a GeoJSON object.
                        var featureCollection = Bootstrapper.MultiModalServiceInstance.GetFeatures(route);

                        return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(featureCollection);
                    }
                }
                catch (Exception)
                { // an unhandled exception!
                    return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
                }
            };
            Options["/multimodal/range"] = _ =>
            {
                this.EnableCors();
                return Negotiate.WithStatusCode(HttpStatusCode.OK);
            };
            Get["/multimodal/range"] = _ =>
            {
                this.EnableCors();

                OsmSharp.Logging.Log.TraceEvent("MultiModalModul", Logging.TraceEventType.Information, "New multimodal range request.");
                try
                {

                    // bind the query if any.
                    var query = this.Bind<RangeQuery>();

                    // parse location.
                    if (string.IsNullOrWhiteSpace(query.loc))
                    { // no loc parameters.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("loc parameter not found or request invalid.");
                    }
                    var locs = query.loc.Split(',');
                    if (locs.Length < 2)
                    { // less than two loc parameters.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("only one loc parameter found or request invalid.");
                    }
                    var coordinates = new GeoCoordinate[locs.Length / 2];
                    for (int idx = 0; idx < coordinates.Length; idx++)
                    {
                        double lat, lon;
                        if (double.TryParse(locs[idx * 2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lat) &&
                            double.TryParse(locs[idx * 2 + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lon))
                        { // parsing was successful.
                            coordinates[idx] = new GeoCoordinate(lat, lon);
                        }
                        else
                        { // invalid formatting.
                            return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("location coordinates are invalid.");
                        }
                    }

                    // get vehicle.
                    string vehicleName = "car"; // assume car is the default.
                    if (!string.IsNullOrWhiteSpace(query.vehicle))
                    { // a vehicle was defined.
                        vehicleName = query.vehicle;
                    }
                    var vehicles = new List<Vehicle>();
                    var vehicleNames = vehicleName.Split('|');
                    for (int idx = 0; idx < vehicleNames.Length; idx++)
                    {
                        var vehicle = Vehicle.GetByUniqueName(vehicleNames[idx]);
                        if (vehicle == null)
                        { // vehicle not found or not registered.
                            return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(string.Format("vehicle with name '{0}' not found.", vehicleName));
                        }
                        vehicles.Add(vehicle);
                    }

                    // parse time.
                    if (string.IsNullOrWhiteSpace(query.time))
                    { // there is a format field.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("No valid time parameter found.");
                    }
                    DateTime dt;
                    string pattern = "yyyyMMddHHmm";
                    if (!DateTime.TryParseExact(query.time, pattern, CultureInfo.InvariantCulture,
                                               DateTimeStyles.None,
                                               out dt))
                    { // could not parse date.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(
                            string.Format("No valid time parameter found, could not parse date: {0}. Expected to be in format yyyyMMddHHmm."));
                    }
                    OsmSharp.Logging.Log.TraceEvent("MultiModalModul", Logging.TraceEventType.Information, "Request accepted.");

                    // calculate route.
                    int max;
                    if(!int.TryParse(query.max, out max))
                    {// could not parse date.
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel(
                            string.Format("No valid max time parameter found, could not parse: {0}.", query.max));
                    }
                    var range = Bootstrapper.MultiModalServiceInstance.GetWithinRange(dt, vehicles, coordinates[0], max);
                    OsmSharp.Logging.Log.TraceEvent("MultiModalModul", Logging.TraceEventType.Information, "Request finished.");

                    // output all vertex and times.
                    var vertexAndTimes = new List<VertexAndTime>(range.Count);
                    foreach (var rangeEntry in range)
                    {
                        double time = max - rangeEntry.Value;
                        if (time > 0)
                        {
                            time = (int)((time / max) * 100);
                            vertexAndTimes.Add(new VertexAndTime()
                                {
                                    lat = rangeEntry.Key.Latitude,
                                    lon = rangeEntry.Key.Longitude,
                                    value = time
                                });
                        }
                    }
                    return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(new OsmSharp.Service.Routing.MultiModal.Domain.Queries.Range()
                        {
                            max = 100,
                            data = vertexAndTimes.ToArray()
                        });
                }
                catch (Exception)
                { // an unhandled exception!
                    return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
                }
            };
            Get["/multimodal/status"] = _ =>
                {
                    if(Bootstrapper.IsInitialized())
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(new Status()
                        {
                            Available = true,
                            Info = "Initialized."
                        });
                    }
                    return Negotiate.WithStatusCode(HttpStatusCode.OK).WithModel(new Status()
                    {
                        Available = false,
                        Info = "Not initialized."
                    });
                };
            Get["/multimodal/network"] = _ =>
            {
                try
                {
                    this.EnableCors();

                    // bind the query if any.
                    var query = this.Bind<BoxQuery>();

                    double left, right, top, bottom;
                    if (string.IsNullOrWhiteSpace(query.left) ||
                        !double.TryParse(query.left, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out left))
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("box coordinates are invalid.");
                    }
                    if (string.IsNullOrWhiteSpace(query.right) ||
                        !double.TryParse(query.right, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out right))
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("box coordinates are invalid.");
                    }
                    if (string.IsNullOrWhiteSpace(query.top) ||
                        !double.TryParse(query.top, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out top))
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("box coordinates are invalid.");
                    }
                    if (string.IsNullOrWhiteSpace(query.bottom) ||
                        !double.TryParse(query.bottom, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out bottom))
                    {
                        return Negotiate.WithStatusCode(HttpStatusCode.NotAcceptable).WithModel("box coordinates are invalid.");
                    }

                    var features = Bootstrapper.MultiModalServiceInstance.GetNeworkFeatures(new GeoCoordinateBox(new GeoCoordinate(top, left), new GeoCoordinate(bottom, right)));
                    var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();
                    return geoJsonWriter.Write(features);
                }
                catch (Exception)
                { // an unhandled exception!
                    return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
                }
            };
        }
    }
}
