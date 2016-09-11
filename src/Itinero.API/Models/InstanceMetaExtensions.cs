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

using System.IO;

namespace Itinero.API.Models
{
    /// <summary>
    /// Contains extension methods for the routerdb meta.
    /// </summary>
    public static class InstanceMetaExtensions
    {
        /// <summary>
        /// Converts the router db meta to json.
        /// </summary>
        public static void ToJson(this InstanceMeta routerDbMeta, TextWriter writer)
        {
            var jsonWriter = new IO.Json.JsonWriter(writer);
            jsonWriter.WriteOpen();

            jsonWriter.WritePropertyName("id");
            jsonWriter.WritePropertyValue(routerDbMeta.Id);

            if (routerDbMeta.Meta != null)
            {
                jsonWriter.WriteOpen();

                foreach(var attribute in routerDbMeta.Meta)
                {
                    jsonWriter.WritePropertyName(attribute.Key);
                    jsonWriter.WritePropertyValue(attribute.Value);
                }

                jsonWriter.WriteClose();
            }

            if (routerDbMeta.Profiles != null)
            {
                jsonWriter.WritePropertyName("Profiles");
                jsonWriter.WriteArrayOpen();
                for (var i = 0; i < routerDbMeta.Profiles.Length; i++)
                {
                    var profile = routerDbMeta.Profiles[i];

                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteOpen();

                    jsonWriter.WritePropertyName("name");
                    jsonWriter.WritePropertyValue(profile.Name);
                    
                    jsonWriter.WritePropertyName("metric");
                    jsonWriter.WritePropertyValue(profile.Metric.ToInvariantString());

                    jsonWriter.WriteClose();
                    jsonWriter.WriteArrayClose();
                }
                jsonWriter.WriteArrayClose();
            }

            if (routerDbMeta.Contracted != null)
            {
                jsonWriter.WritePropertyName("Profiles");
                jsonWriter.WriteArrayOpen();
                for (var i = 0; i < routerDbMeta.Contracted.Length; i++)
                {
                    var profile = routerDbMeta.Contracted[i];

                    jsonWriter.WriteArrayOpen();

                    jsonWriter.WriteArrayValue(profile);

                    jsonWriter.WriteArrayClose();
                }
                jsonWriter.WriteArrayClose();
            }
        }
    }
}