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
