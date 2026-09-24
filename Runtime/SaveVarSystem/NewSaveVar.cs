using System.IO;
using fefek5.Toys.Runtime;
using UnityEngine;

namespace fefek5.SaveDataVariable.Runtime
{
    public class NewSaveVar { }

    public class NewSaveVar<T> : NewSaveVar
    {
        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public string Path => GetPath(RelativePath);

        #region Inspector Fields
        
        [field: SerializeField] public SaveKey SaveKey { get; private set; }
        
        [field: SerializeField] public T DefaultValue { get; private set; }
        
        [field: SerializeField] public string RelativePath { get; private set; }

        #endregion

        private HasValue<T> _value;
        
        #region Getrers and Setters

        public T GetValue() => GetSaveData().GetKey(SaveKey, DefaultValue);

        public async Awaitable<T> GetValueAsync()
        {
            var saveData = await GetSaveDataAsync();
            
            return saveData.GetKey(SaveKey, DefaultValue);
        }
        
        public void SetValue(T value)
        {
            var saveData = GetSaveData();
            
            saveData.SetKey(SaveKey, value);

            saveData.Save(Path);
        }

        public async void SetValueAsync(T value)
        {
            var saveData = await GetSaveDataAsync();
            
            saveData.SetKey(SaveKey, value);
            
            await saveData.SaveAsync(Path);
        }

        public SaveData GetSaveData()
        {
            var saveData = new SaveData();
            
            saveData.Load(Path);

            return saveData;
        }

        public async Awaitable<SaveData> GetSaveDataAsync()
        {
            var saveData = new SaveData();
            
            await saveData.LoadAsync(Path);
            
            return saveData;
        }

        #endregion
        
        private static string GetPath(string relativePath) => 
            System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.persistentDataPath, relativePath));
    }
}