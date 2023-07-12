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
