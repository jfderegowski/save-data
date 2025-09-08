using System;
using fefek5.Common.Runtime.Extensions;
using fefek5.Variables.HasValueVariable.Runtime;
using fefek5.Variables.SerializableGuidVariable.Runtime;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Runtime
{
    /// <summary>
    /// This struct is used to create a key for the save data.
    /// It can be used as a SerializableGuid or a string.
    /// </summary>
    [Serializable]
    public struct SaveKey : IEquatable<SaveKey>, IEquatable<SerializableGuid>, IEquatable<string>
    {
        #region Fields

        /// <summary>
        /// Key for the save data. It is a SerializableGuid.
        /// </summary>
        [Tooltip("Use this key to have fast compare")] 
        public SerializableGuid Key;

        /// <summary>
        /// Key for the save data. It is a string.
        /// </summary>
        [Tooltip("If the value is not empty, it will be used instead of the SerializableGuid key.")]
        public HasValue<string> StringKey;
        
        /// <summary>
        /// Comment for the key. If its empty it will no serialize in file
        /// </summary>
        [Tooltip("Comment for the key. If its empty it will no serialize in file")] 
        public HasValue<string> Comment;

        #endregion

        #region Properties

        /// <summary>
        /// Random key for the save data. It is using SerializableGuid.
        /// </summary>
        public static SaveKey RandomKey => NewKey();

        /// <summary>
        /// Empty key for the save data. It is a SerializableGuid.
        /// </summary>
        public static SaveKey Empty => NewEmptyKey();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for the SaveKey. It will create a new key with the given Guid.
        /// </summary>
        /// <param name="key">The key for the save data. It is a Guid.</param>
        public SaveKey(Guid key)
        {
            Key = key.ToSerializableGuid();
            StringKey = new HasValue<string>(string.Empty, false);
            Comment = new HasValue<string>(string.Empty, false);
        }

        public SaveKey(Guid key, string comment)
        {
            Key = key.ToSerializableGuid();
            StringKey = new HasValue<string>(string.Empty, false);
            Comment = new HasValue<string>(comment, !comment.IsBlank());
        }

        /// <summary>
        /// Constructor for the SaveKey. It will create a new key with the given SerializableGuid.
        /// </summary>
        /// <param name="key">The key for the save data. It is a SerializableGuid.</param>
        public SaveKey(SerializableGuid key)
        {
            Key = key;
            StringKey = new HasValue<string>(string.Empty, false);
            Comment = new HasValue<string>(string.Empty, false);
        }
        
        /// <summary>
        /// Constructor for the SaveKey. It will create a new key with the given SerializableGuid and comment.
        /// </summary>
        /// <param name="key">The key for the save data. It is a SerializableGuid.</param>
        /// <param name="comment">The comment for the key. If its empty it will no serialize in file</param>
        public SaveKey(SerializableGuid key, string comment)
        {
            Key = key;
            StringKey = new HasValue<string>(string.Empty, false);
            Comment = new HasValue<string>(comment, !comment.IsBlank());
        }
        
        /// <summary>
        /// Constructor for the SaveKey. It will create a new key with the given string.
        /// </summary>
        /// <param name="key">The key for the save data. It is a string.</param>
        /// <exception cref="ArgumentException">Thrown when the key is empty.</exception>
        public SaveKey(string key)
        {
            if (key.IsBlank())
                throw new ArgumentException("Key is empty.", nameof(key));
            
            if (SerializableGuid.IsHexString(key))
            {
                Key = SerializableGuid.FromHexString(key);
                StringKey = new HasValue<string>(string.Empty, false);
            }
            else
            {
                Key = SerializableGuid.Empty;
                StringKey = new HasValue<string>(key, true);
            }
            
            Comment = new HasValue<string>(string.Empty, false);
        }
        
        /// <summary>
        /// Constructor for the SaveKey. It will create a new key with the given string and comment.
        /// </summary>
        /// <param name="key">The key for the save data. It is a string.</param>
        /// <param name="comment">The comment for the key. If its empty it will no serialize in file</param>
        public SaveKey(string key, string comment)
        {
            if (SerializableGuid.IsHexString(key))
            {
                Key = SerializableGuid.FromHexString(key);
                StringKey = new HasValue<string>(string.Empty, false);
            }
            else
            {
                Key = SerializableGuid.Empty;
                StringKey = new HasValue<string>(key, true);
            }
            
            Comment = new HasValue<string>(comment, !comment.IsBlank());
        }

        /// <summary>
        /// Copy constructor for the SaveKey. It will create a new key with the given SaveKey.
        /// </summary>
        /// <param name="saveKey">The key for the save data. It is a SaveKey.</param>
        public SaveKey(SaveKey saveKey)
        {
            Key = saveKey.Key;
            StringKey = saveKey.StringKey;
            Comment = saveKey.Comment;
        }

        #endregion
        
        #region Methods
        
        /// <summary>
        /// Set the key for the save data. It will use the given string as a key.
        /// </summary>
        /// <param name="key">The key for the save data. It is a string.</param>
        /// <returns>The current instance of the SaveKey.</returns>
        public SaveKey SetKey(string key)
        {
            if (key.IsBlank())
            {
                Debug.LogWarning("Key is empty. SetKey method will not change the key.");
                return this;
            }
            
            if (SerializableGuid.IsHexString(key))
                return SetKey(SerializableGuid.FromHexString(key));

            StringKey.Set(key, true);
            Key = SerializableGuid.Empty;

            return this;
        }
        
        /// <summary>
        /// Set the key for the save data. It will use the given SerializableGuid as a key.
        /// </summary>
        /// <param name="key">The key for the save data. It is a SerializableGuid.</param>
        /// <returns>The current instance of the SaveKey.</returns>
        public SaveKey SetKey(SerializableGuid key)
        {
            if (key.IsEmpty)
            {
                Debug.LogWarning("Key is empty. SetKey method will not change the key.");
                return this;
            }
            
            Key = key;
            StringKey.Set(string.Empty, false);
            return this;
        }

        /// <summary>
        /// Set the comment for the key. If its empty it will no serialize in file.
        /// </summary>
        /// <param name="comment">The comment for the key. If its empty it will no serialize in file.</param>
        /// <returns>The current instance of the SaveKey.</returns>
        public SaveKey SetComment(string comment)
        {
            Comment.Set(comment, !comment.IsBlank());
            return this;
        }
        
        /// <summary>
        /// Set the comment for the key. If its empty it will no serialize in file.
        /// </summary>
        /// <returns>The current instance of the SaveKey.</returns>
        public SaveKey RemoveComment()
        {
            Comment.Set(string.Empty, false);
            return this;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Create a new key for the save data. It will create a new key with a random Guid.
        /// </summary>
        /// <returns>The new key for the save data.</returns>
        public static SaveKey NewKey() => new(SerializableGuid.NewGuid());
        
        /// <summary>
        /// Create a new empty key for the save data. It will create a new key with an empty Guid.
        /// </summary>
        /// <returns>The new empty key for the save data.</returns>
        public static SaveKey NewEmptyKey() => new(SerializableGuid.Empty);

        #endregion

        #region Conditionals

        /// <summary>
        /// Check if the key is empty. If the key is a string, it will check if the string is empty or blank.
        /// </summary>
        /// <returns>True if the key is empty, false otherwise.</returns>
        public bool IsEmpty() => StringKey.hasValue ? StringKey.value.IsBlank() : Key.IsEmpty;

        /// <summary>
        /// Check if the key is valid. If the key is a string, it will check if the string is not empty or blank.
        /// </summary>
        /// <returns>True if the key is valid, false otherwise.</returns>
        public bool IsValid() => !IsEmpty();

        #endregion

        #region Equality

        /// <summary>
        /// Check if the key is equal to another SaveKey. If the other key is a string, it will check if the string is equal to the key.
        /// </summary>
        /// <param name="other">The other key to compare to.</param>
        /// <returns>True if the keys are equal, false otherwise.</returns>
        public bool Equals(SaveKey other)
        {
            if (StringKey.hasValue && other.StringKey.hasValue)
                return StringKey.value == other.StringKey.value;
            
            return Key == other.Key; 
        }
        
        /// <summary>
        /// Check if the key is equal to another SerializableGuid. If the other key is a string, it will check if the string is equal to the key.
        /// </summary>
        /// <param name="other">The other key to compare to.</param>
        /// <returns>True if the keys are equal, false otherwise.</returns>
        public bool Equals(SerializableGuid other) => !StringKey.hasValue && Key == other;

        /// <summary>
        /// Check if the key is equal to another string. If the other key is a SerializableGuid,
        /// it will check if the string is equal to the key.
        /// </summary>
        /// <param name="other">The other key to compare to.</param>
        /// <returns>True if the keys are equal, false otherwise.</returns>
        public bool Equals(string other)
        {
            if (SerializableGuid.IsHexString(other)) 
                return Equals(SerializableGuid.FromHexString(other));
            
            return StringKey.hasValue && StringKey.value == other;
        }

        /// <summary>
        /// Check if the key is equal to another object. If the other key is a string, it will check if the string is equal to the key.
        /// </summary>
        /// <param name="obj">The other key to compare to.</param>
        /// <returns>True if the keys are equal, false otherwise.</returns>
        public override bool Equals(object obj) => obj switch {
            null => false,
            SaveKey other => Equals(other),
            SerializableGuid serializableGuid => Equals(serializableGuid),
            string str => Equals(str),
            _ => false
        };

        #endregion
        
        #region GetHashCode

        /// <summary>
        /// Get the hash code for the key. If the key is a string, it will use the string as a key.
        /// </summary>
        /// <returns>The hash code for the key.</returns>
        public override int GetHashCode() => StringKey.hasValue 
            ? HashCode.Combine(true, StringKey.value.GetHashCode())
            : HashCode.Combine(false, Key.GetHashCode());

        #endregion
        
        #region Operators

        /// <summary>
        /// Check if the key is equal to another SaveKey. If the other key is a string,
        /// it will check if the string is equal to the key.
        /// </summary>
        /// <param name="left">The left SaveKey to compare to.</param>
        /// <param name="right">The right SaveKey to compare to.</param>
        /// <returns>True if the keys are equal, false otherwise.</returns>
        public static bool operator ==(SaveKey left, SaveKey right) => left.Equals(right);

        /// <summary>
        /// Check if the key is not equal to another SaveKey. If the other key is a string,
        /// </summary>
        /// <param name="left">The left SaveKey to compare to.</param>
        /// <param name="right">The right SaveKey to compare to.</param>
        /// <returns>True if the keys are not equal, false otherwise.</returns>
        public static bool operator !=(SaveKey left, SaveKey right) => !left.Equals(right);
        
        /// <summary>
        /// Check if the key is equal to another SerializableGuid. If the other key is a string,
        /// </summary>
        /// <param name="left">The left SaveKey to compare to.</param>
        /// <param name="right">The right SerializableGuid to compare to.</param>
        /// <returns>True if the keys are equal, false otherwise.</returns>
        public static bool operator ==(SaveKey left, SerializableGuid right) => left.Equals(right);
        
        /// <summary>
        /// Check if the key is not equal to another SerializableGuid. If the other key is a string,
        /// </summary>
        /// <param name="left">The left SaveKey to compare to.</param>
        /// <param name="right">The right SerializableGuid to compare to.</param>
        /// <returns>True if the keys are not equal, false otherwise.</returns>
        public static bool operator !=(SaveKey left, SerializableGuid right) => !left.Equals(right);
        
        /// <summary>
        /// Check if the key is equal to another SerializableGuid. If the other key is a string,
        /// </summary>
        /// <param name="left">The left SerializableGuid to compare to.</param>
        /// <param name="right">The right SaveKey to compare to.</param>
        /// <returns>True if the keys are equal, false otherwise.</returns>
        public static bool operator ==(SerializableGuid left, SaveKey right) => right.Equals(left);
        
        /// <summary>
        /// Check if the key is not equal to another SerializableGuid. If the other key is a string,
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(SerializableGuid left, SaveKey right) => !right.Equals(left);
        
        /// <summary>
        /// Check if the key is equal to another string. If the other key is a SerializableGuid,
        /// </summary>
        /// <param name="left">The left SaveKey to compare to.</param>
        /// <param name="right">The right string to compare to.</param>
        /// <returns>True if the keys are equal, false otherwise.</returns>
        public static bool operator ==(SaveKey left, string right) => left.Equals(right);
        
        /// <summary>
        /// Check if the key is not equal to another string. If the other key is a SerializableGuid,
        /// </summary>
        /// <param name="left">The left SaveKey to compare to.</param>
        /// <param name="right">The right string to compare to.</param>
        /// <returns>True if the keys are not equal, false otherwise.</returns>
        public static bool operator !=(SaveKey left, string right) => !left.Equals(right);
        
        /// <summary>
        /// Check if the key is equal to another string. If the other key is a SerializableGuid,
        /// </summary>
        /// <param name="left">The left string to compare to.</param>
        /// <param name="right">The right SaveKey to compare to.</param>
        /// <returns>True if the keys are equal, false otherwise.</returns>
        public static bool operator ==(string left, SaveKey right) => right.Equals(left);
        
        /// <summary>
        /// Check if the key is not equal to another string. If the other key is a SerializableGuid,
        /// </summary>
        /// <param name="left">The left string to compare to.</param>
        /// <param name="right">The right SaveKey to compare to.</param>
        /// <returns>True if the keys are not equal, false otherwise.</returns>
        public static bool operator !=(string left, SaveKey right) => !right.Equals(left);

        #endregion

        #region Implicit
        
        /// <summary>
        /// Implicit conversion from SerializableGuid to SaveKey.
        /// </summary>
        /// <param name="key">The key for the save data. It is a SerializableGuid.</param>
        /// <returns>The SaveKey.</returns>
        public static implicit operator SaveKey(SerializableGuid key) => new(key);
        
        /// <summary>
        /// Implicit conversion from SerializableGuid to SaveKey.
        /// </summary>
        /// <param name="key">The key for the save data. It is a SerializableGuid.</param>
        /// <returns></returns>
        public static implicit operator SaveKey(Guid key) => new(key);
        
        /// <summary>
        /// Implicit conversion from string to SaveKey.
        /// </summary>
        /// <param name="key">The key for the save data. It is a string.</param>
        /// <returns>The SaveKey.</returns>
        public static implicit operator SaveKey(string key) => new(key);
        
        /// <summary>
        /// Implicit conversion from SaveKey to SerializableGuid.
        /// </summary>
        /// <param name="saveKey">The key for the save data. It is a SaveKey.</param>
        /// <returns>The SerializableGuid.</returns>
        public static implicit operator SerializableGuid(SaveKey saveKey) => 
            saveKey.StringKey.hasValue ? SerializableGuid.Empty : saveKey.Key;

        /// <summary>
        /// Implicit conversion from SaveKey to string.
        /// </summary>
        /// <param name="saveKey">The key for the save data. It is a SaveKey.</param>
        /// <returns>The string.</returns>
        public static implicit operator string(SaveKey saveKey) =>
            saveKey.StringKey.hasValue ? saveKey.StringKey.value : saveKey.Key.ToHexString();
        
        #endregion

        #region ToString

        /// <summary>
        /// Override the ToString method to return the key as a string. If the key is a string, it will return the string.
        /// </summary>
        /// <returns>The key as a string.</returns>
        public override string ToString() => StringKey.hasValue ? StringKey.value : Key.ToString();

        #endregion
    }
}
