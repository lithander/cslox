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
            T VisitFunctionDeclaration(FunctionDeclaration functionDeclaration);
            T VisitIfStatement(IfStatement ifStatement);
            T VisitPrintStatement(PrintStatement printStatement);
            T VisitVarDeclaration(VarDeclaration varDeclaration);
            T VisitWhileStatement(WhileStatement whileStatement);
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

    class FunctionDeclaration : Stmt
    {
        public readonly Token Name;
        public readonly List<Token> Parameters;
        public readonly List<Stmt> Body;

        public FunctionDeclaration(Token name, List<Token> parameters, List<Stmt> body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitFunctionDeclaration(this);
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

    class VarDeclaration : Stmt
    {
        public readonly Token Name;
        public readonly Expr Initializer;

        public VarDeclaration(Token name, Expr initializer)
        {
            Name = name;
            Initializer = initializer;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitVarDeclaration(this);
        }
    }

    class WhileStatement : Stmt
    {
        public readonly Expr Condition;
        public readonly Stmt Body;

        public WhileStatement(Expr condition, Stmt body)
        {
            Condition = condition;
            Body = body;
        }

        override public T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitWhileStatement(this);
        }
    }

}
