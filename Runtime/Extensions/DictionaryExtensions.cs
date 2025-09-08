using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace fefek5.Variables.SaveDataVariable.Runtime.Extensions
{
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Get the value of a dictionary by key and convert it to the specified type.
        /// If the key does not exist or the conversion fails, return the default value.
        /// </summary>
        /// <param name="dict">The dictionary to search.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="defaultValue">The default value to return if the key does not exist or conversion fails.</param>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <typeparam name="TK">The type of the key in the dictionary.</typeparam>
        /// <typeparam name="TV">The type of the value in the dictionary.</typeparam>
        /// <returns>The value converted to the specified type, or the default value if the conversion fails.</returns>
        public static T GetAs<T, TK, TV>(this IDictionary<TK, TV> dict, TK key, T defaultValue = default)
        {
            if (!dict.TryGetValue(key, out var rawValue) || rawValue is null)
                return defaultValue;

            if (rawValue is T t)
                return t;

            var targetType = typeof(T);

            try
            {
                if (rawValue is JToken jToken)
                    return jToken.ToObject<T>();

                if (targetType.IsEnum && rawValue is IConvertible)
                    return (T)Enum.ToObject(targetType, rawValue);

                if (rawValue is IConvertible)
                    return (T)Convert.ChangeType(rawValue, targetType);
            }
            catch
            {
                // Conversion failed; fall through to return defaultValue
            }

            return defaultValue;
        }
    }
}