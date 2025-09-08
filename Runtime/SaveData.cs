using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using fefek5.Common.Runtime.Extensions;
using fefek5.Systems.EncryptionSystem;
using fefek5.Variables.SaveDataVariable.Runtime.Extensions;
using fefek5.Variables.SaveDataVariable.Runtime.Settings;
using fefek5.Variables.SerializableGuidVariable.Runtime;
using Newtonsoft.Json;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime
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
        /// Get value from file by string
        /// Note! That this method is expensive (Deserialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="onGetKey">Action that will be invoked after load completion</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <example>Example of getting position from <see cref="SaveData"/> directly from file
        /// <code>
        /// private SaveData _saveData;
        /// private SaveKey _saveKey;
        /// private string _saveFilePath;
        ///  
        /// private void GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveFilePath, _saveKey, Vector3.zero, OnGetKey);
        /// }
        ///  
        /// private void OnGetKey(Vector3 position)
        /// {
        ///     transform.position = position;
        /// }
        /// </code>
        /// </example>
        public static void GetKey<T>(string filePath, string saveKey, T defaultValue, Action<T> onGetKey) => 
            GetKey(filePath, SaveSettings.Default, saveKey, defaultValue, onGetKey);

        /// <summary>
        /// Get value from file by string
        /// Note! That this method is expensive (Deserialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveSettings">Settings for loading</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="onGetKey">Action that will be invoked after load completion</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <example>Example of getting position from <see cref="SaveData"/> directly from file
        /// <code>
        /// private SaveData _saveData;
        /// private string _saveKey;
        /// private string _saveFilePath;
        /// private SaveSettings _saveSettings
        ///  
        /// private void GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveFilePath, _saveSettings, _saveKey, Vector3.zero, OnGetKey);
        /// }
        ///  
        /// private void OnGetKey(Vector3 position)
        /// {
        ///     transform.position = position;
        /// }
        /// </code>
        /// </example>
        public static void GetKey<T>(string filePath, SaveSettings saveSettings, string saveKey, T defaultValue, Action<T> onGetKey)
        {
            var tmpSaveData = new SaveData();
            
            tmpSaveData.Load(filePath, saveSettings, OnLoad);
            
            return;

            void OnLoad()
            {
                tmpSaveData.GetKey(saveKey, defaultValue, out var value);
                
                onGetKey?.Invoke(value);
            }
        }
        
        /// <summary>
        /// Get value from file by Guid
        /// Note! That this method is expensive (Deserialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="onGetKey">Action that will be invoked after load completion</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <example>Example of getting position from <see cref="SaveData"/> directly from file
        /// <code>
        /// private SaveData _saveData;
        /// private Guid _saveKey;
        /// private string _saveFilePath;
        ///  
        /// private void GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveFilePath, _saveKey, Vector3.zero, OnGetKey);
        /// }
        ///  
        /// private void OnGetKey(Vector3 position)
        /// {
        ///     transform.position = position;
        /// }
        /// </code>
        /// </example>
        public static void GetKey<T>(string filePath, Guid saveKey, T defaultValue, Action<T> onGetKey) => 
            GetKey(filePath, SaveSettings.Default, saveKey, defaultValue, onGetKey);
        
        /// <summary>
        /// Get value from file by Guid
        /// Note! That this method is expensive (Deserialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveSettings">Settings for loading</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="onGetKey">Action that will be invoked after load completion</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <example>Example of getting position from <see cref="SaveData"/> directly from file
        /// <code>
        /// private SaveData _saveData;
        /// private Guid _saveKey;
        /// private string _saveFilePath;
        /// private SaveSettings _saveSettings
        ///  
        /// private void GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveFilePath, _saveSettings, _saveKey, Vector3.zero, OnGetKey);
        /// }
        ///  
        /// private void OnGetKey(Vector3 position)
        /// {
        ///     transform.position = position;
        /// }
        /// </code>
        /// </example>
        public static void GetKey<T>(string filePath, SaveSettings saveSettings, Guid saveKey, T defaultValue, Action<T> onGetKey)
        {
            var tmpSaveData = new SaveData();
            
            tmpSaveData.Load(filePath, saveSettings, OnLoad);
            
            return;

            void OnLoad()
            {
                tmpSaveData.GetKey(saveKey, defaultValue, out var value);
                
                onGetKey?.Invoke(value);
            }
        }
        
        /// <summary>
        /// Get value from file by SerializableGuid
        /// Note! That this method is expensive (Deserialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="onGetKey">Action that will be invoked after load completion</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <example>Example of getting position from <see cref="SaveData"/> directly from file
        /// <code>
        /// private SaveData _saveData;
        /// private SerializableGuid _saveKey;
        /// private string _saveFilePath;
        ///  
        /// private void GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveFilePath, _saveKey, Vector3.zero, OnGetKey);
        /// }
        ///  
        /// private void OnGetKey(Vector3 position)
        /// {
        ///     transform.position = position;
        /// }
        /// </code>
        /// </example>
        public static void GetKey<T>(string filePath, SerializableGuid saveKey, T defaultValue, Action<T> onGetKey) => 
            GetKey(filePath, SaveSettings.Default, saveKey, defaultValue, onGetKey);
        
        /// <summary>
        /// Get value from file by SerializableGuid
        /// Note! That this method is expensive (Deserialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveSettings">Settings for loading</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="onGetKey">Action that will be invoked after load completion</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <example>Example of getting position from <see cref="SaveData"/> directly from file
        /// <code>
        /// private SaveData _saveData;
        /// private SerializableGuid _saveKey;
        /// private string _saveFilePath;
        /// private SaveSettings _saveSettings
        ///  
        /// private void GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveFilePath, _saveSettings, _saveKey, Vector3.zero, OnGetKey);
        /// }
        ///  
        /// private void OnGetKey(Vector3 position)
        /// {
        ///     transform.position = position;
        /// }
        /// </code>
        /// </example>
        public static void GetKey<T>(string filePath, SaveSettings saveSettings, SerializableGuid saveKey, T defaultValue, Action<T> onGetKey)
        {
            var tmpSaveData = new SaveData();
            
            tmpSaveData.Load(filePath, saveSettings, OnLoad);
            
            return;

            void OnLoad()
            {
                tmpSaveData.GetKey(saveKey, defaultValue, out var value);
                
                onGetKey?.Invoke(value);
            }
        }
        
        /// <summary>
        /// Get value from file by SaveKey
        /// Note! That this method is expensive (Deserialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="onGetKey">Action that will be invoked after load completion</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <example>Example of getting position from <see cref="SaveData"/> directly from file
        /// <code>
        /// private SaveData _saveData;
        /// private SaveKey _saveKey;
        /// private string _saveFilePath;
        ///  
        /// private void GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveFilePath, _saveKey, Vector3.zero, OnGetKey);
        /// }
        ///  
        /// private void OnGetKey(Vector3 position)
        /// {
        ///     transform.position = position;
        /// }
        /// </code>
        /// </example>
        public static void GetKey<T>(string filePath, SaveKey saveKey, T defaultValue, Action<T> onGetKey) => 
            GetKey(filePath, SaveSettings.Default, saveKey, defaultValue, onGetKey);
        
        /// <summary>
        /// Get value from file by SaveKey
        /// Note! That this method is expensive (Deserialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveSettings">Settings for loading</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="defaultValue">Default value that will be returned if key is not found</param>
        /// <param name="onGetKey">Action that will be invoked after load completion</param>
        /// <typeparam name="T">Type of value</typeparam>
        /// <example>Example of getting position from <see cref="SaveData"/> directly from file
        /// <code>
        /// private SaveData _saveData;
        /// private SaveKey _saveKey;
        /// private string _saveFilePath;
        /// private SaveSettings _saveSettings
        ///  
        /// private void GetPositionFromSaveData()
        /// {
        ///     _saveData.GetKey(_saveFilePath, _saveSettings, _saveKey, Vector3.zero, OnGetKey);
        /// }
        ///  
        /// private void OnGetKey(Vector3 position)
        /// {
        ///     transform.position = position;
        /// }
        /// </code>
        /// </example>
        public static void GetKey<T>(string filePath, SaveSettings saveSettings, SaveKey saveKey, T defaultValue, Action<T> onGetKey)
        {
            var tmpSaveData = new SaveData();
            
            tmpSaveData.Load(filePath, saveSettings, OnLoad);
            
            return;

            void OnLoad()
            {
                tmpSaveData.GetKey(saveKey, defaultValue, out var value);
                
                onGetKey?.Invoke(value);
            }
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
        /// Set value to Data by string
        /// Note! That this method is expensive (Deserialize and Serialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <param name="onSetKey">Callback that will be invoked after save completion</param>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private string _saveKey;
        /// private string _saveFilePath;
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveFilePath, _saveKey, transform.position, OnSetKey);
        /// }
        ///  
        /// private void OnSetKey()
        /// {
        ///     Debug.Log("[Alert] Position saved");
        /// }
        /// </code>
        /// </example>
        public void SetKey(string filePath, string saveKey, object value, Action onSetKey) => 
            SetKey(filePath, SaveSettings.Default, saveKey, value, onSetKey);

        /// <summary>
        /// Set value to Data by string
        /// Note! That this method is expensive (Deserialize and Serialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveSettings">Settings for saving</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <param name="onSetKey">Callback that will be invoked after save completion</param>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private string _saveKey;
        /// private string _saveFilePath;
        /// private SaveSettings _saveSettings
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveFilePath, _saveSettings, _saveKey, transform.position, OnSetKey);
        /// }
        ///  
        /// private void OnSetKey()
        /// {
        ///     Debug.Log("[Alert] Position saved");
        /// }
        /// </code>
        /// </example>
        public static void SetKey(string filePath, SaveSettings saveSettings, string saveKey, object value, Action onSetKey)
        {
            var tmpSaveData = new SaveData();
            
            tmpSaveData.Load(filePath, saveSettings, OnLoad);
            
            return;

            void OnLoad()
            {
                tmpSaveData.SetKey(saveKey, value);
                
                tmpSaveData.Save(filePath, saveSettings, onSetKey);
            }
        }
        
        /// <summary>
        /// Set value to Data by Guid
        /// Note! That this method is expensive (Deserialize and Serialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveKey"><see cref="Guid"/> Save key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <param name="onSetKey">Callback that will be invoked after save completion</param>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private Guid _saveKey;
        /// private string _saveFilePath;
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveFilePath, _saveKey, transform.position, OnSetKey);
        /// }
        ///  
        /// private void OnSetKey()
        /// {
        ///     Debug.Log("[Alert] Position saved");
        /// }
        /// </code>
        /// </example>
        public static void SetKey(string filePath, Guid saveKey, object value, Action onSetKey) => 
            SetKey(filePath, SaveSettings.Default, saveKey, value, onSetKey);
        
        /// <summary>
        /// Set value to Data by Guid
        /// Note! That this method is expensive (Deserialize and Serialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveSettings">Settings for saving</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <param name="onSetKey">Callback that will be invoked after save completion</param>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private Guid _saveKey;
        /// private string _saveFilePath;
        /// private SaveSettings _saveSettings
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveFilePath, _saveSettings, _saveKey, transform.position, OnSetKey);
        /// }
        ///  
        /// private void OnSetKey()
        /// {
        ///     Debug.Log("[Alert] Position saved");
        /// }
        /// </code>
        /// </example>
        public static void SetKey(string filePath, SaveSettings saveSettings, Guid saveKey, object value, Action onSetKey)
        {
            var tmpSaveData = new SaveData();
            
            tmpSaveData.Load(filePath, saveSettings, OnLoad);
            
            return;

            void OnLoad()
            {
                tmpSaveData.SetKey(saveKey, value);
                
                tmpSaveData.Save(filePath, saveSettings, onSetKey);
            }
        }
        
        /// <summary>
        /// Set value to Data by SerializableGuid
        /// Note! That this method is expensive (Deserialize and Serialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveKey"><see cref="SerializableGuid"/> Save key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <param name="onSetKey"><see cref="Action"/> Callback that will be invoked after save completion</param>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SerializableGuid _saveKey;
        /// private string _saveFilePath;
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveFilePath, _saveKey, transform.position, OnSetKey);
        /// }
        ///  
        /// private void OnSetKey()
        /// {
        ///     Debug.Log("[Alert] Position saved");
        /// }
        /// </code>
        /// </example>
        public static void SetKey(string filePath, SerializableGuid saveKey, object value, Action onSetKey) => 
            SetKey(filePath, SaveSettings.Default, saveKey, value, onSetKey);
        
        /// <summary>
        /// Set value to Data by SerializableGuid
        /// Note! That this method is expensive (Deserialize and Serialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveSettings">Settings for saving</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <param name="onSetKey">Callback that will be invoked after save completion</param>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SerializableGuid _saveKey;
        /// private string _saveFilePath;
        /// private SaveSettings _saveSettings
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveFilePath, _saveSettings, _saveKey, transform.position, OnSetKey);
        /// }
        ///  
        /// private void OnSetKey()
        /// {
        ///     Debug.Log("[Alert] Position saved");
        /// }
        /// </code>
        /// </example>
        public static void SetKey(string filePath, SaveSettings saveSettings, SerializableGuid saveKey, object value, Action onSetKey)
        {
            var tmpSaveData = new SaveData();
            
            tmpSaveData.Load(filePath, saveSettings, OnLoad);
            
            return;

            void OnLoad()
            {
                tmpSaveData.SetKey(saveKey, value);
                
                tmpSaveData.Save(filePath, saveSettings, onSetKey);
            }
        }
        
        /// <summary>
        /// Set value to Data by SaveKey
        /// Note! That this method is expensive (Deserialize and Serialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveKey"><see cref="SaveKey"/> Save key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <param name="onSetKey"><see cref="Action"/> Callback that will be invoked after save completion</param>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SaveKey _saveKey;
        /// private string _saveFilePath;
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveFilePath, _saveKey, transform.position, OnSetKey);
        /// }
        ///  
        /// private void OnSetKey()
        /// {
        ///     Debug.Log("[Alert] Position saved");
        /// }
        /// </code>
        /// </example>
        public static void SetKey(string filePath, SaveKey saveKey, object value, Action onSetKey) => 
            SetKey(filePath, SaveSettings.Default, saveKey, value, onSetKey);
        
        /// <summary>
        /// Set value to Data by SaveKey
        /// Note! That this method is expensive (Deserialize and Serialize Json in every call) and should be used only when needed
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="saveSettings">Settings for saving</param>
        /// <param name="saveKey">Key for searching in Data</param>
        /// <param name="value">Value to be set</param>
        /// <param name="onSetKey">Callback that will be invoked after save completion</param>
        /// <example>Example of setting current position to <see cref="SaveData"/>
        /// <code>
        /// private SaveData _saveData;
        /// private SaveKey _saveKey;
        /// private string _saveFilePath;
        /// private SaveSettings _saveSettings
        ///  
        /// private void SetCurrentPosition()
        /// {
        ///     _saveData.SetKey(_saveFilePath, _saveSettings, _saveKey, transform.position, OnSetKey);
        /// }
        ///  
        /// private void OnSetKey()
        /// {
        ///     Debug.Log("[Alert] Position saved");
        /// }
        /// </code>
        /// </example>
        public static void SetKey(string filePath, SaveSettings saveSettings, SaveKey saveKey, object value, Action onSetKey)
        {
            var tmpSaveData = new SaveData();
            
            tmpSaveData.Load(filePath, saveSettings, OnLoad);
            
            return;

            void OnLoad()
            {
                tmpSaveData.SetKey(saveKey, value);
                
                tmpSaveData.Save(filePath, saveSettings, onSetKey);
            }
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
        public void Save(string path, SaveSettings saveSettings) => Save(path, saveSettings, null);
        
        /// <summary>
        /// Save data to file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="onSave">Action that will be invoked after save completion</param>
        public void Save(string path, Action onSave) => Save(path, SaveSettings.Default, onSave);

        /// <summary>
        /// Save data to file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="saveSettings">Settings for saving</param>
        /// <param name="onSave">Action that will be invoked after save completion</param>
        public async void Save(string path, SaveSettings saveSettings, Action onSave)
        {
            try
            {
                try
                {
                    saveSettings ??= SaveSettings.Default;

                    var jsonString = ToJson(saveSettings);

                    if (saveSettings.UseEncryption)
                    {
                        var password = saveSettings.Encryption.Password;
                        var salt = saveSettings.Encryption.Salt;
                        var initVector = saveSettings.Encryption.InitVector;

                        jsonString = jsonString.Encrypt(password, salt, initVector);
                    }

                    // Create directory if it doesn't exist
                    var directoryPath = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);


                    await File.WriteAllTextAsync(path, jsonString);

                    // Delete excess files
                    if (saveSettings.UsedFileLimit)
                        DeleteExcessFiles(directoryPath, "*sav", saveSettings.FileLimit);

                    Debug.Log($"[SAVE-DATA] Saved to file: {path} "
                              + $"{directoryPath.ToFileLink("[Folder]")} "
                              + $"{path.ToFileLink("[File]")}");
                }
                catch (Exception e)
                {
                    Debug.Log($"[SAVE-DATA] Error on save completion read the exception bellow");
                    Debug.LogError(e);
                }

                onSave?.Invoke();
            }
            catch (Exception e)
            {
                Debug.Log($"[SAVE-DATA] Error on save completion read the exception bellow");
                Debug.LogError(e);
            }
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
        public void Load(string path, SaveSettings saveSettings) => Load(path, saveSettings, null);
        
        /// <summary>
        /// Load data from file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="onLoad">Action that will be invoked after load completion</param>
        public void Load(string path, Action onLoad) => Load(path, SaveSettings.Default, onLoad);

        /// <summary>
        /// Load data from file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="saveSettings">Settings for loading</param>
        /// <param name="onLoad">Action that will be invoked after load completion</param>
        public async void Load(string path, SaveSettings saveSettings, Action onLoad)
        {
            try
            {
                try
                {
                    saveSettings ??= SaveSettings.Default;

                    var jsonSerializerSettings = saveSettings.UseJsonCustomSettings
                        ? saveSettings.JsonCustomSettings.JsonSerializerSettings
                        : new JsonSerializerSettings();

                    var jsonText = await File.ReadAllTextAsync(path);

                    if (saveSettings.UseEncryption)
                    {
                        var password = saveSettings.Encryption.Password;
                        var salt = saveSettings.Encryption.Salt;
                        var initVector = saveSettings.Encryption.InitVector;

                        jsonText = jsonText.Decrypt(password, salt, initVector);
                    }

                    var saveData = !jsonText.IsBlank()
                        ? FromJson(jsonText, jsonSerializerSettings)
                        : new SaveData();

                    Data = saveData.Data;

                    Debug.Log($"[SAVE-DATA] Loaded from File: {path} "
                              + $"{Path.GetDirectoryName(path).ToFileLink("[Folder]")} "
                              + $"{path.ToFileLink("[File]")}");
                }
                catch (Exception e)
                {
                    Debug.Log($"[SAVE-DATA] Error on load completion read the exception bellow");
                    Debug.LogError(e);
                }

                onLoad?.Invoke();
            }
            catch (Exception e)
            {
                Debug.Log($"[SAVE-DATA] Error on load completion read the exception bellow");
                Debug.LogError(e);
            }
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