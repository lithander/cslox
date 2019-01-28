//Code auto-generated. Don't edit by hand! Change, build and run ast_codgen instead!
using System;

namespace cslox
{
    abstract class Stmt
    {
        public interface Visitor<T>
        {
            T VisitExpressionStatement(ExpressionStatement expressionStatement);
            T VisitPrintStatement(PrintStatement printStatement);
        }

        abstract public T Accept<T>(Visitor<T> visitor);
    }

    class ExpressionStatement : Stmt
    {
        public readonly Expr Expression;

        public ExpressionStatement(Expr expression)
        {
            Expression = expression;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }

    class PrintStatement : Stmt
    {
        public readonly Expr Expression;

        public PrintStatement(Expr expression)
        {
            Expression = expression;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitPrintStatement(this);
        }
    }

}
