using System;
using fefek5.Variables.HasValueVariable.Runtime;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime.Settings
{
    [Serializable]
    public class SaveSettings
    {
        public static SaveSettings Default => DefaultSaveSettings.Instance.SaveSettings;
        
        public bool UsedFileLimit
        {
            get => _fileLimit.hasValue;
            set => _fileLimit.hasValue = value;
        }
        
        public int FileLimit
        {
            get => _fileLimit.value;
            set => _fileLimit.value = value;
        }
        
        public bool UseJsonCustomSettings
        {
            get => _jsonCustomSettings.hasValue;
            set => _jsonCustomSettings.hasValue = value;
        }
        
        public JsonSettings JsonCustomSettings
        {
            get => _jsonCustomSettings.value;
            set => _jsonCustomSettings.value = value;
        }
        
        public bool UseEncryption
        {
            get => _encryption.hasValue;
            set => _encryption.hasValue = value;
        }
        
        public EncryptionSettings Encryption
        {
            get => _encryption.value;
            set => _encryption.value = value;
        }
        
        [SerializeField] private HasValue<int> _fileLimit = new(10, false);
        [SerializeField] private HasValue<JsonSettings> _jsonCustomSettings = new(new JsonSettings(), true);
        [SerializeField] private HasValue<EncryptionSettings> _encryption = new(new EncryptionSettings(), false);
    }
}