using System;
using fefek5.Common.Runtime.Extensions;
using fefek5.Variables.HasValueVariable.Runtime;
using fefek5.Variables.SaveDataVariable.Runtime.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Settings
{
    [Serializable]
    public class JsonSettings
    {
        public enum CommentPosition
        {
            BeforeObject,
            AfterObject
        }

        [Serializable]
        internal class SaveFileComment
        {
            [SerializeField] private string _title;
            [SerializeField] private bool _writeGameVersion;
            [SerializeField] private bool _writeSaveTime;

            private HasValue<string> _fileComment = new(string.Empty, false);
            
            internal string GetFileComment()
            {
                if (_fileComment.hasValue)
                    return _fileComment.value;
                
                var comment = string.Empty;
                
                if (!string.IsNullOrEmpty(_title) && !string.IsNullOrWhiteSpace(_title))
                    comment += "    " + _title + "\n";
                
                if (_writeGameVersion)
                    comment += "    Game version: " + Application.version + "\n";
                
                if (_writeSaveTime)
                    comment += "    Save time: " + DateTime.Now + "\n";

                if (!string.IsNullOrEmpty(comment) || !string.IsNullOrWhiteSpace(comment))
                    comment = "\n" + comment;
                
                return comment;
            }

            internal void SetFileComment(string comment)
            {
                if (comment.IsBlank())
                {
                    _fileComment.hasValue = false;
                    return;
                }

                _fileComment.hasValue = true;
                _fileComment.value = comment;
            }
        }
        
        public bool HasWriteComment
        {
            get => _writeComment.hasValue;
            set => _writeComment.hasValue = value;
        }
        
        public CommentPosition WriteComment
        {
            get => _writeComment.value;
            set => _writeComment.value = value;
        }

        public bool HasFileComment
        {
            get => _fileComment.hasValue;
            set => _fileComment.hasValue = value;
        }

        public string FileComment
        {
            get => _fileComment.value.GetFileComment();
            set => _fileComment.value.SetFileComment(value);
        }

        public JsonSerializerSettings JsonSerializerSettings => new() {
            TypeNameHandling = TypeNameHandling,
            Formatting = Formatting,
            Converters = Converters
        };

        private JsonConverter[] Converters => new JsonConverter[] {
            // Converter for SaveData Variable
            new SaveDataConverter(this),
            
            // Converters for Unity types
            new Color32Converter(),
            new ColorConverter(),
            new Matrix4x4Converter(),
            new QuaternionConverter(),
            new Vector2Converter(),
            new Vector2IntConverter(),
            new Vector3Converter(),
            new Vector3IntConverter(),
            new Vector4Converter(),
            new TransformConverter()
        };

        [Header("Json")]
        public TypeNameHandling TypeNameHandling = TypeNameHandling.None;
        public Formatting Formatting = Formatting.Indented;
        
        [Header("Comments")]
        [SerializeField] private HasValue<CommentPosition> _writeComment = new(CommentPosition.BeforeObject, true);
        [SerializeField] private HasValue<SaveFileComment> _fileComment = new(new SaveFileComment(), false);
    }
}