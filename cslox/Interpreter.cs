﻿using System;
using System.Collections.Generic;
using static cslox.TokenType;

namespace cslox
{
    class Interpreter : Expr.Visitor<object>, Stmt.Visitor<bool>
    {
        public class InterpreterError : Exception
        {
            public readonly Token Token;

            public InterpreterError(Token token, string message) : base(message)
            {
                Token = token;
            }
        }

        readonly private Environment _rootEnv;
        private Environment _env;

        public Environment Globals => _rootEnv;

        public Interpreter()
        {
            _rootEnv = new Environment();
            _rootEnv.Define("clock", new StdLib.Clock());

            _env = _rootEnv;
        }

        internal void Execute(List<Stmt> body, Environment env)
        {
            var previous = _env;
            _env = env;
            try
            {
                foreach (var stmt in body)
                    stmt.Accept(this);
            }
            finally
            {
                _env = previous;
            }
        }

        public object VisitBinary(Binary binary)
        {
            switch (binary.Op.Type)
            {
                case MINUS:
                    return EvalBinary(binary, (l, r) => l - r);
                case SLASH:
                    return EvalBinary(binary, (l, r) => l / r);
                case STAR:
                    return EvalBinary(binary, (l, r) => l * r);
                case PLUS:
                    return EvalBinaryPlus(binary);
                case GREATER:
                    return EvalBinary(binary, (l, r) => l > r);
                case GREATER_EQUAL:
                    return EvalBinary(binary, (l, r) => l >= r);
                case LESS:
                    return EvalBinary(binary, (l, r) => l < r);
                case LESS_EQUAL:
                    return EvalBinary(binary, (l, r) => l <= r);
                case EQUAL_EQUAL:
                    return IsBinaryEqual(binary);
                case BANG_EQUAL:
                    return !IsBinaryEqual(binary);
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
                case MINUS:
                    if(right is double dRight)
                        return -dRight;
                    throw new InterpreterError(unary.Op, "Operand must be a number!");
                case BANG:
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

        private bool IsBinaryEqual(Binary binary)
        {
            object left = binary.Left.Accept(this);
            object right = binary.Right.Accept(this);
            if (left == null && right == null) return true;
            if (left == null) return false;
            return left.Equals(right);
        }


        private object EvalBinaryPlus(Binary binary)
        {
            object left = binary.Left.Accept(this);
            object right = binary.Right.Accept(this);
            if (left is double dLeft && right is double dRight)
                return dLeft + dRight;
            if (left is string sLeft && right is string sRight)
                return sLeft + sRight;

            //throw new InterpreterError(binary.Op, "Operands must be two numbers or two strings.");
            return left.ToString() + right.ToString();
        }

        private object EvalBinary(Binary binary, Func<double, double, object> operation)
        {
            object left = binary.Left.Accept(this);
            if (!(left is double))
                throw new InterpreterError(binary.Op, "Left operand must be a number!");

            object right = binary.Right.Accept(this);
            if (!(right is double))
                throw new InterpreterError(binary.Op, "Right operand must be a numbers!");

            return operation((double)left, (double)right);
        }

        public bool VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            object value = expressionStatement.Expression.Accept(this);
            return true;
        }

        public bool VisitPrintStatement(PrintStatement printStatement)
        {
            object value = printStatement.Expression.Accept(this);
            Lox.Print(value);
            return true;
        }

        public object VisitVariable(Variable variable)
        {
            return _env.Get(variable.Name);
        }

        public bool VisitVarDeclaration(VarDeclaration varStatement)
        {
            string name = varStatement.Name.Lexeme;
            object value = null;

            if(varStatement.Initializer != null)
                value = varStatement.Initializer.Accept(this);

            _env.Define(name, value);
            return true;
        }

        public object VisitAssign(Assign assign)
        {
            //TODO: we could do without an explicit VarStatement if we let Assign declare new variables too
            object value = assign.Value.Accept(this);
            _env.Assign(assign.Name, assign.Value.Accept(this));
            return value;
        }

        public bool VisitBlock(Block block)
        {
            var previous = _env;
            _env = new Environment(_env);
            try
            {
                foreach (var stmt in block.Statements)
                    stmt.Accept(this);
            }
            finally
            {
                _env = previous;
            }
            return true;
        }

        public bool VisitIfStatement(IfStatement ifStatement)
        {
            object condition = ifStatement.Condition.Accept(this);
            if (IsTrue(condition))
                ifStatement.ThenBranch.Accept(this);
            else
                ifStatement.ElseBranch?.Accept(this);
    
            return true;
        }

        public object VisitLogical(Logical logical)
        {
            object left = logical.Left.Accept(this);

            if (logical.Op.Type == OR && IsTrue(left))
                return left;//early out -> TRUE

            if (logical.Op.Type == AND && !IsTrue(left))
                return left;//early out -> FALSE

            object right = logical.Right.Accept(this);
            return right;
        }

        public bool VisitWhileStatement(WhileStatement whileStatement)
        {
            object condition = whileStatement.Condition.Accept(this);
            while (IsTrue(condition))
            {
                whileStatement.Body.Accept(this);
                condition = whileStatement.Condition.Accept(this);
            }
            return true;
        }

        public object VisitCall(Call call)
        {
            object callee = call.Callee.Accept(this);
            List<object> arguments = new List<object>();
            foreach (var expr in call.Arguments)
                arguments.Add(expr.Accept(this));

            if (callee is ICallable function)
            {
                if(arguments.Count == function.Arity)
                    return function.Call(this, arguments);

                throw new InterpreterError(call.Paren, $"{function.Arity} arguments expected but got {arguments.Count}!");
            }

            throw new InterpreterError(call.Paren, "Call target can not be called!");
        }

        public bool VisitFunctionDeclaration(FunctionDeclaration function)
        {
            string name = function.Name.Lexeme;
            object value = new LoxFunction(function, _env);
            _env.Define(name, value);
            return true;
        }

        public bool VisitReturnStatement(ReturnStatement returnStatement)
        {
            //TODO: the overhead of using an exception to hack stack unwinding makes me cringe :/
            object value = returnStatement.Value?.Accept(this);
            LoxFunction.Return(value);
            return true;
        }
    }   
}
