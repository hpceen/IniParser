using System;

namespace Parser {
    internal static class Program {
        private static void Main(string[] args) {
            var parser = new IniParser(@"C:\Users\hpceen\RiderProjects\Parser\test.ini");
            Console.WriteLine(parser.ToString());
            Console.WriteLine(parser.SearchFullSection("DEBUG"));
        }
    }
}