//Code auto-generated. Don't edit by hand! Change, build and run ast_codgen instead!
using System;
using System.Collections.Generic;

namespace cslox
{
    abstract class Stmt
    {
        public interface Visitor<T>
        {
            T VisitExpressionStatement(ExpressionStatement expressionStatement);
            T VisitBlock(Block block);
            T VisitIfStatement(IfStatement ifStatement);
            T VisitPrintStatement(PrintStatement printStatement);
            T VisitVarStatement(VarStatement varStatement);
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

    class Block : Stmt
    {
        public readonly List<Stmt> Statements;

        public Block(List<Stmt> statements)
        {
            Statements = statements;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }

    class IfStatement : Stmt
    {
        public readonly Expr Condition;
        public readonly Stmt ThenBranch;
        public readonly Stmt ElseBranch;

        public IfStatement(Expr condition, Stmt thenBranch, Stmt elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
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

    class VarStatement : Stmt
    {
        public readonly Token Name;
        public readonly Expr Initializer;

        public VarStatement(Token name, Expr initializer)
        {
            Name = name;
            Initializer = initializer;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitVarStatement(this);
        }
    }

}
