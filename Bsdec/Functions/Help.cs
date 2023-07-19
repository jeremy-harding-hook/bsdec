using System;
namespace Bsdec.Functions
{
    internal static class Help
    {
        private static readonly string errorText =
            $"Usage: {Program.ProgramFileName} [options] schema_file\n\n" +
            $"A tool for reading or writing a binary savefile using a schema dll\n" +
            $"which can be produced using the associated schema generator.\n\n" +
            $"Options:\n" +
            $"    -i input_format\n" +
            $"    --in input_format\n" +
            $"        Sets the input format. More on this below.\n\n" +
            $"    -o output_format\n" +
            $"    --out output_format\n" +
            $"        Sets the output format. More on this below.\n\n" +
            $"    -h\n" +
            $"    --help\n" +
            $"        Returns this help text.\n\n" +
            $"Parameters:\n" +
            $"    schema_file:\n" +
            $"        The location of the schema dll to use.\n\n" +
            $"Input/output formats:\n" +
            $"To set the format of the input or output, pass the appropriate -i or -o option\n" +
            $"followed by one of the format specifiers. Note that each format can be written\n" +
            $"using option syntax or parameter syntax, and the default for both input and\n" +
            $"output is auto. When using option syntax the preceeding -i or -o option is not\n" +
            $"required; if it is missing, the format option is assumed to apply to the output.\n" +
            $"If multiple formats are specified for either input or output, the last one\n" +
            $"to appear is used.\n\n" +
            $"    -a\n" +
            $"    --auto\n" +
            $"    auto\n" +
            $"        When used as input, makes a rough guess based on the first characters\n" +
            $"        of input. Under certain conditions, this 'good faith' effort is liable\n" +
            $"        to fail, so in scripts it is best to provide more precise instruction." +
            $"        When applied to output, if the input format actually used was json or\n" +
            $"        xml the output format is binary. If the input format used was binary,\n" +
            $"        the output format is json (or xml during the full moon).\n\n" +
            $"    -b\n" +
            $"    --binary\n" +
            $"    binary\n" +
            $"        Specifies the binary format, i.e. the format the savefiles are in when\n" +
            $"        found in the wild.\n\n" +
            $"    -j\n" +
            $"    --json\n" +
            $"    json\n" +
            $"        Specifies JSON format.\n\n" +
            $"    -x\n" +
            $"    --xml\n" +
            $"    xml\n" +
            $"        Specifies XML format.\n\n" +
            $"Exit values:\n" +
            $"    0: Successfully executed.\n" +
            $"    1: Generic error prevented the operation from being carried out.\n" +
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
