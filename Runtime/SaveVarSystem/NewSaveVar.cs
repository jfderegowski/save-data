using System;
using System.IO;
using fefek5.Toys.Runtime;
using UnityEngine;

namespace fefek5.SaveDataVariable.Runtime
{
    [Serializable]
    public class NewSaveVar { }

    [Serializable]
    public class NewSaveVar<T> : NewSaveVar
    {
        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public bool IsDirty => _value.hasValue;
        
        public string Path => GetPath(RelativePath);

        #region Inspector Fields
        
        [field: SerializeField] public SaveKey SaveKey { get; private set; }
        
        [field: SerializeField] public T DefaultValue { get; private set; }
        
        [field: SerializeField] public string RelativePath { get; private set; }

        #endregion

        #region Privete Fields

        private HasValue<T> _value = new(default, false);

        #endregion
        
        #region Getrers and Setters

        public T GetValue() => _value.hasValue ? _value.value : GetSaveData().GetKey(SaveKey, DefaultValue);

        public async Awaitable<T> GetValueAsync()
        {
            if (_value.hasValue)
                return _value.value;
            
            var saveData = await GetSaveDataAsync();
            
            return saveData.GetKey(SaveKey, DefaultValue);
        }

        public void SetValue(T value)
        {
            _value.Set(default, false);

            var saveData = GetSaveData();

            saveData.SetKey(SaveKey, value);

            saveData.Save(Path);
        }

        public async Awaitable SetValueAsync(T value)
        {
            _value.Set(default, false);

            var saveData = await GetSaveDataAsync();

            saveData.SetKey(SaveKey, value);

            await saveData.SaveAsync(Path);
        }

        public void SetValueWithoutNotifying(T value) => _value.Set(value, true);

        public SaveData GetSaveData()
        {
            var saveData = new SaveData();
            
            if(File.Exists(Path))
                saveData.Load(Path);

            return saveData;
        }

        public async Awaitable<SaveData> GetSaveDataAsync()
        {
            var saveData = new SaveData();

            if (File.Exists(Path))
                await saveData.LoadAsync(Path);
            
            return saveData;
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
        
        private static string GetPath(string relativePath) => 
            System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.persistentDataPath, relativePath));
    }
}