using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace fefek5.SaveDataVariable.Runtime
{
    public static class SaveVarStorage
    {
        private static readonly Dictionary<string, SaveData> _saveDatas = new();
        private static readonly Dictionary<string, HashSet<SaveKey>> _dirtyKeys = new();

        private static string GetPath(string relativePath) =>
            Path.Combine(Application.persistentDataPath, relativePath);

        /// <summary>
        /// Returns the SaveData of the file, on first access it's loaded from disk first,
        /// so nothing can be saved over the file before it was read
        /// </summary>
        public static SaveData Get(string relativePath)
        {
            if (_saveDatas.TryGetValue(relativePath, out var saveData))
                return saveData;

            saveData = new SaveData();

            var path = GetPath(relativePath);

            if (File.Exists(path))
                saveData.Load(path);

            _saveDatas[relativePath] = saveData;

            return saveData;
        }

        /// <inheritdoc cref="Get"/>
        public static async Awaitable<SaveData> GetAsync(string relativePath,
            CancellationToken cancellationToken = default)
        {
            if (_saveDatas.TryGetValue(relativePath, out var saveData))
                return saveData;

            saveData = new SaveData();

            var path = GetPath(relativePath);

            if (File.Exists(path))
                await saveData.LoadAsync(path, cancellationToken);

            // Another Get or GetAsync could finish during the await, keep the first so there's one instance per file
            if (_saveDatas.TryGetValue(relativePath, out var loadedSaveData))
                return loadedSaveData;

            _saveDatas[relativePath] = saveData;

            return saveData;
        }

        /// <summary>
        /// True when any key of the file was set and not pushed yet
        /// </summary>
        public static bool IsDirty(string relativePath) =>
            _dirtyKeys.TryGetValue(relativePath, out var keys) && keys.Count > 0;

        /// <summary>
        /// True when the key was set and not pushed yet
        /// </summary>
        public static bool IsDirty(string relativePath, SaveKey saveKey) =>
            _dirtyKeys.TryGetValue(relativePath, out var keys) && keys.Contains(saveKey);

        /// <summary>
        /// Returns the value from the SaveData of the file, or the default value when the key is missing
        /// </summary>
        public static T GetValue<T>(string relativePath, SaveKey saveKey, T defaultValue = default) =>
            Get(relativePath).GetKey(saveKey, defaultValue);

        /// <summary>
        /// Sets the value in the SaveData of the file and marks the key as dirty. Does not touch the disk
        /// </summary>
        public static void SetValue<T>(string relativePath, SaveKey saveKey, T value)
        {
            Get(relativePath).SetKey(saveKey, value);

            if (!_dirtyKeys.TryGetValue(relativePath, out var keys))
                _dirtyKeys[relativePath] = keys = new HashSet<SaveKey>();

            keys.Add(saveKey);
        }

        private static void ClearDirty(string relativePath)
        {
            if (_dirtyKeys.TryGetValue(relativePath, out var keys))
                keys.Clear();
        }

        /// <summary>
        /// Reloads the SaveData from disk, discarding unsaved changes
        /// </summary>
        public static void Pull(string relativePath)
        {
            if (!_saveDatas.TryGetValue(relativePath, out var saveData))
            {
                Get(relativePath);
                return;
            }

            var path = GetPath(relativePath);

            if (!File.Exists(path)) return;

            saveData.Load(path);
            ClearDirty(relativePath);
        }

        /// <inheritdoc cref="Pull"/>
        public static async Awaitable PullAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            if (!_saveDatas.TryGetValue(relativePath, out var saveData))
            {
                await GetAsync(relativePath, cancellationToken);
                return;
            }

            var path = GetPath(relativePath);

            if (!File.Exists(path)) return;

            await saveData.LoadAsync(path, cancellationToken);
            ClearDirty(relativePath);
        }

        public static void Push(string relativePath)
        {
            var saveData = Get(relativePath);

            ClearDirty(relativePath);
            saveData.Save(GetPath(relativePath));
        }

        public static async Awaitable PushAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            var saveData = await GetAsync(relativePath, cancellationToken);

            // SaveAsync builds the json before its first await, so a key set during the write stays dirty
            ClearDirty(relativePath);

            await saveData.SaveAsync(GetPath(relativePath), cancellationToken);
        }
    }
}