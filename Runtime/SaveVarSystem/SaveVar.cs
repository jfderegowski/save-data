using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using fefek5.SaveDataVariable.Runtime.Settings;
using UnityEngine;

namespace fefek5.SaveDataVariable.Runtime
{
    [Serializable]
    public abstract class SaveVar
    {
        #region Properties
        
        /// <summary>
        /// True when the value was changed but not written to the file yet.
        /// </summary>
        public bool IsDirty => SaveVarStorage.IsDirty(RelativePath, SaveKey);

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

        #region Constructors

        /// <summary>
        /// Parameterless constructor used by Unity serialization.
        /// </summary>
        protected SaveVar() : this("SaveVars.json", SaveKey.RandomKey) { }

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

        #region Push and Pull

        public abstract void Push();
        
        public abstract Awaitable PushAsync(CancellationToken cancellationToken =  default);
        
        public abstract void Pull();
        
        public abstract Awaitable PullAsync(CancellationToken cancellationToken =  default);
        
        #endregion
        
        #region ToString

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion
    }

    [Serializable]
    public class SaveVar<T> : SaveVar, IEquatable<SaveVar<T>>, IEquatable<T>
    {
        #region Inspector Fields

        /// <summary>
        /// Value reported before the file was read, and whenever the key is missing from the file.
        /// </summary>
        [field: SerializeField] public T DefaultValue { get; private set; }

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
        public SaveVar() : this("SaveVar.json", SaveKey.RandomKey) { }

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
        public virtual T GetValue() => SaveVarStorage.GetValue(RelativePath, SaveKey, DefaultValue);

        /// <summary>
        /// Write the value to memory and mark the variable as not synced. Does not touch the disk.
        /// </summary>
        /// <param name="value">The new value</param>
        public virtual void SetValue(T value) => SaveVarStorage.SetValue(RelativePath, SaveKey, value);

        /// <summary>
        /// Put <see cref="DefaultValue"/> back. The file is untouched until the next push.
        /// </summary>
        public void ResetToDefault() => SetValue(DefaultValue);

        public override string GetStringValue() => Value?.ToString() ?? "null";

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

        #region Push and Pull

        public override void Push() => 
            SaveVarStorage.Push(RelativePath);

        public override async Awaitable PushAsync(CancellationToken cancellationToken =  default) => 
            await SaveVarStorage.PullAsync(RelativePath, cancellationToken);

        public override void Pull() => 
            SaveVarStorage.Pull(RelativePath);

        public override async Awaitable PullAsync(CancellationToken cancellationToken =  default) => 
            await SaveVarStorage.PullAsync(RelativePath, cancellationToken);

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