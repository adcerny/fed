using Fed.Core.ValueTypes;
using Newtonsoft.Json;
using System;

namespace Fed.Core.Converters
{
    public class DateJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return type == typeof(Date);
        }

        public override bool CanRead => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;
            var dt = reader.Value.ToString();
            return Date.Parse(dt);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var date = (Date)value;
            writer.WriteValue(date.ToString());
        }
    }
}