using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using fefek5.SaveDataVariable.Runtime.Extensions;
using fefek5.SaveDataVariable.Runtime.Settings;
using fefek5.SerializableGuidVariable.Runtime;
using fefek5.Systems.EncryptionSystem;
using fefek5.Toys.Runtime.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.SaveDataVariable.Runtime
{
    /// <summary>
    /// SaveData class that can be used to save and load data
    /// </summary>
    [Serializable]
    public class SaveData
    {
        #region Properties

        /// <summary>
        /// Data that will be serialized to save file
        /// [<see cref="SaveKey"/>] - Is a key that contains string key and SerializableGuid key
        /// [object] - Is a value that will be saved
        /// </summary>
        /// <example>Example of how to get a value
        /// <code>
        /// private SaveData _saveData;
        ///  
        /// // Get or set position using string key
        /// _saveData.Data["Position"] = transform.position;
        /// transform.position = _saveData.Data["Position"] as Vector3
        ///  
        /// // Get or set position using Guid key
        /// var guid = Guid.NewGuid();
        /// _saveData.Data[guid] = transform.position;
        /// transform.position = _saveData.Data[guid] as Vector3
        ///  
        /// // Get or set position using SerializableGuid key
        /// var serializableGuid = SerializableGuid.NewGuid();
        /// _saveData.Data[serializableGuid] = transform.position;
        /// transform.position = _saveData.Data[serializableGuid] as Vector3
        ///
        /// // Get or set position using SaveKey key
        /// var saveKey = SaveKey.NewKey();
        /// _saveData.Data[saveKey] = transform.position;
        /// transform.position = _saveData.Data[saveKey] as Vector3
        /// </code>
        /// </example>
        public Dictionary<SaveKey, object> Data { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance of BetterSaveData
        /// </summary>
        public SaveData() => Data = new Dictionary<SaveKey, object>();

        /// <summary>
        /// Create a new instance of BetterSaveData
        /// </summary>
        /// <param name="data">Data to be copied</param>
        public SaveData(Dictionary<SaveKey, object> data) => Data = new Dictionary<SaveKey, object>(data);

        /// <summary>
        /// Create a new instance of BetterSaveData
        /// </summary>
        /// <param name="saveData">SaveData to be copied</param>
        public SaveData(SaveData saveData) => Data = new Dictionary<SaveKey, object>(saveData.Data);

        #endregion
        
        #region Getters

        /// <summary>
        /// Get value from Data by string
        /// If key not found, default value will be returned but the key will be NOT added to Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>Value that will be found in Data or default value</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private string _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     return _saveData.GetKey(_saveKey, Vector3.zero);
        /// }
        /// </code>
        /// </example>
        public T GetKey<T>(string saveKey, T defaultValue) => Data.GetAs(saveKey, defaultValue);

        /// <summary>
        /// Get value from Data by Guid
        /// If key not found, default value will be returned but the key will be NOT added to Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>Value that will be found in Data or default value</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private Guid _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     return _saveData.GetKey(_saveKey, Vector3.zero);
        /// }
        /// </code>
        /// </example>
        public T GetKey<T>(Guid saveKey, T defaultValue) => Data.GetAs(saveKey, defaultValue);
        
        /// <summary>
        /// Get value from Data by SerializableGuid
        /// If key not found, default value will be returned but the key will be NOT added to Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>Value that will be found in Data or default value</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SerializableGuid _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     return _saveData.GetKey(_saveKey, Vector3.zero);
        /// }
        /// </code>
        /// </example>
        public T GetKey<T>(SerializableGuid saveKey, T defaultValue) => Data.GetAs(saveKey, defaultValue);

        /// <summary>
        /// Get value from Data by SaveKey
        /// If key not found, default value will be returned but the key will be NOT added to Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>Value that will be found in Data or default value</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SaveKey _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     return _saveData.GetKey(_saveKey, Vector3.zero);
        /// }
        /// </code>
        /// </example>
        public T GetKey<T>(SaveKey saveKey, T defaultValue) => Data.GetAs(saveKey, defaultValue);

        /// <summary>
        /// Get value from Data by string
        /// If key not found, default value will be out but the key will be NOT added to Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="value">The value that will be found in Data or default value</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>This instance of SaveData</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private string _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveKey, Vector3.zero, out var position);
        ///   
        ///     return position;
        /// }
        /// </code>
        /// </example>
        public SaveData GetKey<T>(string saveKey, T defaultValue, out T value)
        {
            value = Data.GetAs(saveKey, defaultValue);
            return this;
        }
        
        /// <summary>
        /// Get value from Data by Guid
        /// If key not found, default value will be out but the key will be NOT added to Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="value">The value that will be found in Data or default value</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>This instance of SaveData</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private Guid _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveKey, Vector3.zero, out var position);
        ///   
        ///     return position;
        /// }
        /// </code>
        /// </example>
        public SaveData GetKey<T>(Guid saveKey, T defaultValue, out T value)
        {
            value = Data.GetAs(saveKey, defaultValue);
            return this;
        }
        
        /// <summary>
        /// Get value from Data by SerializableGuid
        /// If key not found, default value will be out but the key will be NOT added to Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="value">The value that will be found in Data or default value</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>This instance of SaveData</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SerializableGuid _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveKey, Vector3.zero, out var position);
        ///   
        ///     return position;
        /// }
        /// </code>
        /// </example>
        public SaveData GetKey<T>(SerializableGuid saveKey, T defaultValue, out T value)
        {
            value = Data.GetAs(saveKey, defaultValue);
            return this;
        }
        
        /// <summary>
        /// Get value from Data by SaveKey
        /// If key not found, default value will be out but the key will be NOT added to Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="value">The value that will be found in Data or default value</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>This instance of SaveData</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SaveKey _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveKey, Vector3.zero, out var position);
        ///   
        ///     return position;
        /// }
        /// </code>
        /// </example>
        public SaveData GetKey<T>(SaveKey saveKey, T defaultValue, out T value)
        {
            value = Data.GetAs(saveKey, defaultValue);
            return this;
        }
        
        /// <summary>
        /// Get value from Data by string
        /// If key not found, default value will be out but the key will be NOT added to Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">The value that will be found in Data or default value</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>True if key is found, otherwise false</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private string _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     if (_saveData.TryGetKey(_saveKey, Vector3.zero, out var position))
        ///     {
        ///         return position;
        ///     }
        ///   
        ///     return Vector3.zero;
        /// }
        /// </code>
        /// </example>
        public bool TryGetKey<T>(string saveKey, out T value)
        {
            if (IsKeyExist(saveKey))
            {
                value = Data.GetAs(saveKey, default(T));
                return true;
            }

            value = default;
            return false;
        }
        
        /// <summary>
        /// Get value from Data by Guid
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">The value that will be found in Data or default value</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>True if key is found, otherwise false</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private Guid _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     if (_saveData.TryGetKey(_saveKey, Vector3.zero, out var position))
        ///     {
        ///         return position;
        ///     }
        ///   
        ///     return Vector3.zero;
        /// }
        /// </code>
        /// </example>
        public bool TryGetKey<T>(Guid saveKey, out T value)
        {
            if (IsKeyExist(saveKey))
            {
                value = Data.GetAs(saveKey, default(T));
                return true;
            }

            value = default;
            return false;
        }
        
        /// <summary>
        /// Get value from Data by SerializableGuid
        /// If key not found, default value will be out but the key will be NOT added to Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">The value that will be found in Data or default value</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>True if key is found, otherwise false</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SerializableGuid _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     if (_saveData.TryGetKey(_saveKey, Vector3.zero, out var position))
        ///     {
        ///         return position;
        ///     }
        ///   
        ///     return Vector3.zero;
        /// }
        /// </code>
        /// </example>
        public bool TryGetKey<T>(SerializableGuid saveKey, out T value)
        {
            if (IsKeyExist(saveKey))
            {
                value = Data.GetAs(saveKey, default(T));
                return true;
            }

            value = default;
            return false;
        }
        
        /// <summary>
        /// Get value from Data by SerializableGuid
        /// If key not found, default value will be out but the key will be NOT added to Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">The value that will be found in Data or default value</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <returns>True if key is found, otherwise false</returns>
        /// <example>Example of getting position from <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SaveKey _saveKey;
        ///  
        /// private Vector3 GetPositionFromSaveData()
        /// {
        ///     if (_saveData.TryGetKey(_saveKey, Vector3.zero, out var position))
        ///     {
        ///         return position;
        ///     }
        ///   
        ///     return Vector3.zero;
        /// }
        /// </code>
        /// </example>
        public bool TryGetKey<T>(SaveKey saveKey, out T value)
        {
            if (IsKeyExist(saveKey))
            {
                value = Data.GetAs(saveKey, default(T));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Read one value straight from a file, without keeping the rest of the file.
        /// A missing file gives the default value.
        /// Note! That this method is expensive (Deserialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="cancellationToken">Token that cancels the read</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <example>Example of getting position from <see cref="SaveData"/> directly from file
        /// <code>
        /// transform.position = await SaveData.GetKeyAsync(_saveFilePath, "Position", Vector3.zero);
        /// </code>
        /// </example>
        public static Awaitable<T> GetKeyAsync<T>(string filePath, SaveKey saveKey, T defaultValue,
            CancellationToken cancellationToken = default) =>
            GetKeyAsync(filePath, SaveSettings.Default, saveKey, defaultValue, cancellationToken);

        /// <summary>
        /// Read one value straight from a file, without keeping the rest of the file.
        /// A missing file gives the default value.
        /// Note! That this method is expensive (Deserialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveSettings">Settings for loading</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="cancellationToken">Token that cancels the read</param>
        /// <typeparam name="T">Type of value</typeparam>
        public static async Awaitable<T> GetKeyAsync<T>(string filePath, SaveSettings saveSettings, SaveKey saveKey,
            T defaultValue, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath)) return defaultValue;

            var tmpSaveData = new SaveData();

            await tmpSaveData.LoadAsync(filePath, saveSettings, cancellationToken);

            return tmpSaveData.GetKey(saveKey, defaultValue);
        }

        #endregion

        #region Setters

        /// <summary>
        /// Set value to Data by string
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <returns>This instance of SaveData</returns>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private string _saveKey;
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveKey, transform.position);
        /// }
        /// </code>
        /// </example>
        public SaveData SetKey(string saveKey, object value)
        {
            Data[saveKey] = value;
            return this;
        }

        /// <summary>
        /// Set value to Data by Guid
        /// </summary>
        /// <param name="saveKey"></param>
        /// <param name="value"></param>
        /// <returns>This instance of SaveData</returns>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private Guid _saveKey;
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveKey, transform.position);
        /// }
        /// </code>
        /// </example>
        public SaveData SetKey(Guid saveKey, object value)
        {
            Data[saveKey] = value;
            return this;
        }

        /// <summary>
        /// Set value to Data by SerializableGuid
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <returns>This instance of SaveData</returns>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SerializableGuid _saveKey;
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveKey, transform.position);
        /// }
        /// </code>
        /// </example>
        public SaveData SetKey(SerializableGuid saveKey, object value)
        {
            Data[saveKey] = value;
            return this;
        }

        /// <summary>
        /// Set value to Data by SaveKey
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <returns>This instance of SaveData</returns>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SaveKey _saveKey;
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveKey, transform.position);
        /// }
        /// </code>
        /// </example>
        public SaveData SetKey(SaveKey saveKey, object value)
        {
            Data[saveKey] = value;
            return this;
        }
        
        /// <summary>
        /// Write one value straight to a file, keeping the rest of the file as it is.
        /// A missing file is created.
        /// Note! That this method is expensive (Deserialize and Serialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <param name="cancellationToken">Token that cancels the read and the write</param>
        /// <example>Example of setting current position to <see cref="SaveData"/> directly in file
        /// <code>
        /// await SaveData.SetKeyAsync(_saveFilePath, "Position", transform.position);
        /// </code>
        /// </example>
        public static Awaitable SetKeyAsync(string filePath, SaveKey saveKey, object value,
            CancellationToken cancellationToken = default) =>
            SetKeyAsync(filePath, SaveSettings.Default, saveKey, value, cancellationToken);

        /// <summary>
        /// Write one value straight to a file, keeping the rest of the file as it is.
        /// A missing file is created.
        /// Note! That this method is expensive (Deserialize and Serialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveSettings">Settings for loading and saving</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <param name="cancellationToken">Token that cancels the read and the write</param>
        public static async Awaitable SetKeyAsync(string filePath, SaveSettings saveSettings, SaveKey saveKey,
            object value, CancellationToken cancellationToken = default)
        {
            var tmpSaveData = new SaveData();

            if (File.Exists(filePath))
                await tmpSaveData.LoadAsync(filePath, saveSettings, cancellationToken);

            tmpSaveData.SetKey(saveKey, value);

            await tmpSaveData.SaveAsync(filePath, saveSettings, cancellationToken);
        }

        #endregion

        #region Indexers

        /// <summary>
        /// Get value from Data by string
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        public object this[string saveKey]
        {
            get => Data[saveKey];
            set => Data[saveKey] = value;
        }

        /// <summary>
        /// Get value from Data by Guid
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        public object this[Guid saveKey]
        {
            get => Data[saveKey];
            set => Data[saveKey] = value;
        }

        /// <summary>
        /// Get value from Data by SerializableGuid
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        public object this[SerializableGuid saveKey]
        {
            get => Data[saveKey];
            set => Data[saveKey] = value;
        }

        /// <summary>
        /// Get value from Data by SaveKey
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        public object this[SaveKey saveKey]
        {
            get => Data[saveKey];
            set => Data[saveKey] = value;
        }

        #endregion

        #region Helpers
        
        /// <summary>
        /// Check if key exist in Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <returns>True if key exist, otherwise false</returns>
        public bool IsKeyExist(string saveKey) => Data.ContainsKey(saveKey);

        /// <summary>
        /// Check if key exist in Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <returns>True if key exist, otherwise false</returns>
        public bool IsKeyExist(SerializableGuid saveKey) => Data.ContainsKey(saveKey);
        
        /// <summary>
        /// Check if saveKey exist in Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <returns>True if key exist, otherwise false</returns>
        public bool IsKeyExist(Guid saveKey) => Data.ContainsKey(saveKey);
        
        /// <summary>
        /// Check if key exist in Data
        /// </summary>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <returns>True if key exist, otherwise false</returns>
        public bool IsKeyExist(SaveKey saveKey) => Data.ContainsKey(saveKey);

        /// <summary>
        /// Remove key from Data
        /// </summary>
        /// <param name="saveKey">Key to be removed</param>
        public void RemoveKey(string saveKey)
        {
            if (IsKeyExist(saveKey)) Data.Remove(saveKey);
        }

        /// <summary>
        /// Remove key from Data
        /// </summary>
        /// <param name="saveKey">Key to be removed</param>
        public void RemoveKey(SerializableGuid saveKey)
        {
            if (IsKeyExist(saveKey)) Data.Remove(saveKey);
        }
        
        /// <summary>
        /// Remove saveKey from Data
        /// </summary>
        /// <param name="saveKey">Key to be removed</param>
        public void RemoveKey(Guid saveKey)
        {
            if (IsKeyExist(saveKey)) Data.Remove(saveKey);
        }
        
        /// <summary>
        /// Remove saveKey from Data
        /// </summary>
        /// <param name="saveKey">Key to be removed</param>
        public void RemoveKey(SaveKey saveKey)
        {
            if (IsKeyExist(saveKey)) Data.Remove(saveKey);
        }

        #endregion

        #region Save

        /// <summary>
        /// Save data to file
        /// </summary>
        /// <param name="path">Path to file</param>
        public void Save(string path) => Save(path, SaveSettings.Default);

        /// <summary>
        /// Save data to file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="saveSettings">Settings for saving</param>
        public void Save(string path, SaveSettings saveSettings)
        {
            try
            {
                saveSettings ??= SaveSettings.Default;

                SaveCore(path, ToJson(saveSettings), saveSettings, CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.Log($"[SAVE-DATA] Error on save completion read the exception bellow");
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Save data to file. The json is built on the main thread, so it holds Data as it is at the call,
        /// then the encryption and the write run on a background thread.
        /// Unlike <see cref="Save(string)"/> this propagates exceptions to the caller instead of only logging them.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="cancellationToken">Token that cancels the write</param>
        public Awaitable SaveAsync(string path, CancellationToken cancellationToken = default) =>
            SaveAsync(path, SaveSettings.Default, cancellationToken);

        /// <summary>
        /// Save data to file. The json is built on the main thread, so it holds Data as it is at the call,
        /// then the encryption and the write run on a background thread.
        /// Unlike <see cref="Save(string, SaveSettings)"/> this propagates exceptions to the caller instead of only logging them.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="saveSettings">Settings for saving</param>
        /// <param name="cancellationToken">Token that cancels the write</param>
        public async Awaitable SaveAsync(string path, SaveSettings saveSettings,
            CancellationToken cancellationToken = default)
        {
            saveSettings ??= SaveSettings.Default;

            // Serializing reads Data and Unity objects (TransformConverter), so it has to stay on the main thread
            var jsonString = ToJson(saveSettings);

            cancellationToken.ThrowIfCancellationRequested();

            await Awaitable.BackgroundThreadAsync();

            try
            {
                SaveCore(path, jsonString, saveSettings, cancellationToken);
            }
            finally
            {
                // Also on failure, so the caller never continues on the background thread
                await Awaitable.MainThreadAsync();
            }
        }

        /// <summary>
        /// Encrypt and write an already serialized json. Safe to call from any thread.
        /// </summary>
        private static void SaveCore(string path, string jsonString, SaveSettings saveSettings,
            CancellationToken cancellationToken)
        {
            if (saveSettings.UseEncryption)
            {
                var password = saveSettings.Encryption.Password;
                var salt = saveSettings.Encryption.Salt;
                var initVector = saveSettings.Encryption.InitVector;

                jsonString = jsonString.Encrypt(password, salt, initVector);
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Create directory if it doesn't exist
            var directoryPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            File.WriteAllText(path, jsonString);

            // Delete excess files
            if (saveSettings.UsedFileLimit)
                DeleteExcessFiles(directoryPath, "*sav", saveSettings.FileLimit);

            Debug.Log($"[SAVE-DATA] Saved to file: {path} "
                      + $"{directoryPath.ToFileLink("[Folder]")} "
                      + $"{path.ToFileLink("[File]")}");
        }

        #endregion

        #region Load

        /// <summary>
        /// Load data from file
        /// </summary>
        /// <param name="path">Path to file</param>
        public void Load(string path) => Load(path, SaveSettings.Default);

        /// <summary>
        /// Load data from file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="saveSettings">Settings for loading</param>
        public void Load(string path, SaveSettings saveSettings)
        {
            try
            {
                saveSettings ??= SaveSettings.Default;

                Data = LoadCore(path, saveSettings, CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.Log($"[SAVE-DATA] Error on load completion read the exception bellow");
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Load data from file. The read, the decryption and the deserialization run on a background thread,
        /// Data is replaced back on the main thread.
        /// Unlike <see cref="Load(string)"/> this propagates exceptions to the caller instead of only logging them.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="cancellationToken">Token that cancels the read</param>
        public Awaitable LoadAsync(string path, CancellationToken cancellationToken = default) =>
            LoadAsync(path, SaveSettings.Default, cancellationToken);

        /// <summary>
        /// Load data from file. The read, the decryption and the deserialization run on a background thread,
        /// Data is replaced back on the main thread.
        /// Unlike <see cref="Load(string, SaveSettings)"/> this propagates exceptions to the caller instead of only logging them.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="saveSettings">Settings for loading</param>
        /// <param name="cancellationToken">Token that cancels the read</param>
        public async Awaitable LoadAsync(string path, SaveSettings saveSettings,
            CancellationToken cancellationToken = default)
        {
            // Resolved here, SaveSettings.Default loads from Resources which only works on the main thread
            saveSettings ??= SaveSettings.Default;

            cancellationToken.ThrowIfCancellationRequested();

            await Awaitable.BackgroundThreadAsync();

            Dictionary<SaveKey, object> data;

            try
            {
                data = LoadCore(path, saveSettings, cancellationToken);
            }
            finally
            {
                // Also on failure, so the caller never continues on the background thread
                await Awaitable.MainThreadAsync();
            }

            Data = data;
        }

        /// <summary>
        /// Read, decrypt and deserialize a file without touching Data. Safe to call from any thread.
        /// </summary>
        private Dictionary<SaveKey, object> LoadCore(string path, SaveSettings saveSettings,
            CancellationToken cancellationToken)
        {
            var jsonText = File.ReadAllText(path);

            cancellationToken.ThrowIfCancellationRequested();

            if (saveSettings.UseEncryption)
            {
                var password = saveSettings.Encryption.Password;
                var salt = saveSettings.Encryption.Salt;
                var initVector = saveSettings.Encryption.InitVector;

                jsonText = jsonText.Decrypt(password, salt, initVector);
            }

            var data = !jsonText.IsBlank()
                ? FromJson(jsonText, saveSettings).Data
                : new Dictionary<SaveKey, object>();

            Debug.Log($"[SAVE-DATA] Loaded from File: {path} "
                      + $"{Path.GetDirectoryName(path).ToFileLink("[Folder]")} "
                      + $"{path.ToFileLink("[File]")}");

            return data;
        }

        #endregion

        #region ToJson

        /// <summary>
        /// Convert SaveData to Json string
        /// </summary>
        /// <returns>Json string</returns>
        public string ToJson() => ToJson(SaveSettings.Default);

        /// <summary>
        /// Convert SaveData to Json string
        /// </summary>
        /// <param name="saveSettings">Settings for serialization</param>
        /// <returns>Json string</returns>
        public string ToJson(SaveSettings saveSettings) =>
            saveSettings.UseJsonCustomSettings
                ? ToJson(saveSettings.JsonCustomSettings)
                : ToJson(new JsonSerializerSettings());

        /// <summary>
        /// Convert SaveData to Json string
        /// </summary>
        /// <param name="jsonSettings">Settings for serialization</param>
        /// <returns>Json string</returns>
        public string ToJson(JsonSettings jsonSettings) => ToJson(jsonSettings.JsonSerializerSettings);

        /// <summary>
        /// Convert SaveData to Json string
        /// </summary>
        /// <param name="jsonSerializerSettings">Settings for serialization</param>
        /// <returns>Json string</returns>
        public string ToJson(JsonSerializerSettings jsonSerializerSettings) =>
            JsonConvert.SerializeObject(new SaveData(this), jsonSerializerSettings);

        #endregion

        #region FromJson

        /// <summary>
        /// Convert Json string to SaveData
        /// </summary>
        /// <param name="json">Json string</param>
        /// <returns>SaveData</returns>
        public SaveData FromJson(string json) => FromJson(json, SaveSettings.Default);
        
        /// <summary>
        /// Convert Json string to SaveData
        /// </summary>
        /// <param name="jsonString">Json string</param>
        /// <param name="saveSettings">Settings for deserialization</param>
        /// <returns>SaveData</returns>
        public SaveData FromJson(string jsonString, SaveSettings saveSettings) =>
            saveSettings.UseJsonCustomSettings
                ? FromJson(jsonString, saveSettings.JsonCustomSettings)
                : FromJson(jsonString, new JsonSerializerSettings());

        /// <summary>
        /// Convert Json string to SaveData
        /// </summary>
        /// <param name="jsonString">Json string</param>
        /// <param name="jsonSettings">Settings for deserialization</param>
        /// <returns>SaveData</returns>
        public SaveData FromJson(string jsonString, JsonSettings jsonSettings) =>
            FromJson(jsonString, jsonSettings.JsonSerializerSettings);
            
        
        /// <summary>
        /// Convert Json string to SaveData
        /// </summary>
        /// <param name="jsonString">Json string</param>
        /// <param name="jsonSerializerSettings">Settings for deserialization</param>
        /// <returns>SaveData</returns>
        public SaveData FromJson(string jsonString, JsonSerializerSettings jsonSerializerSettings) => 
            JsonConvert.DeserializeObject<SaveData>(jsonString, jsonSerializerSettings);

        #endregion
        
        #region File Management

        /// <summary>
        /// Delete excess files in folder
        /// </summary>
        /// <param name="folderPath">Path to folder</param>
        /// <param name="searchPattern">Search pattern for files (for example: *.sav)</param>
        /// <param name="fileLimit">Limit of files in folder</param>
        public static void DeleteExcessFiles(string folderPath, string searchPattern, int fileLimit)
        {
            if (fileLimit <= 0) return;

            var saveFiles = GetFiles(folderPath, searchPattern).SortOldestFirst().ToArray();
            var filesToDelete = saveFiles.Length - fileLimit;

            if (filesToDelete <= 0) return;
            
            for (var i = 0; i < filesToDelete; i++)
                saveFiles[i].Delete();
        }

        /// <summary>
        /// Get save files from folder
        /// </summary>
        /// <param name="folderPath">Path to folder</param>
        /// <param name="searchPattern"></param>
        /// <returns>Array of save files</returns>
        public static FileInfo[] GetFiles(string folderPath, string searchPattern) =>
            !folderPath.IsBlank() && !searchPattern.IsBlank() && Directory.Exists(folderPath)
                ? new DirectoryInfo(folderPath).GetFiles(searchPattern, SearchOption.AllDirectories)
                : null;

        #endregion
    }
}