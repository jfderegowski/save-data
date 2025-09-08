using System;
using fefek5.Variables.SaveDataVariable.Runtime.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Converters
{ 
    public class QuaternionConverter : JsonConverter<Quaternion>
    {
        private const string X = "x";
        private const string Y = "y";
        private const string Z = "z";
        private const string W = "w";
        
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            writer.TryWriteType<Quaternion>(serializer);
            
            writer.WritePropertyName(X);
            writer.WriteValue(value.x);
            
            writer.WritePropertyName(Y);
            writer.WriteValue(value.y);
            
            writer.WritePropertyName(Z);
            writer.WriteValue(value.z);
            
            writer.WritePropertyName(W);
            writer.WriteValue(value.w);
            
            writer.WriteEndObject();
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var quaternion = new Quaternion();
            
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
                        quaternion.x = Convert.ToSingle(reader.Value);
                        break;
                    case Y:
                        quaternion.y = Convert.ToSingle(reader.Value);
                        break;
                    case Z:
                        quaternion.z = Convert.ToSingle(reader.Value);
                        break;
                    case W:
                        quaternion.w = Convert.ToSingle(reader.Value);
                        break;
                }
            }

            return quaternion;
        }
    }
}
