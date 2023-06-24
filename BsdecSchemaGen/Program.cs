using System;
using System.Threading.Tasks;

namespace BsdecSchemaGen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.Error.WriteLine("Hi! Here's an error!");
            Task.Delay(10000).Wait();
            Console.WriteLine("Hello again!");
            Console.Error.WriteLine("Uh-oh, another error!");
        }
    }
}