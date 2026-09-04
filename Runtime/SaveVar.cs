using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using fefek5.SaveDataVariable.Runtime.Settings;
using UnityEngine;

namespace fefek5.SaveDataVariable.Runtime
{
    /// <summary>
    /// Keeps one <see cref="SaveData"/> per save file and moves it to and from the disk.
    /// <para>
    /// Nothing is ever written automatically. Call <see cref="PushFileAsync(string)"/> or
    /// <see cref="PushAllAsync()"/> yourself. The only exception is the shutdown flush wired up in
    /// <see cref="InstallLifecycleHooks"/>, which is a safety net against losing unsaved changes.
    /// </para>
    /// </summary>
    public static class SaveVarStorage
    {
        #region Events

        /// <summary>
        /// Invoked with the relative path after a file was written to the disk.
        /// </summary>
        public static event Action<string> onFilePushed;

        /// <summary>
        /// Invoked with the relative path after a file was read from the disk.
        /// </summary>
        public static event Action<string> onFilePulled;

        /// <summary>
        /// Invoked with the relative path and the exception when a read or write failed.
        /// </summary>
        public static event Action<string, Exception> onFileSyncFailed;

        #endregion

        #region Fields

        private static readonly Dictionary<string, SaveData> _saveDatas = new();
        private static readonly HashSet<string> _loadedPaths = new();
        private static readonly Dictionary<string, List<WeakReference<SaveVar>>> _registry = new();

        #endregion

        #region Paths

        /// <summary>
        /// Turn a relative save path into a full path inside <see cref="Application.persistentDataPath"/>.
        /// </summary>
        /// <param name="relativePath">Path relative to the persistent data path</param>
        /// <returns>The full path of the save file</returns>
        public static string GetFullPath(string relativePath) =>
            Path.Combine(Application.persistentDataPath, relativePath);

        #endregion

        #region SaveData Access

        /// <summary>
        /// The in memory <see cref="SaveData"/> of a file. Never touches the disk, so it is safe to call
        /// every frame. An empty <see cref="SaveData"/> is created for paths that were not loaded yet.
        /// </summary>
        /// <param name="relativePath">Path relative to the persistent data path</param>
        /// <returns>The <see cref="SaveData"/> held for that path</returns>
        public static SaveData GetSaveData(string relativePath)
        {
            if (_saveDatas.TryGetValue(relativePath, out var saveData))
                return saveData;

            saveData = new SaveData();
            _saveDatas[relativePath] = saveData;

            return saveData;
        }

        /// <summary>
        /// True when the file was read from the disk at least once. Until then every
        /// <see cref="SaveVar{T}"/> pointing at it reports its <see cref="SaveVar{T}.DefaultValue"/>.
        /// </summary>
        /// <param name="relativePath">Path relative to the persistent data path</param>
        /// <returns>True when the file content is in memory</returns>
        public static bool IsLoaded(string relativePath) => _loadedPaths.Contains(relativePath);

        /// <summary>
        /// Relative paths of every file that holds at least one changed and unsaved variable.
        /// </summary>
        /// <returns>The paths that <see cref="PushAllAsync()"/> would write</returns>
        public static IEnumerable<string> GetDirtyPaths()
        {
            foreach (var relativePath in _registry.Keys)
            {
                foreach (var saveVar in GetSaveVars(relativePath))
                {
                    if (saveVar.IsSync) continue;

                    yield return relativePath;

                    break;
                }
            }
        }

        #endregion

        #region Registration

        /// <summary>
        /// Register a variable so that file wide operations can reach it. Called lazily on first use of a
        /// <see cref="SaveVar{T}"/>, because Unity does not guarantee a constructor call for inspector fields.
        /// </summary>
        /// <param name="saveVar">The variable to register</param>
        internal static void Register(SaveVar saveVar)
        {
            if (saveVar == null) return;

            var relativePath = saveVar.RelativePath;

            if (!_registry.TryGetValue(relativePath, out var references))
            {
                references = new List<WeakReference<SaveVar>>();
                _registry[relativePath] = references;
            }

            references.Add(new WeakReference<SaveVar>(saveVar));
        }

        /// <summary>
        /// Every registered variable of a file that is still alive. Dead weak references are dropped on the way.
        /// </summary>
        /// <param name="relativePath">Path relative to the persistent data path</param>
        /// <returns>The living variables that belong to that file</returns>
        private static List<SaveVar> GetSaveVars(string relativePath)
        {
            var saveVars = new List<SaveVar>();

            if (!_registry.TryGetValue(relativePath, out var references))
                return saveVars;

            for (var i = references.Count - 1; i >= 0; i--)
            {
                if (references[i].TryGetTarget(out var saveVar))
                    saveVars.Add(saveVar);
                else
                    references.RemoveAt(i);
            }

            return saveVars;
        }

        #endregion

        #region Pull

        /// <summary>
        /// Read the given files from the disk before anything reads their variables.
        /// Call it once on boot, on a loading screen.
        /// </summary>
        /// <param name="relativePaths">Paths relative to the persistent data path</param>
        public static Task PreloadAsync(params string[] relativePaths) =>
            PreloadAsync(relativePaths, SaveSettings.Default, CancellationToken.None);

        /// <summary>
        /// Read the given files from the disk before anything reads their variables.
        /// Call it once on boot, on a loading screen.
        /// </summary>
        /// <param name="relativePaths">Paths relative to the persistent data path</param>
        /// <param name="cancellationToken">Token that cancels the remaining reads</param>
        public static Task PreloadAsync(IEnumerable<string> relativePaths, CancellationToken cancellationToken) =>
            PreloadAsync(relativePaths, SaveSettings.Default, cancellationToken);

        /// <summary>
        /// Read the given files from the disk before anything reads their variables.
        /// Call it once on boot, on a loading screen.
        /// </summary>
        /// <param name="relativePaths">Paths relative to the persistent data path</param>
        /// <param name="saveSettings">Settings used for reading</param>
        /// <param name="cancellationToken">Token that cancels the remaining reads</param>
        public static async Task PreloadAsync(IEnumerable<string> relativePaths, SaveSettings saveSettings,
            CancellationToken cancellationToken)
        {
            if (relativePaths == null) return;

            foreach (var relativePath in relativePaths)
                await PullFileAsync(relativePath, saveSettings, cancellationToken);
        }

        /// <summary>
        /// Read one file from the disk and refresh every registered variable that belongs to it.
        /// </summary>
        /// <param name="relativePath">Path relative to the persistent data path</param>
        public static Task PullFileAsync(string relativePath) =>
            PullFileAsync(relativePath, SaveSettings.Default, CancellationToken.None);

        /// <summary>
        /// Read one file from the disk and refresh every registered variable that belongs to it.
        /// </summary>
        /// <param name="relativePath">Path relative to the persistent data path</param>
        /// <param name="cancellationToken">Token that cancels the read</param>
        public static Task PullFileAsync(string relativePath, CancellationToken cancellationToken) =>
            PullFileAsync(relativePath, SaveSettings.Default, cancellationToken);

        /// <summary>
        /// Read one file from the disk and refresh every registered variable that belongs to it.
        /// A missing file is not an error, it just leaves the variables on their default values.
        /// </summary>
        /// <param name="relativePath">Path relative to the persistent data path</param>
        /// <param name="saveSettings">Settings used for reading</param>
        /// <param name="cancellationToken">Token that cancels the read</param>
        public static async Task PullFileAsync(string relativePath, SaveSettings saveSettings,
            CancellationToken cancellationToken)
        {
            try
            {
                var fullPath = GetFullPath(relativePath);

                if (File.Exists(fullPath))
                    await GetSaveData(relativePath).LoadAsync(fullPath, saveSettings, cancellationToken);

                _loadedPaths.Add(relativePath);

                foreach (var saveVar in GetSaveVars(relativePath))
                    saveVar.ReadFromSaveData();

                onFilePulled?.Invoke(relativePath);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                onFileSyncFailed?.Invoke(relativePath, e);

                throw;
            }
        }

        #endregion

        #region Push

        /// <summary>
        /// Write one file to the disk with the current value of every registered variable that belongs to it.
        /// </summary>
        /// <param name="relativePath">Path relative to the persistent data path</param>
        public static Task PushFileAsync(string relativePath) =>
            PushFileAsync(relativePath, SaveSettings.Default, CancellationToken.None);

        /// <summary>
        /// Write one file to the disk with the current value of every registered variable that belongs to it.
        /// </summary>
        /// <param name="relativePath">Path relative to the persistent data path</param>
        /// <param name="cancellationToken">Token that cancels the write</param>
        public static Task PushFileAsync(string relativePath, CancellationToken cancellationToken) =>
            PushFileAsync(relativePath, SaveSettings.Default, cancellationToken);

        /// <summary>
        /// Write one file to the disk with the current value of every registered variable that belongs to it.
        /// Every variable of that file becomes synced, because the whole file is written at once.
        /// </summary>
        /// <param name="relativePath">Path relative to the persistent data path</param>
        /// <param name="saveSettings">Settings used for writing</param>
        /// <param name="cancellationToken">Token that cancels the write</param>
        public static async Task PushFileAsync(string relativePath, SaveSettings saveSettings,
            CancellationToken cancellationToken)
        {
            try
            {
                var saveVars = GetSaveVars(relativePath);

                foreach (var saveVar in saveVars)
                    saveVar.WriteToSaveData();

                await GetSaveData(relativePath)
                    .SaveAsync(GetFullPath(relativePath), saveSettings, cancellationToken);

                foreach (var saveVar in saveVars)
                    saveVar.MarkSynced();

                _loadedPaths.Add(relativePath);

                onFilePushed?.Invoke(relativePath);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                onFileSyncFailed?.Invoke(relativePath, e);

                throw;
            }
        }

        /// <summary>
        /// Write every file that holds unsaved changes. Files without changes are skipped.
        /// </summary>
        public static Task PushAllAsync() => PushAllAsync(SaveSettings.Default, CancellationToken.None);

        /// <summary>
        /// Write every file that holds unsaved changes. Files without changes are skipped.
        /// </summary>
        /// <param name="cancellationToken">Token that cancels the remaining writes</param>
        public static Task PushAllAsync(CancellationToken cancellationToken) =>
            PushAllAsync(SaveSettings.Default, cancellationToken);

        /// <summary>
        /// Write every file that holds unsaved changes. Files without changes are skipped.
        /// </summary>
        /// <param name="saveSettings">Settings used for writing</param>
        /// <param name="cancellationToken">Token that cancels the remaining writes</param>
        public static async Task PushAllAsync(SaveSettings saveSettings, CancellationToken cancellationToken)
        {
            foreach (var relativePath in new List<string>(GetDirtyPaths()))
                await PushFileAsync(relativePath, saveSettings, cancellationToken);
        }

        /// <summary>
        /// Blocking write of every file that holds unsaved changes.
        /// <para>
        /// Used by the quit and pause flush, where the player loop will not wait for a <see cref="Task"/>.
        /// Prefer <see cref="PushAllAsync()"/> everywhere else.
        /// </para>
        /// </summary>
        public static void PushAllImmediate() => PushAllImmediate(SaveSettings.Default);

        /// <summary>
        /// Blocking write of every file that holds unsaved changes.
        /// <para>
        /// Used by the quit and pause flush, where the player loop will not wait for a <see cref="Task"/>.
        /// Prefer <see cref="PushAllAsync()"/> everywhere else.
        /// </para>
        /// </summary>
        /// <param name="saveSettings">Settings used for writing</param>
        public static void PushAllImmediate(SaveSettings saveSettings)
        {
            foreach (var relativePath in new List<string>(GetDirtyPaths()))
            {
                // This runs from a shutdown event, so one unwritable file must not stop the rest.
                try
                {
                    var saveVars = GetSaveVars(relativePath);

                    foreach (var saveVar in saveVars)
                        saveVar.WriteToSaveData();

                    GetSaveData(relativePath).Save(GetFullPath(relativePath), saveSettings);

                    foreach (var saveVar in saveVars)
                        saveVar.MarkSynced();

                    _loadedPaths.Add(relativePath);

                    onFilePushed?.Invoke(relativePath);
                }
                catch (Exception e)
                {
                    onFileSyncFailed?.Invoke(relativePath, e);

                    Debug.LogError(e);
                }
            }
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Drop every cached file and registration. Static state survives a play mode session when
        /// domain reload is disabled, so this runs on every play mode start.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            _saveDatas.Clear();
            _loadedPaths.Clear();
            _registry.Clear();

            onFilePushed = null;
            onFilePulled = null;
            onFileSyncFailed = null;
        }

        /// <summary>
        /// Subscribe the safety net that flushes unsaved variables before the process goes away.
        /// <para>
        /// Everything here is a static <see cref="Application"/> event, so no scene object is needed.
        /// Handlers are detached first, because subscriptions survive a play mode session when domain
        /// reload is disabled and would otherwise pile up.
        /// </para>
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallLifecycleHooks()
        {
            Application.quitting -= FlushOnQuit;
            Application.quitting += FlushOnQuit;

#if UNITY_ANDROID || UNITY_IOS
            // Mobile can kill the process without ever raising quitting, so losing focus is the last
            // reliable moment to write. On desktop this would fire on every alt-tab, hence the guard.
            Application.focusChanged -= FlushOnFocusLost;
            Application.focusChanged += FlushOnFocusLost;

            Application.lowMemory -= FlushOnLowMemory;
            Application.lowMemory += FlushOnLowMemory;
#endif
        }

        private static void FlushOnQuit() => PushAllImmediate();

#if UNITY_ANDROID || UNITY_IOS
        private static void FlushOnFocusLost(bool hasFocus)
        {
            if (!hasFocus)
                PushAllImmediate();
        }

        private static void FlushOnLowMemory() => PushAllImmediate();
#endif

        #endregion
    }

    /// <summary>
    /// Non generic base of a <see cref="SaveVar{T}"/>. It holds everything that does not depend on the value
    /// type, so <see cref="SaveVarStorage"/> can move values between a variable and its <see cref="SaveData"/>
    /// without knowing that type.
    /// <para>
    /// Derive from <see cref="SaveVar{T}"/> rather than from this class. The reason to reach for this one is
    /// to hold or pass around variables of mixed value types.
    /// </para>
    /// </summary>
    [Serializable]
    public abstract class SaveVar
    {
        #region Events

        /// <summary>
        /// Invoked when <see cref="IsSync"/> flips. False right after a change, true right after a push or pull.
        /// </summary>
        public event Action<bool> onIsSyncChanged;

        /// <summary>
        /// Invoked when a push or a pull failed. The exception is rethrown to the caller as well.
        /// </summary>
        public event Action<Exception> onSyncFailed;

        #endregion

        #region Properties

        /// <summary>
        /// False when the value was changed but not written to the file yet.
        /// </summary>
        public bool IsSync => !_isDirty;

        /// <summary>
        /// True when the save file was read from the disk. While false, the variable reports its default
        /// value rather than whatever the file holds.
        /// </summary>
        public bool IsLoaded => IsConfigured && SaveVarStorage.IsLoaded(RelativePath);

        /// <summary>
        /// True when the variable points at a file. A variable left empty in the inspector still works as a
        /// plain value holder, it just never reaches the disk.
        /// </summary>
        public bool IsConfigured => !string.IsNullOrEmpty(RelativePath);

        public string StringValue => GetStringValue();
        
        #endregion
        
        #region Inspector Fields

        /// <summary>
        /// Key under which the value is stored inside the save file.
        /// </summary>
        [field: SerializeField] public SaveKey SaveKey { get; private set; }

        /// <summary>
        /// Path of the save file, relative to <see cref="Application.persistentDataPath"/>.
        /// </summary>
        [field: SerializeField] public string RelativePath { get; private set; }

        #endregion

        #region Runtime State

        // Never serialized. The inspector holds the setup, not the runtime state.
        [NonSerialized] private bool _isDirty;
        [NonSerialized] private bool _isRegistered;

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor used by Unity serialization.
        /// </summary>
        protected SaveVar()
        {
            SaveKey = SaveKey.RandomKey;
            RelativePath = "SaveVars.json";
        }

        /// <summary>
        /// Create a variable from code.
        /// </summary>
        /// <param name="relativePath">Path of the save file, relative to the persistent data path</param>
        /// <param name="saveKey">Key under which the value is stored inside the file</param>
        protected SaveVar(string relativePath, SaveKey saveKey)
        {
            RelativePath = relativePath;
            SaveKey = saveKey;
        }

        #endregion

        #region Getters and Setters

        public virtual string GetStringValue() => ToString();

        #endregion
        
        #region SaveData

        /// <summary>
        /// Copy the in memory value into the <see cref="SaveData"/> of its file. Does not touch the disk.
        /// </summary>
        public abstract void WriteToSaveData();

        /// <summary>
        /// Copy the value from the <see cref="SaveData"/> of its file back into the variable. Does not touch the disk.
        /// </summary>
        public abstract void ReadFromSaveData();

        /// <summary>
        /// Mark the variable as written to the file.
        /// </summary>
        public virtual void MarkSynced() => SetIsSync(true);

        #endregion

        #region Push

        /// <summary>
        /// Write the value to its save file.
        /// </summary>
        public Task PushAsync() => PushAsync(SaveSettings.Default, CancellationToken.None);

        /// <summary>
        /// Write the value to its save file.
        /// </summary>
        /// <param name="cancellationToken">Token that cancels the write</param>
        public Task PushAsync(CancellationToken cancellationToken) =>
            PushAsync(SaveSettings.Default, cancellationToken);

        /// <summary>
        /// Write the value to its save file. The whole file is written, so every other variable of that
        /// file becomes synced too.
        /// <para>
        /// Override this to sync somewhere else first and call <c>base.PushAsync</c> to fall through to the file.
        /// </para>
        /// </summary>
        /// <param name="saveSettings">Settings used for writing</param>
        /// <param name="cancellationToken">Token that cancels the write</param>
        public virtual async Task PushAsync(SaveSettings saveSettings, CancellationToken cancellationToken)
        {
            ThrowIfNotConfigured();
            EnsureRegistered();

            try
            {
                await SaveVarStorage.PushFileAsync(RelativePath, saveSettings, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                onSyncFailed?.Invoke(e);

                throw;
            }

            RaisePushed();
        }

        #endregion

        #region Pull

        /// <summary>
        /// Read the value back from its save file.
        /// </summary>
        public Task PullAsync() => PullAsync(SaveSettings.Default, CancellationToken.None);

        /// <summary>
        /// Read the value back from its save file.
        /// </summary>
        /// <param name="cancellationToken">Token that cancels the read</param>
        public Task PullAsync(CancellationToken cancellationToken) =>
            PullAsync(SaveSettings.Default, cancellationToken);

        /// <summary>
        /// Read the value back from its save file, overwriting whatever is in memory. The whole file is read,
        /// so every other variable of that file is refreshed too.
        /// <para>
        /// Override this to read from somewhere else first and call <c>base.PullAsync</c> to fall through to the file.
        /// </para>
        /// </summary>
        /// <param name="saveSettings">Settings used for reading</param>
        /// <param name="cancellationToken">Token that cancels the read</param>
        public virtual async Task PullAsync(SaveSettings saveSettings, CancellationToken cancellationToken)
        {
            ThrowIfNotConfigured();
            EnsureRegistered();

            try
            {
                await SaveVarStorage.PullFileAsync(RelativePath, saveSettings, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                onSyncFailed?.Invoke(e);

                throw;
            }

            RaisePulled();
        }

        #endregion

        #region Internals

        /// <summary>
        /// Raise the typed pushed event of the derived class, once the file was written.
        /// </summary>
        protected abstract void RaisePushed();

        /// <summary>
        /// Raise the typed pulled event of the derived class, once the file was read.
        /// </summary>
        protected abstract void RaisePulled();

        /// <summary>
        /// Set the sync flag and raise <see cref="onIsSyncChanged"/> when it actually changed.
        /// </summary>
        /// <param name="isSync">True when the value matches the file</param>
        protected void SetIsSync(bool isSync)
        {
            if (IsSync == isSync) return;

            _isDirty = !isSync;

            onIsSyncChanged?.Invoke(isSync);
        }

        /// <summary>
        /// Register on first use. Unity does not guarantee a constructor call for a field deserialized from
        /// the inspector, so registration cannot happen in the constructor.
        /// </summary>
        protected void EnsureRegistered()
        {
            if (_isRegistered || !IsConfigured) return;

            SaveVarStorage.Register(this);

            _isRegistered = true;
        }

        /// <summary>
        /// Throw when the variable cannot reach a file. Reading a value is allowed without a path,
        /// syncing is not.
        /// </summary>
        protected void ThrowIfNotConfigured()
        {
            if (IsConfigured) return;

            throw new InvalidOperationException(
                $"{nameof(SaveVar)} with key '{SaveKey}' has no {nameof(RelativePath)} set.");
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion
    }

    /// <summary>
    /// A variable that lives in memory and is synchronized with a save file on demand.
    /// <para>
    /// Reading and writing <see cref="Value"/> never touches the disk. Setting it only marks the variable
    /// as not synced. The disk is read by <see cref="SaveVar.PullAsync()"/> and written by
    /// <see cref="SaveVar.PushAsync()"/>, both of which are awaitable and both of which are virtual, so a
    /// derived class can sync somewhere else first, for example a Steam stat, before falling through to the file.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// // boot
    /// await SaveVar2Storage.PreloadAsync("player.sav");
    ///
    /// // gameplay, no disk access
    /// _hp.Value -= 10;        // IsSync == false
    ///
    /// // checkpoint
    /// await _hp.PushAsync();  // IsSync == true
    /// </code>
    /// </example>
    /// <typeparam name="T">Type of the stored value</typeparam>
    [Serializable]
    public class SaveVar<T> : SaveVar, IEquatable<SaveVar<T>>, IEquatable<T>
    {
        #region Events

        /// <summary>
        /// Invoked with the old and the new value whenever the value actually changes,
        /// either through <see cref="Value"/> or through a pull.
        /// </summary>
        public event Action<T, T> onValueChanged;

        /// <summary>
        /// Invoked with the pushed value after the file was written.
        /// </summary>
        public event Action<T> onPushed;

        /// <summary>
        /// Invoked with the loaded value after the file was read.
        /// </summary>
        public event Action<T> onPulled;

        #endregion

        #region Inspector Fields

        /// <summary>
        /// Value reported before the file was read, and whenever the key is missing from the file.
        /// </summary>
        [field: SerializeField] public T DefaultValue { get; private set; }

        #endregion

        #region Runtime State

        // Never serialized. The inspector holds the default, not the runtime value.
        [NonSerialized] private T _value;
        [NonSerialized] private bool _hasValue;

        #endregion

        #region Properties

        /// <summary>
        /// The current value. Reading and writing stays in memory, setting it marks the variable as not synced.
        /// </summary>
        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor used by Unity serialization.
        /// </summary>
        public SaveVar() { }

        /// <summary>
        /// Create a variable from code.
        /// </summary>
        /// <param name="relativePath">Path of the save file, relative to the persistent data path</param>
        /// <param name="saveKey">Key under which the value is stored inside the file</param>
        /// <param name="defaultValue">Value reported while the file has no entry for the key</param>
        public SaveVar(string relativePath, SaveKey saveKey, T defaultValue = default)
            : base(relativePath, saveKey)
        {
            DefaultValue = defaultValue;
        }

        #endregion

        #region Getters and Setters

        /// <summary>
        /// Read the value from memory. On the very first call it is taken from the loaded
        /// <see cref="SaveData"/>, or from <see cref="DefaultValue"/> when the file was not read yet.
        /// </summary>
        /// <returns>The current value</returns>
        public virtual T GetValue()
        {
            EnsureRegistered();

            if (_hasValue) return _value;

            // An unconfigured variable stays on its default instead of creating a save file for an empty path.
            if (!IsConfigured) return DefaultValue;

            _value = SaveVarStorage.GetSaveData(RelativePath).GetKey(SaveKey, DefaultValue);
            _hasValue = true;

            return _value;
        }

        /// <summary>
        /// Write the value to memory and mark the variable as not synced. Does not touch the disk.
        /// </summary>
        /// <param name="value">The new value</param>
        public virtual void SetValue(T value)
        {
            var oldValue = GetValue();

            if (EqualityComparer<T>.Default.Equals(oldValue, value)) return;

            WriteValue(value);
            SetIsSync(false);

            onValueChanged?.Invoke(oldValue, value);
        }

        /// <summary>
        /// Write the value to memory and mark the variable as not synced, without raising
        /// <see cref="onValueChanged"/>. Useful to break feedback loops between a variable and a UI field.
        /// </summary>
        /// <param name="value">The new value</param>
        public void SetValueWithoutNotify(T value)
        {
            if (EqualityComparer<T>.Default.Equals(GetValue(), value)) return;

            WriteValue(value);
            SetIsSync(false);
        }

        /// <summary>
        /// Put <see cref="DefaultValue"/> back. The file is untouched until the next push.
        /// </summary>
        public void ResetToDefault() => SetValue(DefaultValue);

        private void WriteValue(T value)
        {
            _value = value;
            _hasValue = true;

            if (IsConfigured)
                SaveVarStorage.GetSaveData(RelativePath).SetKey(SaveKey, value);
        }

        public override string GetStringValue()
        {
            return Value?.ToString() ?? "null";
        }

        #endregion

        #region SaveVar

        /// <inheritdoc/>
        public override void WriteToSaveData() =>
            SaveVarStorage.GetSaveData(RelativePath).SetKey(SaveKey, GetValue());

        /// <inheritdoc/>
        public override void ReadFromSaveData()
        {
            var oldValue = _hasValue ? _value : DefaultValue;
            var newValue = SaveVarStorage.GetSaveData(RelativePath).GetKey(SaveKey, DefaultValue);

            _value = newValue;
            _hasValue = true;

            SetIsSync(true);

            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
                onValueChanged?.Invoke(oldValue, newValue);
        }

        /// <inheritdoc/>
        protected override void RaisePushed() => onPushed?.Invoke(GetValue());

        /// <inheritdoc/>
        protected override void RaisePulled() => onPulled?.Invoke(GetValue());

        #endregion

        #region Conversion

        /// <summary>
        /// Read the value of a variable. A null variable yields the default of <typeparamref name="T"/>
        /// instead of throwing.
        /// </summary>
        /// <param name="saveVar">The variable to read</param>
        /// <returns>The stored value</returns>
        public static implicit operator T(SaveVar<T> saveVar) => saveVar is not null ? saveVar.Value : default;

        #endregion

        #region Equality

        /// <summary>
        /// Hash of the stored value.
        /// </summary>
        /// <returns>The hash code of the value</returns>
        public override int GetHashCode() => Value is null ? 0 : Value.GetHashCode();

        /// <summary>
        /// Compare against a raw value.
        /// </summary>
        /// <param name="other">The value to compare to</param>
        /// <returns>True when the stored value equals it</returns>
        public bool Equals(T other) => EqualityComparer<T>.Default.Equals(Value, other);

        /// <summary>
        /// Compare against another variable by value. A null variable is never equal.
        /// </summary>
        /// <param name="other">The variable to compare to</param>
        /// <returns>True when both hold the same value</returns>
        public bool Equals(SaveVar<T> other) =>
            other is not null && EqualityComparer<T>.Default.Equals(Value, other.Value);

        /// <summary>
        /// Compare against another variable or a raw value.
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns>True when the values are equal</returns>
        public override bool Equals(object obj) => obj switch
        {
            null => false,
            SaveVar<T> saveVar => Equals(saveVar),
            T value => Equals(value),
            _ => false
        };

        /// <summary>
        /// Compare two variables by value. Two nulls are equal, one null never is.
        /// </summary>
        /// <param name="left">The left variable</param>
        /// <param name="right">The right variable</param>
        /// <returns>True when both hold the same value</returns>
        public static bool operator ==(SaveVar<T> left, SaveVar<T> right)
        {
            if (ReferenceEquals(left, right)) return true;

            if (left is null || right is null) return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Compare two variables by value.
        /// </summary>
        /// <param name="left">The left variable</param>
        /// <param name="right">The right variable</param>
        /// <returns>True when they hold different values</returns>
        public static bool operator !=(SaveVar<T> left, SaveVar<T> right) => !(left == right);

        /// <summary>
        /// Compare a variable against a raw value.
        /// </summary>
        /// <param name="left">The variable</param>
        /// <param name="right">The value to compare to</param>
        /// <returns>True when the variable holds that value</returns>
        public static bool operator ==(SaveVar<T> left, T right) => left is not null && left.Equals(right);

        /// <summary>
        /// Compare a variable against a raw value.
        /// </summary>
        /// <param name="left">The variable</param>
        /// <param name="right">The value to compare to</param>
        /// <returns>True when the variable holds a different value</returns>
        public static bool operator !=(SaveVar<T> left, T right) => !(left == right);

        /// <summary>
        /// Compare a raw value against a variable.
        /// </summary>
        /// <param name="left">The value to compare to</param>
        /// <param name="right">The variable</param>
        /// <returns>True when the variable holds that value</returns>
        public static bool operator ==(T left, SaveVar<T> right) => right == left;

        /// <summary>
        /// Compare a raw value against a variable.
        /// </summary>
        /// <param name="left">The value to compare to</param>
        /// <param name="right">The variable</param>
        /// <returns>True when the variable holds a different value</returns>
        public static bool operator !=(T left, SaveVar<T> right) => !(right == left);

        #endregion

        #region ToString
        
        /// <summary>
        /// The value as text, so logs show the value instead of the type name.
        /// </summary>
        /// <returns>The value as text</returns>
        public override string ToString() => Value?.ToString() ?? "null";
        
        #endregion
    }
}