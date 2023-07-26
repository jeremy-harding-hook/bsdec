//-----------------------------------------------------------------------
//
// Copyright 2023 Jeremy Harding Hook
//
// This file is part of BsdecSchemaGen.
//
// BsdecSchemaGen is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// BsdecSchemaGen is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along with
// BsdecSchemaGen. If not, see <https://www.gnu.org/licenses/>.
//
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace BsdecSchemaGen.Functions
{
    internal static class SchemaGenerator
    {
        public static int Run(string? assembly, string? filepath, string? className, string? readerName, string? writerName)
        {
            int returnValue = ValidateInputs(assembly, filepath, className, readerName, writerName);
            return returnValue != 0 ? returnValue : GenerateSchema(assembly!, filepath!, className!, readerName, writerName);
        }

        private static int ValidateInputs(string? assembly, string? filepath, string? className, string? readerName, string? writerName)
        {
            int returnValue = 0;

            if (assembly == null)
            {
                Console.Error.WriteLine("Assembly filepath must not be null.");
                returnValue = 2;
            }

            if (filepath == null)
            {
                Console.Error.WriteLine("Output filepath must not be null.");
                returnValue = 2;
            }

            if (className == null)
            {
                Console.Error.WriteLine("Class name must be set.");
                returnValue = 2;
            }

            if (readerName == null && writerName == null)
            {
                Console.Error.WriteLine("At least one of -w, -r must be set.");
                returnValue = 2;
            }

            if (returnValue == 2)
            {
                Console.Error.WriteLine($"See '{Program.ProgramFileName}' --help for correct usage.");
            }

            return returnValue;
        }

        private static int GenerateSchema(string assembly, string destinationFilepath, string className, string? readerName, string? writerName)
        {
            using ModuleDefinition? module = OpenAssembly(assembly);
            if (module == null)
            {
                return 1;
            }

            TypeDefinition? savefileClass = FindClass(module, className);
            if (savefileClass == null)
            {
                return 1;
            }

            MethodDefinition? toplevelReaderMethod = null;
            if (readerName != null)
                toplevelReaderMethod = FindMethod(savefileClass, readerName, false);

            MethodDefinition? toplevelWriterMethod = null;
            if (writerName != null)
                toplevelWriterMethod = FindMethod(savefileClass, writerName, true);

            if (toplevelWriterMethod == null && toplevelReaderMethod == null)
            {
                Console.Error.WriteLine($"No usable top-level read or write method available, aborting.");
                return 1;
            }

            if (toplevelWriterMethod == null)
            {
                Console.Error.WriteLine("Warning: The generated schema will only support reading of preexisting save files.");
            }

            if (toplevelReaderMethod == null)
            {
                Console.Error.WriteLine("Warning: The generated schema will support writing new save files, but will not permit reading from existing ones.");
            }

            return AssemblyBuilder.AssemblyBuilder.BuildAssembly(module, destinationFilepath, savefileClass, toplevelReaderMethod, toplevelWriterMethod);
        }

        private static ModuleDefinition? OpenAssembly(string assembly)
        {
            try
            {
                if (!File.Exists(assembly))
                {
                    Console.Error.WriteLine($"Assembly file '{Path.GetFullPath(assembly)}' could not be found.");
                    Console.Error.WriteLine($"Either it does not exist or the user is missing the needed permissions.");
                    return null;
                }

                DefaultAssemblyResolver resolver = new();
                resolver.AddSearchDirectory(Path.GetDirectoryName(assembly) ?? string.Empty);
                return ModuleDefinition.ReadModule(assembly, new ReaderParameters(ReadingMode.Deferred) { AssemblyResolver = resolver });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Could not open assembly file. Exception details:");
                Console.Error.WriteLine(ex.Message);
                return null;
            }
        }

        private static TypeDefinition? FindClass(ModuleDefinition module, string className)
        {
            TypeDefinition? classFound = module.Types.FirstOrDefault(t => t.FullName == className);
            if (classFound != null)
            {
                return classFound;
            }

            IEnumerable<TypeDefinition> candidates = module.Types.Where(t => t.FullName.EndsWith(className));

            switch (candidates.Count())
            {
                case 0:
                    Console.Error.WriteLine($"Class {className} could not be found.");
                    Console.Error.WriteLine($"Are you sure the assembly is the right one?");
                    return null;
                case 1:
                    classFound = candidates.First();
                    Console.Error.WriteLine($"Class {className} was not initially found, assuming {classFound.FullName}.");
                    return classFound;
                default:
                    Console.Error.WriteLine($"Class {className} was not found, did you mean one of the following?");
                    foreach (TypeDefinition type in candidates)
                    {
                        Console.Error.WriteLine(type.FullName);
                    }
                    return null;
            }
        }

        private static MethodDefinition? FindMethod(TypeDefinition type, string methodName, bool writer)
        {
            string ioManagerType = writer ? "System.IO.BinaryWriter" : "System.IO.BinaryReader";
            IEnumerable<MethodDefinition> definitions = type.Methods.Where(x => x.Name == methodName && x.Parameters.Any(x => x.ParameterType.FullName == ioManagerType));
            switch (definitions.Count())
            {
                case 0:
                    Console.Error.WriteLine($"A suitable method {type.Name}.{methodName} could not be found.");
                    return null;
                case 1:
                    return definitions.First();
                default:
                    Console.Error.WriteLine($"More than one suitable method {type.Name}.{methodName} was found.");
                    Console.Error.WriteLine($"At present this is badly handled. If it is an issue, please consider creating a PR.");
                    Console.Error.WriteLine($"Current behavior defaults to the option with the fewest parameters.");
                    return definitions.MinBy(x => x.Parameters.Count);
            }
        }
    }
}
