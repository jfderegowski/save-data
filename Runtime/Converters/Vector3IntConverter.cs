using System;
using fefek5.Variables.SaveDataVariable.Runtime.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Converters
{
    public class Vector3IntConverter : JsonConverter<Vector3Int>
    {
        private const string X = "x";
        private const string Y = "y";
        private const string Z = "z";
        
        public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            writer.TryWriteType<Vector3Int>(serializer);
            
            writer.WritePropertyName(X);
            writer.WriteValue(value.x);
            
            writer.WritePropertyName(Y);
            writer.WriteValue(value.y);
            
            writer.WritePropertyName(Z);
            writer.WriteValue(value.z);
            
            writer.WriteEndObject();
        }

        public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var vector3Int = new Vector3Int();
            
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
                        vector3Int.x = Convert.ToInt32(reader.Value);
                        break;
                    case Y:
                        vector3Int.y = Convert.ToInt32(reader.Value);
                        break;
                    case Z:
                        vector3Int.z = Convert.ToInt32(reader.Value);
                        break;
                }
            }
            
            return vector3Int;
        }
    }
}
