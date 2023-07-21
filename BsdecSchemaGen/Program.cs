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

using BsdecSchemaGen.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

[assembly: AssemblyVersion("1.0.0.0")]

namespace BsdecSchemaGen
{
    internal partial class Program
    {
        internal static string? ProgramFileName { get; private set; }
#if RELEASELINUX
        internal const string ShippedProgramFilename = "bsdec-schema-gen";
#else
        internal const string ShippedProgramFilename = "BsdecSchemaGen.exe";
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
            Option aboutOption = new("about", null);
            Option licenseOption = new("license", null);
            Option readerOption = new("reader", 'r');
            Option writerOption = new("writer", 'w');
            List<Option> options = new()
            {
                helpOption,
                aboutOption,
                licenseOption,
                readerOption,
                writerOption
            };
            string? className = null;
            string? assembly = null;
            string? outputFile = null;
            string? readerName = null;
            string? writerName = null;

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
                else if (readerOption.set && readerName == null)
                {
                    readerName = arg;
                }
                else if (writerOption.set && writerName == null)
                {
                    writerName = arg;
                }
                else if (className == null)
                {
                    className = arg;
                }
                else if (assembly == null)
                {
                    assembly = arg;
                }
                else if (outputFile == null)
                {
                    outputFile = arg;
                }
                else
                {
                    return FailHelpfully();
                }
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

            if (helpOption.set)
            {
                Help.ShowHelpText(false);
                return 0;
            }

            return SchemaGenerator.Run(assembly, outputFile, className, readerName, writerName);
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
