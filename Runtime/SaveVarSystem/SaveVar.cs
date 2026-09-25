using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace fefek5.SaveDataVariable.Runtime
{
    public class DirtableSaveData
    {
        #region Properties

        /// <summary>Snapshot of the file, only replaced by a load or a save</summary>
        public SaveData Origin { get; private set; } = new();
        public SaveData Target { get; } = new();

        public bool IsDirty => Target.Data.Count > 0;

        #endregion

        public DirtableSaveData(string path) => ReloadOrigin(path);

        private DirtableSaveData() { }

        public static async Awaitable<DirtableSaveData> CreateAsync(string path,
            CancellationToken cancellationToken = default)
        {
            var saveData = new DirtableSaveData();

            await saveData.ReloadOriginAsync(path, cancellationToken);

            return saveData;
        }

        public bool IsKeyDirty(SaveKey saveKey) => Target.IsKeyExist(saveKey);

        #region Getters and Setters

        public T GetKey<T>(SaveKey saveKey, T defaultValue) =>
            Target.TryGetKey(saveKey, out T dirty) ? dirty : Origin.GetKey(saveKey, defaultValue);

        public void SetKey(SaveKey saveKey, object value) => Target.SetKey(saveKey, value);

        #endregion

        #region Push and Pull

        public void Push(string path, SaveKey saveKey)
        {
            if (!IsKeyDirty(saveKey))
                throw new InvalidOperationException("Key is not dirty!");

            var saved = new SaveData(Origin).SetKey(saveKey, Target[saveKey]);
            saved.Save(path);

            Origin = saved;
            Target.RemoveKey(saveKey);
        }

        public void Push(string path)
        {
            var saved = new SaveData(Origin);

            foreach (var (saveKey, value) in Target.Data)
                saved.SetKey(saveKey, value);

            saved.Save(path);

            Origin = saved;
            Target.Data.Clear();
        }

        public async Awaitable PushAsync(string path, SaveKey saveKey, CancellationToken cancellationToken = default)
        {
            if (!IsKeyDirty(saveKey))
                throw new InvalidOperationException("Key is not dirty!");

            var value = Target[saveKey];
            var saved = new SaveData(Origin).SetKey(saveKey, value);
            await saved.SaveAsync(path, cancellationToken);

            Origin = saved;
            RemoveIfUnchanged(saveKey, value);
        }

        public async Awaitable PushAsync(string path, CancellationToken cancellationToken = default)
        {
            var pushed = new Dictionary<SaveKey, object>(Target.Data);
            var saved = new SaveData(Origin);

            foreach (var (saveKey, value) in pushed)
                saved.SetKey(saveKey, value);

            await saved.SaveAsync(path, cancellationToken);

            Origin = saved;

            foreach (var (saveKey, value) in pushed)
                RemoveIfUnchanged(saveKey, value);
        }

        public void Pull(string path, SaveKey saveKey)
        {
            if (!IsKeyDirty(saveKey))
                throw new InvalidOperationException("Key is not dirty!");

            ReloadOrigin(path);

            Target.RemoveKey(saveKey);
        }

        public void Pull(string path)
        {
            ReloadOrigin(path);

            Target.Data.Clear();
        }

        public async Awaitable PullAsync(string path, SaveKey saveKey, CancellationToken cancellationToken = default)
        {
            if (!IsKeyDirty(saveKey))
                throw new InvalidOperationException("Key is not dirty!");

            await ReloadOriginAsync(path, cancellationToken);

            Target.RemoveKey(saveKey);
        }

        public async Awaitable PullAsync(string path, CancellationToken cancellationToken = default)
        {
            await ReloadOriginAsync(path, cancellationToken);

            Target.Data.Clear();
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

        // A SetKey made while the save was awaited stays dirty
        private void RemoveIfUnchanged(SaveKey saveKey, object pushedValue)
        {
            if (Target.Data.TryGetValue(saveKey, out var current) && Equals(current, pushedValue))
                Target.RemoveKey(saveKey);
        }

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
