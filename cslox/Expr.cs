//Code auto-generated. Don't edit by hand! Change, build and run ast_codgen instead!
using System;
using System.Collections.Generic;

namespace cslox
{
    abstract class Expr
    {
        public interface Visitor<T>
        {
            T VisitAssign(Assign assign);
            T VisitBinary(Binary binary);
            T VisitGrouping(Grouping grouping);
            T VisitLiteral(Literal literal);
            T VisitLogical(Logical logical);
            T VisitUnary(Unary unary);
            T VisitVariable(Variable variable);
        }

        abstract public T Accept<T>(Visitor<T> visitor);
    }

    class Assign : Expr
    {
        public readonly Token Name;
        public readonly Expr Value;

        public Assign(Token name, Expr value)
        {
            Name = name;
            Value = value;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitAssign(this);
        }
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

    class Logical : Expr
    {
        public readonly Expr Left;
        public readonly Token Op;
        public readonly Expr Right;

        public Logical(Expr left, Token op, Expr right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitLogical(this);
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

    class Variable : Expr
    {
        public readonly Token Name;

        public Variable(Token name)
        {
            Name = name;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitVariable(this);
        }
    }

}
