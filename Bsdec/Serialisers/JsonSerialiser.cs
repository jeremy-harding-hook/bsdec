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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bsdec.Serialisers
{
    internal class JsonSerialiser : ISerialiser
    {
        private static readonly JsonSerializerOptions options = new()
        {
            IncludeFields = true,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static object? Deserialise(Stream inputStream, Type type)
        {
            try
            {
                return JsonSerializer.Deserialize(inputStream, type, options);
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw new GenericErrorException();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception while deserialising the JSON: {ex}");
                throw new GenericErrorException();
            }
        }

        public static void Serialise(object? serialisee, Stream outputStream)
        {
            try
            {
                JsonSerializer.Serialize(outputStream, serialisee, options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception while serialising the JSON: {ex}");
                throw new GenericErrorException();
            }
        }
    }
}
