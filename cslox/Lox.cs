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
        //Here is the real, long-standing exit status convention for normal termination, i.e.not by signal:
        //Exit status 0: success
        //Exit status 1: "failure", as defined by the program
        //Exit status 2: command line usage error
        const int ERROR_BAD_ARGUMENTS = 2;
        const int ERROR_FAILURE = 1;

        static Scanner _scanner = new Scanner();
        static Parser _parser = new Parser();
        static Interpreter _interpreter = new Interpreter();
        static AstPrinter _printer = new AstPrinter();

        static int _error = 0;

        static void Main(string[] args)
        {
            SetCulture();
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
            try
            {
                var tokens = _scanner.Scan(source).ToList();
                Expr expression = _parser.Parse(tokens);
                //Console.WriteLine(expression.Accept(_printer));
                var result = expression.Accept(_interpreter);
                Console.WriteLine(ToLoxString(result));

            }
            catch (Scanner.ScannerError error)
            {
                Error("Scanner", source, error.Position, error.Message);
            }
            catch (Parser.ParserError error)
            {
                Error("Parser", source, error.Token.Position, error.Message);
            }
            catch (Interpreter.InterpreterError error)
            {
                Error("Runtime", source, error.Token.Position, error.Message);
            }
            finally
            {
                _error = ERROR_FAILURE;
            }
        }

        private static void Error(string prefix, string source, int _pos, string message)
        {
            IdentifyLine(source, _pos, out int lineNumber, out int lineStart, out int linePos, out int lineEnd);
            Console.WriteLine($"[line {lineNumber}] {prefix} Error: {message}");

            string context = source.Substring(lineStart, lineEnd - lineStart);
            Console.WriteLine(context);

            string errorIndicator = new string(' ', linePos) + '^';
            Console.WriteLine(errorIndicator);
        }

        private static string ToLoxString(object obj)
        {
            if (obj == null) return "nil";

            return obj.ToString();
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
