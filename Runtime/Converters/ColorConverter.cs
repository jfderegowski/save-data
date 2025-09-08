using System;
using fefek5.Variables.SaveDataVariable.Runtime.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Converters
{
    public class ColorConverter : JsonConverter<Color>
    {
        private const string R = "r";
        private const string G = "g";
        private const string B = "b";
        private const string A = "a";
        
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.TryWriteType<Color>(serializer);
            
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

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var color = new Color();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType != JsonToken.PropertyName || reader.Value == null)
                    continue;
                
                var propertyName = reader.Value.ToString();
                
                reader.Read();

                switch (propertyName)
                {
                    case R:
                        color.r = serializer.Deserialize<float>(reader);
                        break;
                    case G:
                        color.g = serializer.Deserialize<float>(reader);
                        break;
                    case B:
                        color.b = serializer.Deserialize<float>(reader);
                        break;
                    case A:
                        color.a = serializer.Deserialize<float>(reader);
                        break;
                    default:
                        Debug.LogWarning($"Unknown property name: {propertyName}");
                        break;
                }
            }

            return color;
        }
    }
}
