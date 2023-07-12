using Bsdec.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Bsdec
{
    internal partial class Program
    {
        internal static string? ProgramFileName { get; private set; }
#if RELEASELINUX
        internal const string ShippedProgramFilename = "bsdec";
#else
        internal const string ShippedProgramFilename = "Bsdec.exe";
#endif

        static void Main(string[] args)
        {
            try
            {
                ProgramFileName = Path.GetFileName(Environment.ProcessPath);
                if (string.IsNullOrEmpty(ProgramFileName))
                    ProgramFileName = ShippedProgramFilename;

                int returnValue = HandleArgs(args);
                Environment.Exit(returnValue);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unhandled exception: {ex.Message}");
                Environment.Exit(1);
            }
        }

        static int HandleArgs(string[] args)
        {
            Option helpOption = new("help", 'h');
            Option inputOption = new("in", 'i');
            Option outputOption = new("out", 'o');

            Dictionary<Decoder.Format, Option> formatOptions = new()
            {
                {Decoder.Format.Auto, new("auto", 'a')},
                {Decoder.Format.Binary, new("binary", 'b')},
                {Decoder.Format.Json, new("json", 'j')},
                {Decoder.Format.Xml, new("xml", 'x')}
            };

            List<Option> options = new()
            {
                helpOption,
                inputOption,
                outputOption,
            };

            options.AddRange(formatOptions.Values);

            string? schemaFile = null;
            Decoder.Format inputFormat = Decoder.Format.Auto;
            Decoder.Format outputFormat = Decoder.Format.Auto;

            Regex multiflagFinder = shortFlags();
            Regex longFlagFinder = longFlags();
            foreach (string arg in args)
            {
                if (multiflagFinder.IsMatch(arg))
                {
                    foreach (char flag in arg[1..])
                    {
                        if (!TryMatchFlag(options, flag))
                            return FailHelpfully();
                    }
                }
                else if (longFlagFinder.IsMatch(arg))
                {
                    if (!TryMatchLongformOption(options, arg[2..]))
                        return FailHelpfully();
                }
                else if (inputOption.set)
                {
                    if (!Enum.TryParse(arg, true, out inputFormat))
                        return FailHelpfully();
                    inputOption.set = false;
                }
                else if (outputOption.set)
                {
                    if (!Enum.TryParse(arg, true, out outputFormat))
                        return FailHelpfully();
                    outputOption.set = false;
                }
                else if (schemaFile == null)
                {
                    schemaFile = arg;
                }
                else
                {
                    return FailHelpfully();
                }

                foreach (KeyValuePair<Decoder.Format, Option> option in formatOptions)
                {
                    if (option.Value.set)
                    {
                        option.Value.set = false;
                        if (inputOption.set)
                        {
                            inputOption.set = false;
                            inputFormat = option.Key;
                        }
                        else
                        {
                            outputOption.set = false;
                            outputFormat = option.Key;
                        }
                    }
                }
            }

            if (helpOption.set)
            {
                Help.ShowHelpText(false);
                return 0;
            }

            if (string.IsNullOrEmpty(schemaFile))
            {
                return FailHelpfully();
            }

            try
            {
                Decoder.Decode(inputFormat, outputFormat, schemaFile);
                return 0;
            }
            catch (GenericErrorException)
            {
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static int FailHelpfully()
        {
            Help.ShowHelpText(true);
            return 2;
        }

        private static bool TryMatchFlag(List<Option> options, char flag)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (flag == options[i].flag)
                {
                    options[i].set = true;
                    return true;
                }
            }
            return false;
        }

        private static bool TryMatchLongformOption(List<Option> options, string longformOption)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (longformOption == options[i].name)
                {
                    options[i].set = true;
                    return true;
                }
            }
            return false;
        }

        class Option
        {
            public bool set;
            public string? name;
            public char? flag;
            public Option(string? name, char? flag)
            {
                this.name = name;
                this.flag = flag;
            }
        }

        [GeneratedRegex("^-[a-zA-Z]+$")]
        private static partial Regex shortFlags();

        [GeneratedRegex("^--[a-zA-Z]+$")]
        private static partial Regex longFlags();
    }
}