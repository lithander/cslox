using System;
using System.Collections.Generic;
using static cslox.TokenType;

namespace cslox
{
    /* LOX EXPRESSION GRAMMAR (Chapter 6.2)
    
        program         → declaration* EOF ;
        declaration     → varDecl | statement ;
        varDecl         → "var" IDENTIFIER ( "=" expression )? ";" ;
        statement       → exprStmt | ifStmt | printStmt | block ;
        exprStmt        → expression ";" ;
        ifStmt          → "if" "(" expression ")" statement ( "else" statement )? ;
        printStmt       → "print" expression ";" ;
        block           → "{" declaration* "}" ;
        expression      → assignment ;
        assignment      → IDENTIFIER "=" assignment | logic_or ;
        logic_or        → logic_and ("or" logic_and )* ;
        logic_and       → equality ("and" equality )* ;
        equality        → comparison ( ( "!=" | "==" ) comparison )* ;
        comparison      → addition ( ( ">" | ">=" | "<" | "<=" ) addition )* ;
        addition        → multiplication ( ( "-" | "+" ) multiplication )* ;
        multiplication  → unary ( ( "/" | "*" ) unary )* ;
        unary           → ( "!" | "-" ) unary | primary ;
        primary         → NUMBER | STRING | "false" | "true" | "nil"
                        | "(" expression ")" | IDENTIFIER ;
    */

    class Parser
    {
        public class ParserError : Exception
        {
            public readonly Token Token;

            public ParserError(Token token, string message) : base(message)
            {
                Token = token;
            }
        }

        static readonly TokenType[] EQUALITY_OPS = new[] { BANG_EQUAL, EQUAL_EQUAL };
        static readonly TokenType[] COMPARISION_OPS = new[] { GREATER, GREATER_EQUAL, LESS, LESS_EQUAL };
        static readonly TokenType[] ADDITION_OPS = new[] { MINUS, PLUS };
        static readonly TokenType[] MULTIPLICATION_OPS = new[] { SLASH, STAR };
        static readonly TokenType[] UNARY_OPS = new[] { BANG, MINUS };

        List<Token> _tokens = null;
        int _pos = 0;
        Token Current => _tokens[_pos];
        Token Previous => _tokens[_pos - 1];
        bool Done => _pos >= _tokens.Count || Current.Type == EOF;

        public bool EvalAndPrintUnterminatedExpression = false;

        public Parser()
        {
        }

        public IEnumerable<Stmt> Parse(List<Token> tokens)
        {
            _pos = 0;
            _tokens = tokens;
            while (!Done)
                yield return Declaration();
        }

        private Stmt Declaration()
        {
            if (TryParse(VAR))
                return VarStatement();

            return Statement();
        }

        private Stmt Statement()
        {
            if (TryParse(IF))
                return IfStatement();

            if (TryParse(PRINT))
                return PrintStatement();

            if (TryParse(LEFT_BRACE))
                return Block();

            return ExpressionStatement();
        }

        private Stmt IfStatement()
        {
            if (!TryParse(LEFT_PAREN))
                throw new ParserError(Current, "'(' expected after if.");

            Expr condition = Expression();

            if (!TryParse(RIGHT_PAREN))
                throw new ParserError(Current, "')' expected after if condition.");

            Stmt thenBranch = Statement();

            Stmt elseBranch = null;
            if (TryParse(ELSE))
                elseBranch = Statement();

            return new IfStatement(condition, thenBranch, elseBranch);
        }

        private Block Block()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!Done && Current.Type != RIGHT_BRACE)
                statements.Add(Declaration());

            if (TryParse(RIGHT_BRACE))
                return new Block(statements);

            throw new ParserError(Current, "'}' expected after block.");
        }

        private Stmt VarStatement()
        {
            if (!TryParse(IDENTIFIER))
                throw new ParserError(Current, "variable name expected.");

            Token varName = Previous;

            Expr initializer = null;
            if (TryParse(EQUAL))
                initializer = Expression();

            if (TryParse(SEMICOLON))
                return new VarStatement(varName, initializer);

            throw new ParserError(Current, "';' expected after variable declaration.");
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();

            if (TryParse(SEMICOLON))
                return new ExpressionStatement(expr);

            //Instead of throwing an error we may convert the dangling Expression into a PrintStatement:
            if(EvalAndPrintUnterminatedExpression)
                return new PrintStatement(expr);

            throw new ParserError(Current, "';' expected after expression.");
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();

            if (TryParse(SEMICOLON))
                return new PrintStatement(value);

            throw new ParserError(Current,  "';' expected after value.");
        }

        private Expr Expression()
        {
            //→ assignment;
            return Assignment();
        }

        private Expr Assignment()
        {
            Expr expr = Or();

            if (TryParse(EQUAL))
            {
                Token equals = Previous;
                Expr value = Assignment();

                if (expr is Variable variable)
                {
                    Token name = variable.Name;
                    return new Assign(name, value);
                }

                throw new ParserError(equals, "assignment target is invalid!");
            }

            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();
            while (TryParse(OR))
            {
                Token op = Previous;
                Expr right = And();
                expr = new Logical(expr, op, right);
            }
            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();
            while (TryParse(AND))
            {
                Token op = Previous;
                Expr right = Equality();
                expr = new Logical(expr, op, right);
            }
            return expr;
        }

        private Expr Equality()
        {
            //equality       → comparison(("!=" | "==") comparison) * ;
            Expr expr = Comparision();
            while(TryParse(EQUALITY_OPS))
            {
                Token op = Previous;
                Expr right = Comparision();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Comparision()
        {
            //comparison     → addition((">" | ">=" | "<" | "<=") addition) * ;
            Expr expr = Addition();
            while (TryParse(COMPARISION_OPS))
            {
                Token op = Previous;
                Expr right = Addition();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Addition()
        {
            //addition       → multiplication(("-" | "+") multiplication) * ;
            Expr expr = Multiplication();
            while (TryParse(ADDITION_OPS))
            {
                Token op = Previous;
                Expr right = Multiplication();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Multiplication()
        {
            //multiplication → unary(("/" | "*") unary) * ;
            Expr expr = Unary();
            while (TryParse(MULTIPLICATION_OPS))
            {
                Token op = Previous;
                Expr right = Unary();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Unary()
        {
            //unary → ( "!" | "-" ) unary
            //        | primary;
            if(TryParse(UNARY_OPS))
            {
                Token op = Previous;
                Expr right = Unary();
                return new Unary(op, right);
            }
            return Primary();
        }

        private Expr Primary()
        {
            //primary → NUMBER | STRING | "false" | "true" | "nil"
            //          | IDENTIFIER | "(" expression ")";
            if(TryParseLiteral(out Literal literal))
                return literal;

            if (TryParse(IDENTIFIER))
                return new Variable(Previous);

            if (TryParse(LEFT_PAREN))
            {
                Expr expr = Expression();
                if (TryParse(RIGHT_PAREN))
                    return new Grouping(expr);

                throw new ParserError(Current, "')' expected!");
            }

            throw new ParserError(Current, "Expression expected!");
        }

        //Utility
        private bool TryParseLiteral(out Literal literal)
        {
            literal = null;

            if (Done)
                return false;

            switch (Current.Type)
            {
                case NUMBER:
                case STRING:
                    literal = new Literal(Current.Literal); break;
                case FALSE:
                    literal = new Literal(false); break;
                case TRUE:
                    literal = new Literal(true); break;
                case NIL:
                    literal = new Literal(null); break;
                default:
                    return false;
            }

            //Match! Advance!
            _pos++;
            return true;
        }

        private bool TryParse(TokenType type)
        {
            if (Done)
                return false;

            if (Current.Type != type)
                return false;

            //Match! Advance pos!
            _pos++;
            return true;
        }

        private bool TryParse(TokenType[] tokens)
        {
            if (Done)
                return false;

            
            if (Array.IndexOf(tokens, Current.Type) == -1)
                return false;

            //Match! Advance pos!
            _pos++;
            return true;
        }
    }
}
