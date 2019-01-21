using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Interpreter : Expr.Visitor<object>
    {
        public object VisitBinary(Binary binary)
        {
            object left = binary.Left.Accept(this);
            object right = binary.Right.Accept(this);

            switch (binary.Op.Type)
            {
                case TokenType.MINUS:
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    return (double)left / (double)right;
                case TokenType.STAR:
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if(left is string sLeft && right is string sRight)
                        return sLeft + sRight;
                    return (double)left + (double)right;
                case TokenType.GREATER:
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    return (double)left <= (double)right;
                case TokenType.EQUAL_EQUAL:
                    return left.Equals(right);
                case TokenType.BANG_EQUAL:
                    return IsEqual(left, right);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object VisitGrouping(Grouping grouping)
        {
            return grouping.Expression.Accept(this);
        }

        public object VisitLiteral(Literal literal)
        {
            return literal.Value;
        }

        public object VisitUnary(Unary unary)
        {
            object right = unary.Right.Accept(this);
            switch (unary.Op.Type)
            {
                case TokenType.MINUS:
                    return -(double)right;
                case TokenType.BANG:
                    return !IsTrue(right);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool IsTrue(object value)
        {
            if (value == null)
                return false;
            if (value is bool b)
                return b;
            return true;
        }

        private static object IsEqual(object left, object right)
        {
            if (left == null && right == null) return true;
            if (left == null) return false;
            return !left.Equals(right);
        }

    }
}
