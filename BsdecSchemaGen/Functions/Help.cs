using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsdecSchemaGen.Functions
{
    internal static class Help
    {
        private static readonly string errorText =
            $"Usage: {Program.ProgramFileName} [options] class_name assembly_file\n\n" +
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
            $"If there are extra parameters to pass into the reader and writer methods, that\n" +
            $"can be achieved by appending them to the method name like so:\n" +
            "    -r reader_method_name+param_name_1=some_value+param_name_2=another_value\n" +
            "Note this only works with parameters of primitive type.\n\n" +
            $"Parameters:\n" +
            $"    class_name:\n" +
            $"        The name of the top-level class corresponding to the object being saved,\n" +
            $"        fully qualified with namespace. If no fully-qualified match exists, the\n" +
            $"        program will instead treat the class_name as partially qualified and will\n" +
            $"        try to make it work.\n\n" +
            $"    assembly_file:\n" +
            $"        The path to the assembly containing the saving code for saving.\n\n" +
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
