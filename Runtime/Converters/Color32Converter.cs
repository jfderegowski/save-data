using System;
using fefek5.Variables.SaveDataVariable.Runtime.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Converters
{
    public class Color32Converter : JsonConverter<Color32>
    {
        private const string R = "r";
        private const string G = "g";
        private const string B = "b";
        private const string A = "a";
        
        public override void WriteJson(JsonWriter writer, Color32 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.TryWriteType<Color32>(serializer);
            
            writer.WritePropertyName(R);
            writer.WriteValue(value.r);
            
            writer.WritePropertyName(G);
            writer.WriteValue(value.g);
            
            writer.WritePropertyName(B);
            writer.WriteValue(value.b);
            
            writer.WritePropertyName(A);
            writer.WriteValue(value.a);
            
            writer.WriteEndObject();
        }

        public override Color32 ReadJson(JsonReader reader, Type objectType, Color32 existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var color = new Color32();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader is not { TokenType: JsonToken.PropertyName, Value: not null }) continue;
                
                var propertyName = reader.Value.ToString();
                
                reader.Read();

                switch (propertyName)
                {
                    case R:
                        color.r = Convert.ToByte(reader.Value);
                        break;
                    case G:
                        color.g = Convert.ToByte(reader.Value);
                        break;
                    case B:
                        color.b = Convert.ToByte(reader.Value);
                        break;
                    case A:
                        color.a = Convert.ToByte(reader.Value);
                        break;
                }
            }

            return color;
        }
    }
}
