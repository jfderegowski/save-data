using System;
using System.Collections.Generic;
using System.IO;
using fefek5.Toys.Runtime;
using UnityEngine;

namespace fefek5.SaveDataVariable.Runtime
{
    public class DirtableSaveData
    {
        public SaveData Origin;
        public SaveData Target;

        
    }
    
    [Serializable]
    public class NewSaveVar
    {
        internal static Dictionary<string, SaveData> SaveDatas = new();
        internal static Dictionary<string, HashSet<SaveKey>> Dirties = new();

        #region Properties

        public bool IsDirty => Dirties.TryGetValue(Path,  out var keys) && keys.Contains(SaveKey);

        public string Path => GetPath(RelativePath);

        #endregion
        
        #region Inspector Fields
        
        [field: SerializeField] public SaveKey SaveKey { get; private set; }
        
        [field: SerializeField] public string RelativePath { get; private set; }

        #endregion

        internal void SetDirty()
        {
            if (IsDirty)
                throw new InvalidOperationException("Value is arleady dirty!");

            if (Dirties.TryGetValue(Path, out var keys)) keys.Add(SaveKey);
            else Dirties[Path] = new HashSet<SaveKey> { SaveKey };
        }
        
        internal virtual void RemoveDirty()
        {
            if (!IsDirty)
                throw new InvalidOperationException("Value is not dirty!");
            
            Dirties[Path].Remove(SaveKey);
        }

        public void ApplyDirty()
        {
            if (!IsDirty)
                throw new InvalidOperationException("Value is not dirty!");
            
            
        }
        
        private static string GetPath(string relativePath) =>
            System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.persistentDataPath, relativePath));

        public static bool IsSaveDataDirty(string path) => Dirties.ContainsKey(path);

        public static void RemoveSaveDataDirty(string path)
        {
            if (!IsSaveDataDirty(path))
                throw new InvalidOperationException("SaveData is not dirty!");
            
            Dirties.Remove(path);
        }
    }

    [Serializable]
    public class NewSaveVar<T> : NewSaveVar
    {
        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        #region Inspector Fields
        
        [field: SerializeField] public T DefaultValue { get; private set; }
        
        #endregion

        private T _dirty;
        
        #region Getrers and Setters

        public T GetValue() => TryGetDirtyValue(out var dirty) ? dirty : GetSaveData().GetKey(SaveKey, DefaultValue);

        public async Awaitable<T> GetValueAsync()
        {
            if (TryGetDirtyValue(out var dirty))
                return dirty;
            
            var saveData = await GetSaveDataAsync();
            
            return saveData.GetKey(SaveKey, DefaultValue);
        }

        public void SetValue(T value)
        {
            if (IsDirty)
                RemoveDirty();

            var saveData = GetSaveData();

            saveData.SetKey(SaveKey, value);

            saveData.Save(Path);
        }

        public async Awaitable SetValueAsync(T value)
        {
            if (IsDirty)
                RemoveDirty();

            var saveData = await GetSaveDataAsync();

            saveData.SetKey(SaveKey, value);

            await saveData.SaveAsync(Path);
        }

        public void SetValueWithoutNotifying(T value)
        {
            GetSaveData().SetKey(SaveKey, value);
            SetDirty();
        }

        public SaveData GetSaveData()
        {
            if (SaveDatas.TryGetValue(Path, out var existingSaveData))
                return existingSaveData;
            
            var newSaveData = new SaveData();
            
            if(File.Exists(Path))
                newSaveData.Load(Path);
            
            SaveDatas.Add(Path, newSaveData);

            return newSaveData;
        }

        public async Awaitable<SaveData> GetSaveDataAsync()
        {
            if (SaveDatas.TryGetValue(Path, out var existingSaveData))
                return existingSaveData;
            
            var newSaveData = new SaveData();
            
            if(File.Exists(Path))
                await newSaveData.LoadAsync(Path);
            
            SaveDatas.Add(Path, newSaveData);

            return newSaveData;
        }

        private bool TryGetDirtyValue(out T dirty)
        {
            if (!IsDirty)
            {
                dirty =  default;
                return false;
            }

            dirty = GetDirtyValue();
            return true;
        }
        
        private T GetDirtyValue()
        {
            if (!IsDirty)
                throw new InvalidOperationException("Value is not dirty!");

            return _dirty;
        }

        internal override void RemoveDirty()
        {
            base.RemoveDirty();
            
            _dirty = default;
        }

        #endregion

        #region Push and Pull

        public void Push()
        {
            if (!IsDirty)
                throw new Exception("Value is not dirty!");

            SetValue(GetValue());
        }

        public async Awaitable PushAsync()
        {
            if (!IsDirty)
                throw new Exception("Value is not dirty!");

            await SetValueAsync(await GetValueAsync());
        }

        public void Pull()
        {
            if (!IsDirty)
                throw new Exception("Value is not dirty!");

            var saveData = GetSaveData();
            
            var value = saveData.GetKey(SaveKey, DefaultValue);

            SetValue(value);
        }

        public async Awaitable PullAsync()
        {
            if (!IsDirty)
                throw new Exception("Value is not dirty!");
            
            var  saveData = await GetSaveDataAsync();
            
            var value = saveData.GetKey(SaveKey, DefaultValue);
            
            await SetValueAsync(value);
        }

        #endregion
    }
}