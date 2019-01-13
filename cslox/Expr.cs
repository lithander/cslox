//Code auto-generated. Don't edit by hand! Change, build and run ast_codgen instead!
using System;

namespace cslox
{
    abstract class Expr
    {
        abstract public T Accept<T>(Visitor<T> visitor);
    }

    interface Visitor<T>
    {
        T VisitBinary(Binary binary);
        T VisitGrouping(Grouping grouping);
        T VisitLiteral(Literal literal);
        T VisitUnary(Unary unary);
    }

    class Binary : Expr
    {
        readonly Expr Left;
        readonly Token Op;
        readonly Expr Right;

        Binary(Expr left, Token op, Expr right)
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
        readonly Expr Expression;

        Grouping(Expr expression)
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
        readonly Object Value;

        Literal(Object value)
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
        readonly Token Op;
        readonly Expr Right;

        Unary(Token op, Expr right)
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
