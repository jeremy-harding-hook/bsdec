using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bsdec
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.Error.WriteLine("Hi! Here's an error!");
            Task.Delay(10000).Wait();
            Console.WriteLine("Hello again!");
            Console.Error.WriteLine("Uh-oh, another error!");
        }

        static void HandleArgs(List<string> args)
        {
            // Just putting this here for now because I'll probably need the logic but not for the schemaGen interface
            Option helpOption = new("help", 'h');
            string? assembly = null;

            Regex multiflagFinder = shortFlags();
            Regex longFlagFinder = longFlags();
            foreach (string arg in args)
            {
                if (multiflagFinder.IsMatch(arg))
                {
                    foreach (char flag in arg)
                    {
                        if (flag == helpOption.flag)
                            helpOption.set = true;
                        else
                            helpOption.set = true;
                    }
                }
                else if (longFlagFinder.IsMatch(arg))
                {
                    string bareFlag = arg[2..];
                    if (bareFlag == helpOption.name)
                        helpOption.set = true;
                    else
                        helpOption.set = true;
                }
                else if (assembly != null)
                {
                    helpOption.set = true;
                }
                else
                {
                    assembly = arg;
                }
            }
        }

        struct Option
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

        [GeneratedRegex("-[a-zA-Z]+")]
        private static partial Regex shortFlags();

        [GeneratedRegex("--[a-zA-Z]+")]
        private static partial Regex longFlags();
    }
}