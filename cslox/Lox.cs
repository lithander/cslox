using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Lox
    {
        //System Error Codes (https://docs.microsoft.com/de-de/windows/desktop/Debug/system-error-codes)
        const int ERROR_BAD_ARGUMENTS = 0xA0;//160
        const int ERROR_BAD_FORMAT = 0xB;//11

        static int _error = 0;

        static void Main(string[] args)
        {
            if(args.Length > 1)
            {
                Console.WriteLine("Usage: cslox [script]");
                Environment.Exit(ERROR_BAD_ARGUMENTS);
            }

            if (args.Length == 1)
                RunFile(args[0]);
            else
                RunPromt();
        }

        private static void RunPromt()
        {
            for(;;)
            {
                Console.Write("> ");
                Run(Console.ReadLine());
                //reset to allow user to try again
                _error = 0;
            }
        }

        private static void RunFile(string path)
        {
            Run(File.ReadAllText(path));
            if (_error != 0)
                Environment.Exit(_error);
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            foreach(var token in scanner.Scan())
                Console.WriteLine(token);
        }

        public static void SyntaxError(int line, string message)
        {
            Console.WriteLine("[line " + line + "] Error: " + message);
            _error = ERROR_BAD_FORMAT;
        }
    }
}
