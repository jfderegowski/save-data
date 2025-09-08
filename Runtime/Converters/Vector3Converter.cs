using System;
using fefek5.Variables.SaveDataVariable.Runtime.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Converters
{

    public class Vector3Converter : JsonConverter<Vector3>
    {
        private const string X = "x";
        private const string Y = "y";
        private const string Z = "z";
        
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            writer.TryWriteType<Vector3>(serializer);
            
            writer.WritePropertyName(X);
            writer.WriteValue(value.x);
            
            writer.WritePropertyName(Y);
            writer.WriteValue(value.y);
            
            writer.WritePropertyName(Z);
            writer.WriteValue(value.z);
            
            writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, 
            bool hasExistingValue, JsonSerializer serializer)
        {
            var vector3 = new Vector3();
            
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
                        vector3.x = Convert.ToSingle(reader.Value);
                        break;
                    case Y:
                        vector3.y = Convert.ToSingle(reader.Value);
                        break;
                    case Z:
                        vector3.z = Convert.ToSingle(reader.Value);
                        break;
                }
            }   
            
            return vector3;
        }
    }
}
