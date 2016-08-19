using System;
using Newtonsoft.Json;

namespace Itinero.API
{
    /// The default json converter complains about circular references
    public class RouteJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Route);
        }
        public override bool CanRead => false;

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var route = (Route)value;
            writer.WriteValue(route.ToJson());
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // the default deserialization works fine, 
            // but otherwise we'd handle that here
            throw new NotImplementedException();
        }
    }
}
