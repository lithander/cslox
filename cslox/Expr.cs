//Code auto-generated. Don't edit by hand! Change, build and run ast_codgen instead!
using System;

namespace cslox
{
    abstract class Expr
    {
        public interface Visitor<T>
        {
            T VisitBinary(Binary binary);
            T VisitGrouping(Grouping grouping);
            T VisitLiteral(Literal literal);
            T VisitUnary(Unary unary);
        }

        abstract public T Accept<T>(Visitor<T> visitor);
    }

    class Binary : Expr
    {
        public readonly Expr Left;
        public readonly Token Op;
        public readonly Expr Right;

        public Binary(Expr left, Token op, Expr right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitBinary(this);
        }
    }

    class Grouping : Expr
    {
        public readonly Expr Expression;

        public Grouping(Expr expression)
        {
            Expression = expression;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitGrouping(this);
        }
    }

    class Literal : Expr
    {
        public readonly Object Value;

        public Literal(Object value)
        {
            Value = value;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitLiteral(this);
        }
    }

    class Unary : Expr
    {
        public readonly Token Op;
        public readonly Expr Right;

        public Unary(Token op, Expr right)
        {
            Op = op;
            Right = right;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitUnary(this);
        }
    }

}
