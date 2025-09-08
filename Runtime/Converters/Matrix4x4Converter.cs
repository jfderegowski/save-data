using System;
using System.Collections.Generic;
using System.Linq;
using fefek5.Variables.SaveDataVariable.Runtime.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Converters
{
    public class Matrix4x4Converter : JsonConverter<Matrix4x4>
    {
        // https://github.com/Unity-Technologies/UnityCsReference/blob/2019.2/Runtime/Export/Math/Matrix4x4.cs#L21-L29
        private static readonly string[] _names = GetMemberNames();
        private static readonly Dictionary<string, int> _namesToIndex = GetNamesToIndex(_names);

        private static string[] GetMemberNames()
        {
            var indexes = new[] { "0", "1", "2", "3" };
            return indexes.SelectMany(row => indexes.Select(column => "m" + column + row)).ToArray();
        }

        private static Dictionary<string, int> GetNamesToIndex(string[] names)
        {
            var dict = new Dictionary<string, int>();
            
            for (var i = 0; i < names.Length; i++) 
                dict[names[i]] = i;
            
            return dict;
        }

        public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            writer.TryWriteType<Matrix4x4>(serializer);
            
            for (var i = 0; i < _names.Length; i++)
            {
                writer.WritePropertyName(_names[i]);
                writer.WriteValue(value[i]);
            }
            
            writer.WriteEndObject();
        }

        public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var matrix = new Matrix4x4();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader is not { TokenType: JsonToken.PropertyName, Value: not null }) continue;
                
                var propertyName = reader.Value.ToString();
                
                reader.Read();

                if (_namesToIndex.TryGetValue(propertyName, out var index))
                    matrix[index] = Convert.ToSingle(reader.Value);
            }
            
            return matrix;
        }
    }
}
