using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ast_codgen
{
    class Program
    {
        static List<string> TYPES = new List<string>
        {
            "Binary   : Expr left, Token op, Expr right",
            "Grouping : Expr expression",
            "Literal  : Object value",
            "Unary    : Token op, Expr right"
        };

        private static StreamWriter _writer;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: generate_ast <output directory>");
                return;
            }

            _writer = File.CreateText(args[0]);
            WriteAST("cslox");
            _writer.Close();
        }

        private static void WriteAST(string nameSpace)
        {
            Line(0, "//Code auto-generated. Don't edit by hand! Change, build and run ast_codgen instead!");
            Line(0, "using System;");
            Line();
            Line(0, $"namespace {nameSpace}");
            Line(0, "{");
            Line(1, "class Expr {}");
            foreach (var type in TYPES)
            {
                ParseType(type, out string className, out string fields);
                WriteClass(className, fields);
            }
            Line(0, "}");
        }

        private static void ParseType(string type, out string className, out string fields)
        {
            //Example Type:  "Binary : Expr left, Token operator, Expr right"
            int i = type.IndexOf(':');
            className = type.Substring(0, i).Trim();
            fields = type.Substring(i+2);
        }

        private static void WriteClass(string className, string fields)
        {
            Line(1, $"class {className} : Expr");
            Line(1, "{");
            
            //FIELDS
            foreach (var field in fields.Split(',').Select(f => f.Trim()))
            {
                ParseField(field, out string fieldType, out string fieldName);
                Line(2, $"readonly {fieldType} {Capitalize(fieldName)};");
            }
            Line();

            //Constructor
            Line(2, $"{className}({fields})");
            Line(2, "{");
            foreach (var field in fields.Split(','))
            {
                ParseField(field, out string fieldType, out string fieldName);
                Line(3, $"{Capitalize(fieldName)} = {fieldName};");
            }
            Line(2, "}");
            Line(1, "}");
            Line();
        }

        private static string Capitalize(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        private static void ParseField(string field, out string fieldType, out string fieldName)
        {
            int i = field.LastIndexOf(' ');
            fieldType = field.Substring(0, i);
            fieldName = field.Substring(i + 1);
        }

        private static void Line(int indention, string text)
        {
            _writer.Write(new string(' ', indention * 4));
            _writer.WriteLine(text);            
        }
        
        private static void Line()
        {
            _writer.WriteLine();
        }
    }
}
