using System;
using fefek5.Variables.SaveDataVariable.Runtime.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Converters
{
    public class Vector4Converter : JsonConverter<Vector4>
    {
        private const string X = "x";
        private const string Y = "y";
        private const string Z = "z";
        private const string W = "w";

        public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            writer.TryWriteType<Vector4>(serializer);

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

        public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var vector4 = new Vector4();

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
                        vector4.x = Convert.ToSingle(reader.Value);
                        break;
                    case Y:
                        vector4.y = Convert.ToSingle(reader.Value);
                        break;
                    case Z:
                        vector4.z = Convert.ToSingle(reader.Value);
                        break;
                    case W:
                        vector4.w = Convert.ToSingle(reader.Value);
                        break;
                }
            }

            return vector4;
        }
    }
}
