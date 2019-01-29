using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ast_codgen
{
    class Program
    {
        static List<string> EXPR_TYPES = new List<string>
        {
            "Assign   : Token name, Expr value",
            "Binary   : Expr left, Token op, Expr right",
            "Grouping : Expr expression",
            "Literal  : Object value",
            "Unary    : Token op, Expr right",
            "Variable : Token name"
        };

        static List<string> STMT_TYPES = new List<string>
        {
            "ExpressionStatement : Expr expression",
            "Block               : List<Stmt> statements",
            "PrintStatement      : Expr expression",
            "VarStatement        : Token name, Expr initializer"  
        };

        private static StreamWriter _writer;
        private static string _path;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: generate_ast <output directory>");
                return;
            }
            _path = args[0];
            WriteAST("cslox", "Expr", EXPR_TYPES);
            WriteAST("cslox", "Stmt", STMT_TYPES);
        }

        private static void WriteAST(string nameSpace, string baseClassName, IEnumerable<string> types)
        {
            _writer = File.CreateText(Path.Combine(_path, baseClassName + ".cs"));

            Line(0, "//Code auto-generated. Don't edit by hand! Change, build and run ast_codgen instead!");
            Line(0, "using System;");
            Line(0, "using System.Collections.Generic;");
            Line();
            Line(0, $"namespace {nameSpace}");
            WriteBaseClass(baseClassName, types);
            foreach (var type in types)
            {
                ParseType(type, out string className, out string fields);
                WriteClass(className, baseClassName, fields);
            }
            Line(0, "}");

            _writer.Close();
        }

        private static void WriteBaseClass(string baseClassName, IEnumerable<string> types)
        {
            Line(0, "{");
            Line(1, $"abstract class {baseClassName}");
            Line(1, "{");

            Line(2, "public interface Visitor<T>");
            Line(2, "{");
            foreach (var type in types)
            {
                ParseType(type, out string className, out string fields);
                Line(3, $"T Visit{className}({className} {Uncapitalize(className)});");
            }
            Line(2, "}");
            Line();

            Line(2, "abstract public T Accept<T>(Visitor<T> visitor);");
            Line(1, "}");
            Line();
        }

        private static void WriteClass(string className, string baseClassName, string fields)
        {
            Line(1, $"class {className} : {baseClassName}");
            Line(1, "{");
            
            //Fields
            foreach (var field in fields.Split(',').Select(f => f.Trim()))
            {
                ParseField(field, out string fieldType, out string fieldName);
                Line(2, $"public readonly {fieldType} {Capitalize(fieldName)};");
            }
            Line();

            //Constructor
            Line(2, $"public {className}({fields})");
            Line(2, "{");
            foreach (var field in fields.Split(','))
            {
                ParseField(field, out string fieldType, out string fieldName);
                Line(3, $"{Capitalize(fieldName)} = {fieldName};");
            }
            Line(2, "}");

            //Accept Visitor
            Line();
            Line(2, $"override public T Accept<T>(Visitor<T> visitor)");
            Line(2, "{");
            Line(3, $"return visitor.Visit{className}(this);");
            Line(2, "}");
            Line(1, "}");
            Line();
        }

        private static string Capitalize(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        private static string Uncapitalize(string input)
        {
            return input.First().ToString().ToLower() + input.Substring(1);
        }

        private static void ParseType(string type, out string className, out string fields)
        {
            //Example Type:  "Binary : Expr left, Token operator, Expr right"
            int i = type.IndexOf(':');
            className = type.Substring(0, i).Trim();
            fields = type.Substring(i + 2);
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
