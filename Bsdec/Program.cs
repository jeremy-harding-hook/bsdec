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

        static readonly Option helpOption = new("help", 'h');
        static readonly Option aboutOption = new("about", null);
        static readonly Option licenseOption = new("license", null);
        static readonly Option inputOption = new("in", 'i');
        static readonly Option outputOption = new("out", 'o');

        static readonly Dictionary<Decoder.Format, Option> formatOptions = new()
            {
                {Decoder.Format.Auto, new("auto", 'a')},
                {Decoder.Format.Binary, new("binary", 'b')},
                {Decoder.Format.Json, new("json", 'j')},
                {Decoder.Format.Xml, new("xml", 'x')}
            };

        static readonly List<Option> options;

        static Decoder.Format inputFormat = Decoder.Format.Auto;
        static Decoder.Format outputFormat = Decoder.Format.Auto;

        static Program()
        {
            options = new()
            {
                helpOption,
                aboutOption,
                licenseOption,
                inputOption,
                outputOption,
            };
            options.AddRange(formatOptions.Values);
        }

        static int HandleArgs(string[] args)
        {
            string? schemaFile = null;
            
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
                        CheckFormatOptions();
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

                CheckFormatOptions();
            }

            if (helpOption.set)
            {
                Help.ShowHelpText(false);
                return 0;
            }

            if (aboutOption.set)
            {
                About.ShowAboutText();
                return 0;
            }

            if (licenseOption.set)
            {
                License.ShowLicenseText();
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

        private static void CheckFormatOptions()
        {
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
