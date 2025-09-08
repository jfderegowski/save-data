using System;
using fefek5.Variables.SaveDataVariable.Runtime.Extensions;
using fefek5.Variables.SaveDataVariable.Runtime.Serializable;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Converters
{
    public class TransformConverter : JsonConverter<Transform>
    {
        private const string POSITION = "position";
        private const string ROTATION = "rotation";
        private const string SCALE = "scale";
        
        public override void WriteJson(JsonWriter writer, Transform value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.TryWriteType<SerializeTransform>(serializer);
            
            writer.WritePropertyName(POSITION);
            serializer.Serialize(writer, value.position, typeof(Vector3));
            
            writer.WritePropertyName(ROTATION);
            serializer.Serialize(writer, value.rotation, typeof(Quaternion));
            
            writer.WritePropertyName(SCALE);
            serializer.Serialize(writer, value.localScale, typeof(Vector3));
            
            writer.WriteEndObject();
        }

        public override Transform ReadJson(JsonReader reader, Type objectType, Transform existingValue,
            bool hasExistingValue, JsonSerializer serializer) => null;

        public override bool CanRead => false;
    }
}