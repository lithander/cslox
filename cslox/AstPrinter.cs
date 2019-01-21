using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class AstPrinter : Expr.Visitor<string>
    {
        public string VisitBinary(Binary binary)
        {
            return Parenthesize(binary.Op.Lexeme, binary.Left, binary.Right);
        }

        public string VisitGrouping(Grouping grouping)
        {
            return Parenthesize("Group", grouping.Expression);
        }

        public string VisitLiteral(Literal literal)
        {
            if (literal.Value == null)
                return "nil";
            return literal.Value.ToString();
        }

        public string VisitUnary(Unary unary)
        {
            return Parenthesize(unary.Op.Lexeme, unary.Right);
        }

        private string Parenthesize(string name, params Expr[] expressions)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(").Append(name);
            foreach(var exp in expressions)
            {
                builder.Append(' ');
                builder.Append(exp.Accept(this));//apply visitor recursively
            }
            builder.Append(')');
            return builder.ToString();
        }
    }
}
