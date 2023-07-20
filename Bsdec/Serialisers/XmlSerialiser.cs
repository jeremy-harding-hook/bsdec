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
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Bsdec.Serialisers
{
    internal partial class XmlSerialiser : ISerialiser
    {
        public static object? Deserialise(Stream inputStream, Type type)
        {
            try
            {
                XmlSerializer serializer = new(type);
                return serializer.Deserialize(inputStream);
            }
            catch (InvalidOperationException ex) when (validationErrorRegex().IsMatch(ex.Message))
            {
                Console.Error.WriteLine(ex.Message);
                if (ex.InnerException?.Message != null)
                    Console.Error.WriteLine(ex.InnerException.Message);
                throw new GenericErrorException();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception while deserialising the XML: {ex}");
                throw new GenericErrorException();
            }
        }

        public static void Serialise(object? serialisee, Stream outputStream)
        {
            Type serialiseeType = serialisee == null ? typeof(object) : serialisee.GetType();
            XmlSerializer serializer = new(serialiseeType);
            serializer.Serialize(outputStream, serialisee);
        }

        [GeneratedRegex(@"There is an error in XML document \(\d*, \d*\)\.")]
        private static partial Regex validationErrorRegex();
    }
}
