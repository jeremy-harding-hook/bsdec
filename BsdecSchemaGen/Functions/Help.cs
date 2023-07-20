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

namespace BsdecSchemaGen.Functions
{
    internal static class Help
    {
        private static readonly string errorText =
            $"Usage: {Program.ProgramFileName} [options] class_name assembly_file output_file\n\n" +
            $"A tool for generating a savefile schema based on an assembly. This schema\n" +
            $"can then be used in conjunction with bsdec to permit manual reading and writing\n" +
            $"of savefiles.\n" +
            $"Note that this program is only compatible with .net assemblies in CIL.\n" +
            $"If obfuscation has been applied, you may need to undo that first.\n\n" +
            $"Options:\n" +
            $"    -r reader\n" +
            $"    --reader reader\n" +
            $"        Specifies the name of the reader method. At least one of -r, -w must be \n" +
            $"        set.\n\n" +
            $"    -w writer\n" +
            $"    --writer writer\n" +
            $"        Specifies the name of the writer method. At least one of -r, -w must be\n" +
            $"        set.\n\n" +
            $"    -h\n" +
            $"    --help\n" +
            $"        Returns this help text.\n\n" +
            $"Parameters:\n" +
            $"    class_name:\n" +
            $"        The name of the top-level class corresponding to the object being saved,\n" +
            $"        fully qualified with namespace. If no fully-qualified match exists, the\n" +
            $"        program will instead treat the class_name as partially qualified and try\n" +
            $"        to make it work.\n\n" +
            $"    assembly_file:\n" +
            $"        The path to the assembly containing the code for saving.\n\n" +
            $"    output_file:\n" +
            $"        The location to which the generated schema is to be saved.\n\n" +
            $"Exit values:\n" +
            $"    0: Successfully executed.\n" +
            $"    1: Generic error prevented the schema from being generated.\n" +
            $"    2: Bad usage.";

        public static void ShowHelpText(bool stderr)
        {
            if(stderr)
                Console.Error.WriteLine(errorText);
            else
                Console.WriteLine(errorText);
        }
    }
}
