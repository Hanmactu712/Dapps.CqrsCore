using System;

namespace Dapps.CqrsCore.Utilities
{
    public interface ISerializer
    {
        /// <summary>
        /// Returns a object of the desired type, deserialized from the input string.
        /// </summary>
        T Deserialize<T>(string value, Type type);

        /// <summary>
        /// Returns the serialized string value for an object of a specific type.
        /// </summary>
        string Serialize<T>(T value);

        /// <summary>
        /// Returns the serialized string value for an object, excluding properties in the exclusions array.
        /// </summary>
        string Serialize(object command, string[] exclusions);
    }
}
