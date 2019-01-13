//Code auto-generated. Don't edit by hand! Change, build and run ast_codgen instead!
using System;

namespace cslox
{
    class Expr {}
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
    }

    class Grouping : Expr
    {
        readonly Expr Expression;

        Grouping(Expr expression)
        {
            Expression = expression;
        }
    }

    class Literal : Expr
    {
        readonly Object Value;

        Literal(Object value)
        {
            Value = value;
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
    }

}
