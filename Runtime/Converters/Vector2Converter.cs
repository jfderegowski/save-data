using System;
using fefek5.Variables.SaveDataVariable.Runtime.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Converters
{
    public class Vector2Converter : JsonConverter<Vector2>
    {
        private const string X = "x";
        private const string Y = "y";
        
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            writer.TryWriteType<Vector2>(serializer);
            
            writer.WritePropertyName(X);
            writer.WriteValue(value.x);
            
            writer.WritePropertyName(Y);
            writer.WriteValue(value.y);
            
            writer.WriteEndObject();
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var vector2 = new Vector2();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader is not { TokenType: JsonToken.PropertyName, Value: not null }) continue;
                
                var propertyName = reader.Value.ToString();
                
                reader.Read();

                switch (propertyName)
                {
                    case X:
                        vector2.x = Convert.ToSingle(reader.Value);
                        break;
                    case Y:
                        vector2.y = Convert.ToSingle(reader.Value);
                        break;
                }
            }
            
            return vector2;
        }
    }
}
