using System;
using System.Collections.Generic;
using static cslox.TokenType;

namespace cslox
{
    /* LOX EXPRESSION GRAMMAR (Chapter 6.2)
    
        program         → declaration* EOF ;
        declaration     → varDecl | funDecl | statement ;
        varDecl         → "var" IDENTIFIER ( "=" expression )? ";" ;
        funDecl         → "fun" function;
        function        → IDENTIFIER "(" parameters? ")" block ;
        statement       → exprStmt | ifStmt | printStmt | returnStmt | whileStmt | forStmt | block ;
        exprStmt        → expression ";" ;
        ifStmt          → "if" "(" expression ")" statement ( "else" statement )? ;
        printStmt       → "print" expression ";" ;
        returnStmt      → "
        whileStmt       → "while" "(" expression ")" statement ;
        forStmt         → "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" expression? ")" statement ;
        block           → "{" declaration* "}" ;
        expression      → assignment ;
        assignment      → IDENTIFIER "=" assignment | logic_or ;
        logic_or        → logic_and ("or" logic_and )* ;
        logic_and       → equality ("and" equality )* ;
        equality        → comparison ( ( "!=" | "==" ) comparison )* ;
        comparison      → addition ( ( ">" | ">=" | "<" | "<=" ) addition )* ;
        addition        → multiplication ( ( "-" | "+" ) multiplication )* ;
        multiplication  → unary ( ( "/" | "*" ) unary )* ;
        unary           → ( "!" | "-" ) unary | call ;
        call            → primary ( "(" arguments? ")" )* ;
        arguments       → expression ( "," expression )* ;
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
                return VarDeclaration();

            if (TryParse(FUN))
                return FunDeclaration();

            return Statement();
        }
        
        private Stmt VarDeclaration()
        {
            if (!TryParse(IDENTIFIER))
                throw new ParserError(Current, "variable name expected.");

            Token varName = Previous;

            Expr initializer = null;
            if (TryParse(EQUAL))
                initializer = Expression();

            if (TryParse(SEMICOLON))
                return new VarDeclaration(varName, initializer);

            throw new ParserError(Current, "';' expected after variable declaration.");
        }
        
        private FunctionDeclaration FunDeclaration()
        {
            if (!TryParse(IDENTIFIER))
                throw new ParserError(Current, "variable name expected.");

            Token funName = Previous;

            List<Token> parameters = new List<Token>();
            if (!TryParse(LEFT_PAREN))
                throw new ParserError(Current, "'(' expected in function declaration after name.");
            
            if(Current.Type != RIGHT_PAREN)
            {
                do
                {
                    if (TryParse(IDENTIFIER))
                        parameters.Add(Previous);
                    else
                        throw new ParserError(Current, "parameter name expected.");
                }
                while (TryParse(COMMA));
            }

            if (!TryParse(RIGHT_PAREN))
                throw new ParserError(Current, "')' expected in function declaration after parameters.");

            if (!TryParse(LEFT_BRACE))
                throw new ParserError(Current, "'{' expected in function declaration before body.");

            var body = Block();
            return new FunctionDeclaration(funName, parameters, body.Statements);
        }


        private Stmt Statement()
        {
            if (TryParse(IF))
                return IfStatement();

            if (TryParse(WHILE))
                return WhileStatement();

            if (TryParse(FOR))
                return ForStatement();

            if (TryParse(RETURN))
                return ReturnStatement();

            if (TryParse(PRINT))
                return PrintStatement();

            if (TryParse(LEFT_BRACE))
                return Block();

            return ExpressionStatement();
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous;
            if (TryParse(SEMICOLON)) //return void
                return new ReturnStatement(keyword, null);

            Expr expression = Expression();
            if (!TryParse(SEMICOLON))
                throw new ParserError(Current, "';' expected after return value.");

            return new ReturnStatement(keyword, expression);
        }

        private Stmt ForStatement()
        {
            //forStmt         → "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" expression? ")" statement ;
            if (!TryParse(LEFT_PAREN))
                throw new ParserError(Current, "'(' expected after 'for'.");

            //initializer is optional
            Stmt initializer = null;
            if(!TryParse(SEMICOLON)) //initializer skipped?
            {
                if (TryParse(VAR))
                    initializer = VarDeclaration();
                else
                    initializer = ExpressionStatement();
            }

            //condition is optional
            Expr condition = null;
            if (!TryParse(SEMICOLON)) //condition skipped?
                condition = Expression();
            if (!TryParse(SEMICOLON))
                throw new ParserError(Current, "';' expected after 'for' loop condition.");

            //increment is also optional
            Expr increment = null;
            if (!TryParse(RIGHT_PAREN)) //increment skipped?
                increment = Expression();
            if (!TryParse(RIGHT_PAREN))
                throw new ParserError(Current, "')' expected after 'for' clauses.");

            //For is represented as Syntax-Tree build from primitives
            //{
            //    var i = 0;
            //    while (i < 10)
            //    {
            //        print i;
            //        i = i + 1;
            //    }
            //}

            Stmt body = Statement();
            //combine body and increment in the inner block
            if (increment != null)
                body = new Block(new List<Stmt>
                {
                    body,
                    new ExpressionStatement(increment)
                });

            //wrap body in while loop with condition
            if (condition != null)
                body = new WhileStatement(condition, body);

            //if there's an initializer combine it with body to form the outer block
            if (initializer != null)
                body = new Block(new List<Stmt>
                {
                    initializer,
                    body
                });

            return body;
        }

        private Stmt IfStatement()
        {
            if (!TryParse(LEFT_PAREN))
                throw new ParserError(Current, "'(' expected after 'if'.");

            Expr condition = Expression();

            if (!TryParse(RIGHT_PAREN))
                throw new ParserError(Current, "')' expected after 'if' condition.");

            Stmt thenBranch = Statement();

            Stmt elseBranch = null;
            if (TryParse(ELSE))
                elseBranch = Statement();

            return new IfStatement(condition, thenBranch, elseBranch);
        }

        private Stmt WhileStatement()
        {
            if (!TryParse(LEFT_PAREN))
                throw new ParserError(Current, "'(' expected after 'while'.");

            Expr condition = Expression();

            if (!TryParse(RIGHT_PAREN))
                throw new ParserError(Current, "')' expected after 'while' condition.");

            Stmt body = Statement();

            return new WhileStatement(condition, body);
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
            return Call();
        }

        private Expr Call()
        {
            Expr expr = Primary();
            while(TryParse(LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }

            return expr;
        }

        private Expr FinishCall(Expr expr)
        {
            var paren = Previous;

            //it's a call - parse one or more arguments
            List<Expr> arguments = new List<Expr>();
            if (!Done && Current.Type != RIGHT_PAREN)
            {
                do
                {
                    arguments.Add(Expression());
                }
                while (TryParse(COMMA));
            }

            if (!TryParse(RIGHT_PAREN))
                throw new ParserError(Current, "')' expected after function call arguments.");

            if (arguments.Count > 8)
                throw new ParserError(Current, "Cannot have more than 8 arguments."); //because the book says 8 is enough :P

            return new Call(expr, paren, arguments);
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
