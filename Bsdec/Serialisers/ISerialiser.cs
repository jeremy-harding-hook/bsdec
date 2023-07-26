//-----------------------------------------------------------------------
//
// Copyright 2023 Jeremy Harding Hook
//
// This file is part of Bsdec.
//
// Bsdec is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// Bsdec is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along with
// Bsdec. If not, see <https://www.gnu.org/licenses/>.
//
//-----------------------------------------------------------------------

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
