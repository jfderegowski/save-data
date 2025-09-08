using System;
using fefek5.Variables.SaveDataVariable.Runtime.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Converters
{
    public class Vector2IntConverter : JsonConverter<Vector2Int>
    {
        private const string X = "x";
        private const string Y = "y";
        
        public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            writer.TryWriteType<Vector2Int>(serializer);

            writer.WritePropertyName(X);
            writer.WriteValue(value.x);
            
            writer.WritePropertyName(Y);
            writer.WriteValue(value.y);
            
            writer.WriteEndObject();
        }

        public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var vector2Int = new Vector2Int();
            
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
                        vector2Int.x = Convert.ToInt32(reader.Value);
                        break;
                    case Y:
                        vector2Int.y = Convert.ToInt32(reader.Value);
                        break;
                }
            }
            
            return vector2Int;
        }
    }
}
