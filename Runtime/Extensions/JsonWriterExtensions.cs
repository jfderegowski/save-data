using System;
using Newtonsoft.Json;

namespace fefek5.Variables.SaveDataVariable.Runtime.Extensions
{
    public static class JsonWriterExtensions
    {
                /// <summary>
        /// Writes the type of the object to the JSON writer.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> used for serialization.</param>
        /// <param name="value">The object whose type to write.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>True if the type was written; otherwise, false.</returns>
        public static bool TryWriteType<T>(this JsonWriter writer, JsonSerializer serializer, T value) => 
            writer.TryWriteType(serializer, typeof(T));

        /// <summary>
        /// Writes the type of the object to the JSON writer if TypeNameHandling is not None.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> used for serialization.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>True if the type was written; otherwise, false.</returns>
        public static bool TryWriteType<T>(this JsonWriter writer, JsonSerializer serializer) => 
            writer.TryWriteType(serializer, typeof(T));

        /// <summary>
        /// Writes the type of the object to the JSON writer if TypeNameHandling is not None.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> used for serialization.</param>
        /// <param name="type">The type of the object.</param>
        /// <returns>True if the type was written; otherwise, false.</returns>
        public static bool TryWriteType(this JsonWriter writer, JsonSerializer serializer, Type type)
        {
            if (serializer.TypeNameHandling == TypeNameHandling.None)
                return false;

            writer.WriteType(type);
            
            return true;
        }

        /// <summary>
        /// Writes the type of the object to the JSON writer.
        /// </summary>
        /// <param name="writer"><see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The object whose type to write.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        public static void WriteType<T>(this JsonWriter writer, T value) => writer.WriteType(typeof(T));

        /// <summary>
        /// Writes the type of the object to the JSON writer.
        /// </summary>
        /// <param name="writer"><see cref="JsonWriter"/> to write to.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        public static void WriteType<T>(this JsonWriter writer) => writer.WriteType(typeof(T));

        /// <summary>
        /// Writes the type of the object to the JSON writer.
        /// </summary>
        /// <param name="writer"><see cref="JsonWriter"/> to write to.</param>
        /// <param name="type">The type of the object.</param>
        public static void WriteType(this JsonWriter writer, Type type)
        {
            writer.WritePropertyName("$type");
            writer.WriteValue(type.GetJsonTypeString());
        }
        
        /// <summary>
        /// Gets the JSON type string representation of the specified type.
        /// </summary>
        /// <param name="type">The type to get the JSON type string for.</param>
        /// <returns>The JSON type string representation of the specified type.</returns>
        public static string GetJsonTypeString(this Type type)
        {
            var typeName = type.FullName;
            var assemblyName = type.Assembly.GetName().Name;
            
            return $"{typeName}, {assemblyName}";
        }
    }
}