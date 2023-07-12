using System;
using System.IO;

namespace Bsdec.Serialisers
{
    internal interface ISerialiser
    {
        /// <summary>
        /// Deserialises an input stream into an object of a given type.
        /// </summary>
        /// <param name="inputStream">The input stream to parse.</param>
        /// <param name="type">The type to which the stream is to be parsed.</param>
        /// <returns>An object of type dynamic, to account for types unknown at compiletime.</returns>
        public static abstract object? Deserialise(Stream inputStream, Type type);

        /// <summary>
        /// Serialises an object into a given output stream.
        /// </summary>
        /// <param name="serialisee">The object to be serialised.</param>
        /// <param name="outputStream">The stream to write the output to.</param>
        public static abstract void Serialise(object? serialisee, Stream outputStream);
    }
}
