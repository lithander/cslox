using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Interpreter : Expr.Visitor<object>, Stmt.Visitor<bool>
    {
        private Environment _env = new Environment();

        public class InterpreterError : Exception
        {
            public readonly Token Token;

            public InterpreterError(Token token, string message) : base(message)
            {
                Token = token;
            }
        }

        public object VisitBinary(Binary binary)
        {
            switch (binary.Op.Type)
            {
                case TokenType.MINUS:
                    return EvalBinary(binary, (l, r) => l - r);
                case TokenType.SLASH:
                    return EvalBinary(binary, (l, r) => l / r);
                case TokenType.STAR:
                    return EvalBinary(binary, (l, r) => l * r);
                case TokenType.PLUS:
                    return EvalBinaryPlus(binary);
                case TokenType.GREATER:
                    return EvalBinary(binary, (l, r) => l > r);
                case TokenType.GREATER_EQUAL:
                    return EvalBinary(binary, (l, r) => l >= r);
                case TokenType.LESS:
                    return EvalBinary(binary, (l, r) => l < r);
                case TokenType.LESS_EQUAL:
                    return EvalBinary(binary, (l, r) => l <= r);
                case TokenType.EQUAL_EQUAL:
                    return IsBinaryEqual(binary);
                case TokenType.BANG_EQUAL:
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
                case TokenType.MINUS:
                    if(right is double dRight)
                        return -dRight;
                    throw new InterpreterError(unary.Op, "Operand must be a number!");
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
            if (left is string sLeft && right is string sRight)
                return sLeft + sRight;
            if (left is double dLeft && right is double dRight)
                return dLeft + dRight;
            throw new InterpreterError(binary.Op, "Operands must be two numbers or two strings.");
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

        public bool VisitVarStatement(VarStatement varStatement)
        {
            string name = varStatement.Name.Lexeme;
            object value = varStatement.Initializer.Accept(this);
            _env.Declare(name, value);
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
    }   
}
