using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace fefek5.SaveDataVariable.Runtime
{
    public static class SaveVarStorage
    {
        private static readonly Dictionary<string, SaveData> _saveDatas = new();
        private static readonly Dictionary<string, bool> _syncStatuses = new();

        public static string GetFullPath(string relativePath)
        {
            return Application.persistentDataPath + "/" + relativePath;
        }

        public static SaveData GetSaveData(string relativePath)
        {
            if (_saveDatas.TryGetValue(relativePath, out var saveData))
                return saveData;
            
            var loadedSaveData = new SaveData();
            var isSync = false;
            
            if (File.Exists(GetFullPath(relativePath)))
            {
                loadedSaveData.Load(GetFullPath(relativePath));
                isSync = true;
            }
                
            AddSaveData(relativePath, loadedSaveData, isSync);
                
            return loadedSaveData;
        }

        private static void AddSaveData(string relativePath, SaveData saveData, bool isSync = false)
        {
            if (!_saveDatas.TryAdd(relativePath, saveData))
                throw new InvalidOperationException($"SaveData {relativePath} is already in use");
            
            if (!_syncStatuses.TryAdd(relativePath, isSync))
                throw new InvalidOperationException($"SaveData {relativePath} is already in use");
        }

        public static bool GetIsSync(string relativePath) => 
            _syncStatuses.TryGetValue(relativePath, out var isSync) 
                ? isSync 
                : throw new NullReferenceException("There is no Sync Status");

        private static void SetIsSync(string relativePath, bool isSync)
        {
            if (!_syncStatuses.ContainsKey(relativePath))
                throw new KeyNotFoundException($"SaveData {relativePath} is not found");
            
            _syncStatuses[relativePath] = isSync;
        }

        public static void SaveToFile()
        {
            foreach (var relativePath in _saveDatas.Keys) 
                SaveToFile(relativePath);
        }
        
        public static void SaveToFile(string relativePath)
        {
            if (GetIsSync(relativePath)) return;
            
            if (_saveDatas.TryGetValue(relativePath, out var saveData))
            {
                if (saveData == null)
                    throw new NullReferenceException("SaveData is null");
                
                saveData.Save(GetFullPath(relativePath));
                
                SetIsSync(relativePath, true);
            }
        }

        #region GetAndSetValues

        public static T GetValue<T>(string relativePath, SaveKey saveKey, T defaultValue)
        {
            var saveData = GetSaveData(relativePath);

            return saveData.GetKey(saveKey, defaultValue);
        }

        public static void SetValue<T>(string relativePath, SaveKey saveKey, T value, bool saveToFile = true)
        {
            var saveData = GetSaveData(relativePath);

            if (saveData.TryGetKey(saveKey, out T currentValue))
            {
                if (currentValue == null && value == null) return;
                
                if (currentValue != null && currentValue.Equals(value)) return;
            }
            
            saveData.SetKey(saveKey, value);
            
            SetIsSync(relativePath, false);
            
            if (saveToFile) 
                SaveToFile(relativePath);
        }

        #endregion
    }
    
    [Serializable]
    public struct SaveVar<T> : IEquatable<SaveVar<T>>
    {
        [field: SerializeField] public SaveKey SaveKey { get; private set; }
        [field: SerializeField] public T DefaultValue { get; private set; }

        public T Value
        {
            get => SaveVarStorage.GetValue(RelativePath, SaveKey, DefaultValue);
            set => SaveVarStorage.SetValue(RelativePath, SaveKey, value);
        }

        public readonly string RelativePath;

        public void SetValueWithoutSaveing(T value) => 
            SaveVarStorage.SetValue(RelativePath, SaveKey, value, false);

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