// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

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

using Itinero.API.Instances;
using Itinero.VectorTiles;
using Itinero.VectorTiles.Mapbox;
using Itinero.VectorTiles.GeoJson;
using Nancy;
using System.IO;
using Itinero.Attributes;
using Itinero.VectorTiles.Layers;
using Itinero.API.VectorTiles.Mapbox;
using System;

namespace Itinero.API.Modules
{
    /// <summary>
    /// A module responsible for serving vector tiles.
    /// </summary>
    public class VectorTileModule : NancyModule
    {
        public VectorTileModule()
        {
            //Get("{instance}/tiles/{z}/{x}/{y}.geojson", _ =>
            //{
            //    return this.DoGetGeoJson(_);
            //});
            Get("{instance}/tiles/mvt.json", _ =>
            {
                this.EnableCors();

                string uri = this.Request.Url;
                var length = "/tiles/mvt.json".Length;
                uri = uri.Substring(0, uri.Length - length);
                var json = DefinitionBuilder.Build(uri, Constants.DefaultAttribution);

                return Response.AsText(json, "application/json");
            });
            Get("{instance}/tiles/{z}/{x}/{y}.mvt", _ =>
            {
                return this.DoGetMapboxVectorTile(_);
            });
        }

        //private object DoGetGeoJson(dynamic _)
        //{
        //    this.EnableCors();

        //    // get instance and check if active.
        //    string instanceName = _.instance;
        //    IInstance instance;
        //    if (!InstanceManager.TryGet(instanceName, out instance))
        //    { // oeps, instance not active!
        //        return null;
        //    }
            
        //    // x,y,z.
        //    VectorTiles.Tiles.Tile tile = null;
        //    VectorTile vectorTile = null;
        //    int x = -1, y = -1;
        //    ushort z = 0;
        //    if (ushort.TryParse(_.z, out z) &&
        //        int.TryParse(_.x, out x) &&
        //        int.TryParse(_.y, out y))
        //    { // ok, valid stuff!
        //        tile = new VectorTiles.Tiles.Tile(x, y, z);
        //        vectorTile = instance.GetVectorTile(tile.Id).Value;
        //    }
        //    else
        //    {
        //        return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
        //    }

        //    var stream = new MemoryStream();
        //    var streamWriter = new StreamWriter(stream);
        //    segments.Value.WriteGeoJson(instance.RouterDb, streamWriter);
        //    streamWriter.Flush();
        //    stream.Seek(0, SeekOrigin.Begin);
        //    return Response.FromStream(stream, "application/json");
        //}
        
        private object DoGetMapboxVectorTile(dynamic _)
        {
            this.EnableCors();

            // get instance and check if active.
            string instanceName = _.instance;
            IInstance instance;
            if (!InstanceManager.TryGet(instanceName, out instance))
            { // oeps, instance not active!
                return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
            }

            try
            {
                // x,y,z.
                Itinero.VectorTiles.Tiles.Tile tile = null;
                Result<VectorTile> vectorTile = null;
                int x = -1, y = -1;
                ushort z = 0;
                if (ushort.TryParse(_.z, out z) &&
                    int.TryParse(_.x, out x) &&
                    int.TryParse(_.y, out y))
                { // ok, valid stuff!
                    tile = new Itinero.VectorTiles.Tiles.Tile(x, y, z);
                    vectorTile = instance.GetVectorTile(tile.Id);
                }
                else
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);
                }

                var stream = new MemoryStream();
                lock (instance.RouterDb)
                {
                    vectorTile.Value.Write(stream, (a, l) =>
                    {
                        if (l.Name != "transportation")
                        {
                            return a;
                        }

                        var result = new AttributeCollection();
                        string highway;
                        if (a.TryGetValue("highway", out highway))
                        {
                            var className = string.Empty;
                            switch (highway)
                            {
                                case "motorway":
                                case "motorway_link":
                                    className = "motorway";
                                    break;
                                case "trunk":
                                case "trunk_link":
                                    className = "trunk";
                                    break;
                                case "primary":
                                case "primary_link":
                                    className = "primary";
                                    break;
                                case "secondary":
                                case "secondary_link":
                                    className = "secondary";
                                    break;
                                case "tertiary":
                                case "tertiary_link":
                                    className = "tertiary";
                                    break;
                                case "unclassified":
                                case "residential":
                                case "living_street":
                                case "road":
                                    className = "minor";
                                    break;
                                case "service":
                                case "track":
                                    className = highway;
                                    break;
                                case "pedestrian":
                                case "path":
                                case "footway":
                                case "cycleway":
                                case "steps":
                                case "bridleway":
                                case "corridor":
                                    className = "path";
                                    break;
                            }
                            if (!string.IsNullOrEmpty(className))
                            {
                                result.AddOrReplace("class", className);
                            }
                        }

                        foreach (var tag in a)
                        {
                            if (tag.Key == "highway")
                            {
                                continue;
                            }

                            result.AddOrReplace(tag.Key, tag.Value);
                        }
                        return result;
                    });
                }
                stream.Seek(0, SeekOrigin.Begin);
                return Response.FromStream(stream, "application/x-protobuf");
            }
            catch(Exception ex)
            {
                Itinero.Logging.Logger.Log("VectorTileModule", Logging.TraceEventType.Error,
                    "Unhandled exception occured during vector tile generation: {0}", ex.ToString());
            }
            return Negotiate.WithStatusCode(HttpStatusCode.InternalServerError);
        }
    }
}
