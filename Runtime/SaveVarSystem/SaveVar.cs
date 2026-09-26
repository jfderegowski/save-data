using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace fefek5.SaveDataVariable.Runtime
{
    public class DirtableSaveData
    {
        #region Properties

        /// <summary>Deep copy of the file, only replaced by a load or a save</summary>
        public SaveData Origin { get; private set; } = new();

        /// <summary>Current values: the file's keys plus every change made since, never cleared by a save</summary>
        public SaveData Target { get; } = new();

        public bool IsDirty => Target.Data.Keys.Any(IsKeyDirty);

        #endregion

        // There is one DirtableSaveData per file, so this queues its async saves and loads:
        // each one starts from the Origin the previous one left and never touches the file at the same time
        private readonly SemaphoreSlim _fileQueue = new(1, 1);

        public DirtableSaveData(string path) => Pull(path);

        private DirtableSaveData() { }

        public static async Awaitable<DirtableSaveData> CreateAsync(string path,
            CancellationToken cancellationToken = default)
        {
            var saveData = new DirtableSaveData();

            await saveData.PullAsync(path, cancellationToken);

            return saveData;
        }

        public bool IsKeyDirty(SaveKey saveKey) => KeyToJson(Origin, saveKey) != KeyToJson(Target, saveKey);

        #region Getters and Setters

        public T GetKey<T>(SaveKey saveKey, T defaultValue) => Target.GetKey(saveKey, defaultValue);

        public void SetKey(SaveKey saveKey, object value) => Target.SetKey(saveKey, value);

        #endregion

        #region Push and Pull

        public void Push(string path, SaveKey saveKey)
        {
            if (!IsKeyDirty(saveKey)) return;

            var saved = new SaveData(Origin);
            CopyKey(Target, saved, saveKey);

            saved.Save(path);

            Origin = DeepCopy(saved);
        }

        public void Push(string path)
        {
            var saved = new SaveData(Target);

            saved.Save(path);

            Origin = DeepCopy(saved);
        }

        public async Awaitable PushAsync(string path, SaveKey saveKey, CancellationToken cancellationToken = default)
        {
            await _fileQueue.WaitAsync(cancellationToken);

            try
            {
                // Checked in the queue, a save queued before this one could have written the value already
                if (!IsKeyDirty(saveKey)) return;

                var saved = new SaveData(Origin);
                CopyKey(Target, saved, saveKey);

                // Copied before the await, so a change made meanwhile still shows as dirty
                var origin = DeepCopy(saved);

                await saved.SaveAsync(path, cancellationToken);

                Origin = origin;
            }
            finally
            {
                _fileQueue.Release();
            }
        }

        public async Awaitable PushAsync(string path, CancellationToken cancellationToken = default)
        {
            await _fileQueue.WaitAsync(cancellationToken);

            try
            {
                var saved = new SaveData(Target);
                var origin = DeepCopy(saved);

                await saved.SaveAsync(path, cancellationToken);

                Origin = origin;
            }
            finally
            {
                _fileQueue.Release();
            }
        }

        public void Pull(string path, SaveKey saveKey)
        {
            ReloadOrigin(path);

            CopyKey(Origin, Target, saveKey);
        }

        public void Pull(string path)
        {
            ReloadOrigin(path);

            ResetTarget();
        }

        public async Awaitable PullAsync(string path, SaveKey saveKey, CancellationToken cancellationToken = default)
        {
            await _fileQueue.WaitAsync(cancellationToken);

            try
            {
                await ReloadOriginAsync(path, cancellationToken);

                CopyKey(Origin, Target, saveKey);
            }
            finally
            {
                _fileQueue.Release();
            }
        }

        public async Awaitable PullAsync(string path, CancellationToken cancellationToken = default)
        {
            await _fileQueue.WaitAsync(cancellationToken);

            try
            {
                await ReloadOriginAsync(path, cancellationToken);

                ResetTarget();
            }
            finally
            {
                _fileQueue.Release();
            }
        }

        private void ReloadOrigin(string path)
        {
            var loaded = new SaveData();

            if (File.Exists(path))
                loaded.Load(path);

            Origin = loaded;
        }

        private async Awaitable ReloadOriginAsync(string path, CancellationToken cancellationToken)
        {
            var loaded = new SaveData();

            if (File.Exists(path))
                await loaded.LoadAsync(path, cancellationToken);

            Origin = loaded;
        }

        private void ResetTarget()
        {
            Target.Data.Clear();

            foreach (var (saveKey, value) in Origin.Data)
                Target.SetKey(saveKey, value);
        }

        #endregion

        #region Helpers

        private static void CopyKey(SaveData from, SaveData to, SaveKey saveKey)
        {
            if (from.Data.TryGetValue(saveKey, out var value))
                to.SetKey(saveKey, value);
            else
                to.RemoveKey(saveKey);
        }

        // Compared as json, since loaded values are long/double/JToken while set ones keep their own type
        private static string KeyToJson(SaveData saveData, SaveKey saveKey) =>
            saveData.Data.TryGetValue(saveKey, out var value) ? new SaveData().SetKey(saveKey, value).ToJson() : null;

        // Origin must not share instances with Target, or mutating a value in place would never show as dirty
        private static SaveData DeepCopy(SaveData saveData) => saveData.FromJson(saveData.ToJson());

        #endregion
    }

    [Serializable]
    public abstract class SaveVar
    {
        internal static Dictionary<string, DirtableSaveData> SaveDatas = new();

        #region Properties

        public bool IsDirty => SaveDatas.TryGetValue(Path, out var saveData) && saveData.IsKeyDirty(SaveKey);

        public string Path => GetPath(RelativePath);

        #endregion

        #region Inspector Fields

        [field: SerializeField] public SaveKey SaveKey { get; private set; }

        [field: SerializeField] public string RelativePath { get; private set; }

        #endregion

        #region Constructors

        protected SaveVar() : this("SaveVars.json", SaveKey.RandomKey) { }

        protected SaveVar(string relativePath, SaveKey saveKey)
        {
            RelativePath = relativePath;
            SaveKey = saveKey;
        }

        #endregion

        public DirtableSaveData GetSaveData() => GetSaveData(Path);

        public Awaitable<DirtableSaveData> GetSaveDataAsync(CancellationToken cancellationToken = default) =>
            GetSaveDataAsync(Path, cancellationToken);

        private static string GetPath(string relativePath) =>
            System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.persistentDataPath, relativePath));

        #region Push and Pull

        public abstract void Push();

        public abstract Awaitable PushAsync(CancellationToken cancellationToken = default);

        public abstract void Pull();

        public abstract Awaitable PullAsync(CancellationToken cancellationToken = default);

        #endregion

        #region Save Datas

        public static DirtableSaveData GetSaveData(string path)
        {
            if (SaveDatas.TryGetValue(path, out var existingSaveData))
                return existingSaveData;

            var newSaveData = new DirtableSaveData(path);

            SaveDatas.Add(path, newSaveData);

            return newSaveData;
        }

        public static async Awaitable<DirtableSaveData> GetSaveDataAsync(string path,
            CancellationToken cancellationToken = default)
        {
            if (SaveDatas.TryGetValue(path, out var existingSaveData))
                return existingSaveData;

            var newSaveData = await DirtableSaveData.CreateAsync(path, cancellationToken);

            // Another call could have loaded the same file while this one was awaited
            if (SaveDatas.TryGetValue(path, out existingSaveData))
                return existingSaveData;

            SaveDatas.Add(path, newSaveData);

            return newSaveData;
        }

        public static bool IsSaveDataDirty(string path) =>
            SaveDatas.TryGetValue(path, out var saveData) && saveData.IsDirty;

        public static void PushSaveData(string path) => GetSaveData(path).Push(path);

        public static async Awaitable PushSaveDataAsync(string path, CancellationToken cancellationToken = default) =>
            await (await GetSaveDataAsync(path, cancellationToken)).PushAsync(path, cancellationToken);

        public static void PullSaveData(string path) => GetSaveData(path).Pull(path);

        public static async Awaitable PullSaveDataAsync(string path, CancellationToken cancellationToken = default) =>
            await (await GetSaveDataAsync(path, cancellationToken)).PullAsync(path, cancellationToken);

        #endregion
    }

    [Serializable]
    public class SaveVar<T> : SaveVar
    {
        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        #region Inspector Fields

        [field: SerializeField] public T DefaultValue { get; private set; }

        #endregion

        #region Constructors

        public SaveVar() : this("SaveVar.json", SaveKey.RandomKey) { }

        public SaveVar(string relativePath, SaveKey saveKey, T defaultValue = default)
            : base(relativePath, saveKey)
        {
            DefaultValue = defaultValue;
        }

        #endregion

        #region Getters and Setters

        public T GetValue() => GetSaveData().GetKey(SaveKey, DefaultValue);

        public async Awaitable<T> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var saveData = await GetSaveDataAsync(cancellationToken);

            return saveData.GetKey(SaveKey, DefaultValue);
        }

        public void SetValue(T value)
        {
            var saveData = GetSaveData();

            saveData.SetKey(SaveKey, value);
            saveData.Push(Path, SaveKey);
        }

        public async Awaitable SetValueAsync(T value, CancellationToken cancellationToken = default)
        {
            var saveData = await GetSaveDataAsync(cancellationToken);

            saveData.SetKey(SaveKey, value);
            await saveData.PushAsync(Path, SaveKey, cancellationToken);
        }

        public void SetValueWithoutNotifying(T value) => GetSaveData().SetKey(SaveKey, value);

        public void ResetToDefault() => SetValue(DefaultValue);

        #endregion

        #region Push and Pull

        public override void Push() => GetSaveData().Push(Path, SaveKey);

        public override async Awaitable PushAsync(CancellationToken cancellationToken = default) =>
            await (await GetSaveDataAsync(cancellationToken)).PushAsync(Path, SaveKey, cancellationToken);

        public override void Pull() => GetSaveData().Pull(Path, SaveKey);

        public override async Awaitable PullAsync(CancellationToken cancellationToken = default) =>
            await (await GetSaveDataAsync(cancellationToken)).PullAsync(Path, SaveKey, cancellationToken);

        #endregion

        public static implicit operator T(SaveVar<T> saveVar) => saveVar is not null ? saveVar.Value : default;

        public override string ToString() => Value?.ToString() ?? "null";
    }
}