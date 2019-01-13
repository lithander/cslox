using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            SetCulture();
            Test();
            if(args.Length > 1)
            {
                Console.WriteLine("Usage: cslox [script]");
                Exit(ERROR_BAD_ARGUMENTS);
            }

            if (args.Length == 1)
                RunFile(args[0]);
            else
                RunPromt();
        }

        private static void SetCulture()
        {
            // Change current culture
            CultureInfo culture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private static void Test()
        {
            Expr expression = new Binary(
            new Unary(
                new Token(TokenType.MINUS, "-", null, 1),
                new Literal(123)),
            new Token(TokenType.STAR, "*", null, 1),
            new Grouping(
                new Literal(45.67)));

            Console.WriteLine(new AstPrinter().Print(expression));
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
                Exit(_error);
        }

        private static void Exit(int errorCode)
        {
            Console.Read();
            Environment.Exit(errorCode);
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            foreach(var token in scanner.Scan())
            {
                if (token.Type == TokenType.ERROR)
                    return; //abort on error!

                Console.WriteLine(token);
            }
        }

        public static void SyntaxError(string source, int _pos, string message)
        {
            IdentifyLine(source, _pos, out int lineNumber, out int lineStart, out int linePos, out int lineEnd);
            Console.WriteLine("[line " + lineNumber + "] Error: " + message);

            string context = source.Substring(lineStart, lineEnd - lineStart);
            Console.WriteLine(context);

            string errorIndicator = new string(' ', linePos) + '^';
            Console.WriteLine(errorIndicator);

            _error = ERROR_BAD_FORMAT;
        }

        private static void IdentifyLine(string source, int _pos, out int lineNumber, out int lineStart, out int linePos, out int lineEnd)
        {
            lineNumber = 1;//lines[0] == 1st line == 1
            lineStart = 0;
            for (int i = 0; i < _pos; i++)
                if (source[i] == '\n')
                {
                    lineNumber++;
                    lineStart = i + 1;
                }
            linePos = _pos - lineStart;
            lineEnd = source.IndexOf('\n', lineStart);
            if (lineEnd < 0)
                lineEnd = source.Length;
        }
    }
}
