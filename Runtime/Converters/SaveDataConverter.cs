using System;
using fefek5.Common.Runtime.Extensions;
using fefek5.Variables.HasValueVariable.Runtime;
using fefek5.Variables.SaveDataVariable.Runtime.Settings;
using Newtonsoft.Json;

namespace fefek5.Variables.SaveDataVariable.Runtime.Converters
{
    /// <summary>
    /// Converter for SaveData.
    /// </summary>
    public class SaveDataConverter : JsonConverter<SaveData>
    {
        private readonly JsonSettings _jsonSettings;

        #region Constructors

        public SaveDataConverter(JsonSettings jsonSettings) => _jsonSettings = jsonSettings;

        #endregion

        public override void WriteJson(JsonWriter writer, SaveData value, JsonSerializer serializer)
        {
            WriteStartFileComment(writer);

            var converterVariant = GetConverterVariant(_jsonSettings);
            
            converterVariant.Write(writer, value, serializer);
        }
        
        public override SaveData ReadJson(JsonReader reader, Type objectType, SaveData existingValue, 
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            while (reader.TokenType == JsonToken.Comment)
                reader.Read();

            var saveData = existingValue ?? new SaveData();

            var converterVariant = GetConverterVariant(_jsonSettings);
            
            converterVariant.Read(reader, saveData, serializer);
            
            return saveData;
        }
        
        private void WriteStartFileComment(JsonWriter writer)
        {
            if (!_jsonSettings.HasFileComment) return;
            
            if (_jsonSettings.FileComment.IsBlank()) return;
                
            writer.WriteComment(_jsonSettings.FileComment);
            writer.WriteWhitespace("\n\n");
        }
        
        private static SaveDataConverterVariant GetConverterVariant(JsonSettings jsonSettings)
        {
            if (jsonSettings.Formatting == Formatting.None)
            {
                if (!jsonSettings.HasWriteComment) return new FnCnVariant();
                
                if (jsonSettings.WriteComment == JsonSettings.CommentPosition.BeforeObject)
                    return new FnCboVariant();
                    
                return new FnCaoVariant();
            }
            
            if (!jsonSettings.HasWriteComment) return new FiCnVariant();

            if (jsonSettings.WriteComment == JsonSettings.CommentPosition.BeforeObject)
                return new FiCboVariant();

            return new FiCaoVariant();
        }
    }

    /// <summary>
    /// Base class for SaveDataConverter variants.
    /// </summary>
    public abstract class SaveDataConverterVariant
    {
        public abstract void Write(JsonWriter writer, SaveData value, JsonSerializer serializer);

        public abstract SaveData Read(JsonReader reader, SaveData saveData, JsonSerializer serializer);

        protected virtual void WriteComment(JsonWriter writer, HasValue<string> comment)
        {
            if (!comment.hasValue) return;

            writer.WriteComment(comment.value);
        }

        private protected static void WriteIdentificationComment(JsonWriter writer)
        {
            if (!writer.Path.IsBlank())
                writer.WriteComment("SaveData");
        }

        private protected static bool IsBetterSaveData(JsonReader reader)
        {
            if (reader.TokenType != JsonToken.Comment) return false;

            var comment = reader.Value?.ToString();

            if (string.IsNullOrEmpty(comment) || string.IsNullOrWhiteSpace(comment)) return false;

            var isBetterSaveData = comment.StartsWith("SaveData");

            if (isBetterSaveData) reader.Read();

            return isBetterSaveData;
        }
    }

    /// <summary>
    /// Formatting.None, CommentPosition.None
    /// </summary>
    public class FnCnVariant : SaveDataConverterVariant
    {
        public override void Write(JsonWriter writer, SaveData value, JsonSerializer serializer)
        {
            WriteIdentificationComment(writer);

            writer.WriteStartObject();

            foreach (var (saveKey, saveValue) in value.Data)
            {
                writer.WritePropertyName(saveKey.ToString());

                if (saveValue is SaveData saveData)
                    Write(writer, saveData, serializer);
                else serializer.Serialize(writer, saveValue);
            }

            writer.WriteEndObject();
        }
        
        public override SaveData Read(JsonReader reader, SaveData saveData, JsonSerializer serializer)
        {
            while (reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    reader.Read();
                    continue;
                }

                var propertyName = reader.Value?.ToString();
                
                reader.Read();

                var propertyValue = IsBetterSaveData(reader)
                    ? serializer.Deserialize(reader, typeof(SaveData))
                    : serializer.Deserialize(reader);

                saveData.SetKey(new SaveKey(propertyName), propertyValue);
            }

            return saveData;
        }
    }
    
    /// <summary>
    /// Formatting.None, CommentPosition.BeforeObject
    /// </summary>
    public class FnCboVariant : SaveDataConverterVariant
    {
        public override void Write(JsonWriter writer, SaveData value, JsonSerializer serializer)
        {
            WriteIdentificationComment(writer);

            writer.WriteStartObject();

            foreach (var (saveKey, saveValue) in value.Data)
            {
                WriteComment(writer, saveKey.Comment);

                writer.WritePropertyName(saveKey.ToString());

                if (saveValue is SaveData saveData) Write(writer, saveData, serializer);
                else serializer.Serialize(writer, saveValue);
            }

            writer.WriteEndObject();
        }
        
        public override SaveData Read(JsonReader reader, SaveData saveData, JsonSerializer serializer)
        {
            var jsonComment = string.Empty;

            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        var jsonPropertyName = reader.Value?.ToString();

                        reader.Read();

                        var isBetterSaveData = IsBetterSaveData(reader);

                        var jsonPropertyValue = isBetterSaveData
                            ? serializer.Deserialize(reader, typeof(SaveData))
                            : serializer.Deserialize(reader);

                        var saveKey = new SaveKey(jsonPropertyName, jsonComment);
                        saveData.SetKey(saveKey, jsonPropertyValue);

                        jsonComment = string.Empty;
                        break;

                    case JsonToken.Comment:
                        jsonComment = reader.Value?.ToString();
                        break;
                }
            }

            return saveData;
        }
    }

    /// <summary>
    /// Formatting.None, CommentPosition.AfterObject
    /// </summary>
    public class FnCaoVariant : SaveDataConverterVariant
    {
        public override void Write(JsonWriter writer, SaveData value, JsonSerializer serializer)
        {
            WriteIdentificationComment(writer);

            writer.WriteStartObject();

            foreach (var (saveKey, saveValue) in value.Data)
            {
                writer.WritePropertyName(saveKey.ToString());

                if (saveValue is SaveData betterSaveData)
                    Write(writer, betterSaveData, serializer);
                else serializer.Serialize(writer, saveValue);

                WriteComment(writer, saveKey.Comment);
            }

            writer.WriteEndObject();
        }
        
        public override SaveData Read(JsonReader reader, SaveData saveData, JsonSerializer serializer)
        {
            var readNextToken = true;
            while (true)
            {
                if (readNextToken)
                {
                    if (!reader.Read())
                        break;
                }

                readNextToken = true;

                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType != JsonToken.PropertyName)
                    continue;

                var propertyName = reader.Value?.ToString();
                reader.Read();

                var isBetterSaveData = IsBetterSaveData(reader);
                var propertyValue = isBetterSaveData
                    ? serializer.Deserialize(reader, typeof(SaveData))
                    : serializer.Deserialize(reader);

                var comment = string.Empty;
                var hasReadAfterValue = reader.Read();

                if (hasReadAfterValue && reader.TokenType == JsonToken.Comment)
                {
                    comment = reader.Value?.ToString();
                    readNextToken = true;
                }
                else readNextToken = false;

                saveData.SetKey(new SaveKey(propertyName, comment), propertyValue);
            }

            return saveData;
        }
    }
    
    /// <summary>
    /// Formatting.Indented, CommentPosition.None
    /// </summary>
    public class FiCnVariant : SaveDataConverterVariant
    {
        // The indentation is handled by the JsonSerializerSettings.
        // So we don't need to do anything special here.
        // The code is copied from FnCnVariant.
        
        public override void Write(JsonWriter writer, SaveData value, JsonSerializer serializer)
        {
            WriteIdentificationComment(writer);

            writer.WriteStartObject();

            foreach (var (saveKey, saveValue) in value.Data)
            {
                writer.WritePropertyName(saveKey.ToString());

                if (saveValue is SaveData saveData)
                    Write(writer, saveData, serializer);
                else serializer.Serialize(writer, saveValue);
            }

            writer.WriteEndObject();
        }
        
        public override SaveData Read(JsonReader reader, SaveData saveData, JsonSerializer serializer)
        {
            while (reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    reader.Read();
                    continue;
                }

                var propertyName = reader.Value?.ToString();
                
                reader.Read();

                var propertyValue = IsBetterSaveData(reader)
                    ? serializer.Deserialize(reader, typeof(SaveData))
                    : serializer.Deserialize(reader);

                saveData.SetKey(new SaveKey(propertyName), propertyValue);
            }

            return saveData;
        }
    }
    
    /// <summary>
    /// Formatting.Indented, CommentPosition.BeforeObject
    /// </summary>
    public class FiCboVariant : SaveDataConverterVariant
    {
        private int _depth;
        
        public override void Write(JsonWriter writer, SaveData value, JsonSerializer serializer)
        {
            WriteIdentificationComment(writer);

            _depth += 2;

            writer.WriteStartObject();

            foreach (var (saveKey, saveValue) in value.Data)
            {
                WriteComment(writer, saveKey.Comment);

                writer.WritePropertyName(saveKey.ToString());

                if (saveValue is SaveData saveData)
                    Write(writer, saveData, serializer);
                else serializer.Serialize(writer, saveValue);
            }

            writer.WriteEndObject();

            _depth -= 2;
        }
        
        protected override void WriteComment(JsonWriter writer, HasValue<string> comment)
        {
            if (!comment.hasValue) return;

            var indentation = new string(' ', _depth);
            writer.WriteWhitespace($"\n{indentation}");
            writer.WriteComment(comment.value);
        }
        
        public override SaveData Read(JsonReader reader, SaveData saveData, JsonSerializer serializer)
        {
            var jsonComment = string.Empty;

            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        var jsonPropertyName = reader.Value?.ToString();

                        reader.Read();

                        var isBetterSaveData = IsBetterSaveData(reader);

                        var jsonPropertyValue = isBetterSaveData
                            ? serializer.Deserialize(reader, typeof(SaveData))
                            : serializer.Deserialize(reader);

                        var saveKey = new SaveKey(jsonPropertyName, jsonComment);
                        saveData.SetKey(saveKey, jsonPropertyValue);

                        jsonComment = string.Empty;
                        break;

                    case JsonToken.Comment:
                        jsonComment = reader.Value?.ToString();
                        break;
                }
            }

            return saveData;
        }
    }

    /// <summary>
    /// Formatting.Indented, CommentPosition.AfterObject
    /// </summary>
    public class FiCaoVariant : SaveDataConverterVariant
    {
        public override void Write(JsonWriter writer, SaveData value, JsonSerializer serializer)
        {
            WriteIdentificationComment(writer);

            writer.WriteStartObject();

            foreach (var (saveKey, saveValue) in value.Data)
            {
                writer.WritePropertyName(saveKey.ToString());

                if (saveValue is SaveData betterSaveData)
                    Write(writer, betterSaveData, serializer);
                else serializer.Serialize(writer, saveValue);

                WriteComment(writer, saveKey.Comment);
            }

            writer.WriteEndObject();
        }
        
        protected override void WriteComment(JsonWriter writer, HasValue<string> comment)
        {
            if (!comment.hasValue) return;

            writer.WriteWhitespace(" ");
            writer.WriteComment(comment.value);
        }
        
        public override SaveData Read(JsonReader reader, SaveData saveData, JsonSerializer serializer)
        {
            var readNextToken = true;
            while (true)
            {
                if (readNextToken)
                {
                    if (!reader.Read())
                        break;
                }

                readNextToken = true;

                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType != JsonToken.PropertyName)
                    continue;

                var propertyName = reader.Value?.ToString();
                reader.Read();

                var isBetterSaveData = IsBetterSaveData(reader);
                var propertyValue = isBetterSaveData
                    ? serializer.Deserialize(reader, typeof(SaveData))
                    : serializer.Deserialize(reader);

                var comment = string.Empty;
                var hasReadAfterValue = reader.Read();

                if (hasReadAfterValue && reader.TokenType == JsonToken.Comment)
                {
                    comment = reader.Value?.ToString();
                    readNextToken = true;
                }
                else readNextToken = false;

                saveData.SetKey(new SaveKey(propertyName, comment), propertyValue);
            }

            return saveData;
        }
    }
}