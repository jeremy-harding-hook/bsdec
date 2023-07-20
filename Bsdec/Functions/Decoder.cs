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

using Bsdec.Serialisers;
using BsdecCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bsdec.Functions
{
    internal static class Decoder
    {
        public static void Decode(Format inputFormat, Format outputFormat, string schemaFilepath)
        {
            Assembly schemaAssembly = LoadAssembly(schemaFilepath);
            Type topLevelType = GetTopLevelType(schemaAssembly);

            using Stream stdin = Console.OpenStandardInput();
            using Stream stdout = Console.OpenStandardOutput();

            object? intermediateObject = Deserialise(stdin, topLevelType, ref inputFormat);
            Serialise(intermediateObject, stdout, outputFormat, inputFormat);
        }

        private static object? Deserialise(Stream stdin, Type topLevelType, ref Format format)
        {
            switch (format)
            {
                case Format.Auto:
                    {
                        using PeekableStream proxyStdin = new(stdin);

                        switch (proxyStdin.Peek())
                        {
                            case (byte)'{':
                                format = Format.Json;
                                return JsonSerialiser.Deserialise(proxyStdin, topLevelType);
                            case (byte)'<':
                                format = Format.Xml;
                                return XmlSerialiser.Deserialise(proxyStdin, topLevelType);
                            default:
                                format = Format.Binary;
                                return BinarySerialiser.Deserialise(proxyStdin, topLevelType);
                        }
                    }

                case Format.Binary:
                    return BinarySerialiser.Deserialise(stdin, topLevelType);
                case Format.Json:
                    return JsonSerialiser.Deserialise(stdin, topLevelType);
                case Format.Xml:
                    return XmlSerialiser.Deserialise(stdin, topLevelType);
                default:
                    Console.Error.WriteLine("Unrecognised input format; could not deserialise.");
                    throw new GenericErrorException();
            }
        }

        private static void Serialise(object? item, Stream stdout, Format format, Format actualInputFormat)
        {
            switch (format)
            {
                case Format.Auto:
                    if (actualInputFormat != Format.Binary)
                        goto case Format.Binary;
                    if (FullMoonDetector.IsFullNow())
                        goto case Format.Xml;
                    goto case Format.Json;
                case Format.Binary:
                    BinarySerialiser.Serialise(item, stdout);
                    return;
                case Format.Json:
                    JsonSerialiser.Serialise(item, stdout);
                    return;
                case Format.Xml:
                    XmlSerialiser.Serialise(item, stdout);
                    return;
                default:
                    Console.Error.WriteLine("Unrecognised output format; could not serialise.");
                    throw new GenericErrorException();
            }
        }

        public static Assembly LoadAssembly(string filepath)
        {
            if (!File.Exists(filepath))
            {
                Console.Error.WriteLine($"Schema file could not be found at \"{filepath}\".");
                throw new GenericErrorException();
            }

            try
            {
                return Assembly.LoadFile(Path.GetFullPath(filepath));
            }
            catch
            {
                Console.Error.WriteLine($"Schema file could not be loaded. Are you sure it is valid and accessable?");
                throw new GenericErrorException();
            }
        }

        public static Type GetTopLevelType(Assembly assembly)
        {
            IEnumerable<Type> topLevelTypeCandidates = assembly.GetTypes().Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(BsdecToplevelSavefileClassAttribute)));
            if (!topLevelTypeCandidates.Any())
            {
                Console.Error.WriteLine($"The schema file was loaded, but did not appear to be a valid schema (no {nameof(BsdecToplevelSavefileClassAttribute)}). Are you sure this is the right file?");
                throw new GenericErrorException();
            }
            if (topLevelTypeCandidates.Count() > 1)
            {
                Console.Error.WriteLine($"The schema file was loaded, but did not appear to be a valid schema ({nameof(BsdecToplevelSavefileClassAttribute)} appears {topLevelTypeCandidates.Count()} times). Are you sure this is the right file?");
                throw new GenericErrorException();
            }
            return topLevelTypeCandidates.First();
        }

        public enum Format
        {
            Auto,
            Binary,
            Json,
            Xml,
        }
    }
}
