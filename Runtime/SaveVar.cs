using System;
using System.Collections.Generic;
using UnityEngine;

namespace fefek5.SaveDataVariable.Runtime
{
    [Serializable]
    public struct SaveVar<T> : IEquatable<SaveVar<T>>
    {
        private static Dictionary<string, SaveData> _saveDatas = new();
        private static Dictionary<string, bool> _saveStatus = new();
        
        public SaveData SaveData => _saveDatas[RelativePath];

        public bool IsSync
        {
            get => _saveStatus.TryGetValue(RelativePath, out var isSync) && isSync;
            private set => _saveStatus[RelativePath] = value;
        }

        [field: SerializeField] public SaveKey SaveKey { get; private set; }
        [field: SerializeField] public T DefaultValue { get; private set; }

        public T Value
        {
            get
            {
                if (!IsSync) 
                    Sync();
                
                var value = GetValueWithoutSaveing();

                if (SaveData.IsKeyExist(SaveKey)) return value;
                
                SaveData.SetKey(SaveKey, value);
                Sync();

                return value;
            }
            set
            {
                SaveData.SetKey(SaveKey, value);
                Sync();
            }
        }

        public string SavePath => Application.persistentDataPath + "/" + RelativePath;

        public readonly string RelativePath;

        public T GetValueWithoutSaveing() => 
            SaveData.GetKey(SaveKey, DefaultValue);

        public void SetValueWithoutSaveing(T value)
        {
            SaveData.SetKey(SaveKey, value);
            IsSync = false;
        }

        public bool TrySaveToFile()
        {
            if (IsSync) 
                return false;

            Sync();
            
            return true;
        }

        public void Sync()
        {
            SaveData.Save(SavePath);
            IsSync = true;
        }

        #region Constructors

        public SaveVar(SaveKey saveKey, string relativePath = "SaveVars.json")
            : this(saveKey, default, relativePath) { }

        public SaveVar(SaveKey saveKey, T defaultValue, string relativePath = "SaveVars.json")
        {
            DefaultValue = defaultValue;
            SaveKey = saveKey;
            RelativePath = relativePath;
        }

        #endregion

        #region Conversion

        public static implicit operator T(SaveVar<T> saveVar) => saveVar.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is SaveVar<T> saveVar)
                return saveVar.Equals(this);

            if (obj is T value)
                return value.Equals(Value);

            return false;
        }
        
        public bool Equals(SaveVar<T> other) => 
            Value.Equals(other.Value);

        public static bool operator ==(SaveVar<T> saveVar1, SaveVar<T> saveVar2) => 
            saveVar1.Equals(saveVar2);

        public static bool operator !=(SaveVar<T> saveVar1, SaveVar<T> saveVar2) => !(saveVar1 == saveVar2);

        #endregion
    }
}