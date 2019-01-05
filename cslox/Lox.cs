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
            }
        }

        private static void RunFile(string path)
        {
            Run(File.ReadAllText(path));
        }

        private static void Run(string script)
        {
            Console.WriteLine(script);
        }
    }
}
