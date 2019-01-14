using System;
using System.Collections.Generic;
using static cslox.TokenType;

namespace cslox
{
    /* LOX EXPRESSION GRAMMAR (Chapter 6.2)
    
        expression     → equality ;
        equality       → comparison ( ( "!=" | "==" ) comparison )* ;
        comparison     → addition ( ( ">" | ">=" | "<" | "<=" ) addition )* ;
        addition       → multiplication ( ( "-" | "+" ) multiplication )* ;
        multiplication → unary ( ( "/" | "*" ) unary )* ;
        unary          → ( "!" | "-" ) unary
                       | primary ;
        primary        → NUMBER | STRING | "false" | "true" | "nil"
                       | "(" expression ")" ;
    */

    class Parser
    {
        static readonly TokenType[] EQUALITY_OPS = new[] { BANG_EQUAL, EQUAL_EQUAL };
        static readonly TokenType[] COMPARISION_OPS = new[] { GREATER, GREATER_EQUAL, LESS, LESS_EQUAL };
        static readonly TokenType[] ADDITION_OPS = new[] { MINUS, PLUS };
        static readonly TokenType[] MULTIPLICATION_OPS = new[] { SLASH, STAR };
        static readonly TokenType[] UNARY_OPS = new[] { BANG, MINUS };

        List<Token> _tokens = new List<Token>();
        int _pos = 0;
        
        bool Done => _pos >= _tokens.Count || _tokens[_pos].Type == EOF;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Expr Expression()
        {
            //→ equality;
            return Equality();
        }

        private Expr Equality()
        {
            //equality       → comparison(("!=" | "==") comparison) * ;
            Expr expr = Comparision();
            while(TryParse(EQUALITY_OPS))
            {
                Token op = Previous();
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
                Token op = Previous();
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
                Token op = Previous();
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
                Token op = Previous();
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
                Token op = Previous();
                Expr right = Unary();
                return new Unary(op, right);
            }
            return Primary();
        }

        private Expr Primary()
        {
            //primary → NUMBER | STRING | "false" | "true" | "nil"
            //          | "(" expression ")";
            if (Done)
                return null;

            var t = _tokens[_pos++];
            switch (t.Type)
            {
                case NUMBER:
                case STRING:
                    return new Literal(t.Literal);
                case FALSE: return new Literal(false);
                case TRUE: return new Literal(true);
                case NIL: return new Literal(null);
            }

            //Rollback
            _pos--;

            //Match! Advance pos!
            if (TryParse(LEFT_PAREN))
            {
                Expr expr = Expression();
                if (TryParse(RIGHT_PAREN))
                    return new Grouping(expr);

                throw new Exception("Expect ')' after expression.");
            }

            throw new Exception("Expected expression!");
        }

        //Utility
        private bool TryParse(TokenType token)
        {
            if (Done)
                return false;

            if (_tokens[_pos].Type != token)
                return false;

            //Match! Advance pos!
            _pos++;
            return true;
        }

        private bool TryParse(TokenType[] tokens)
        {
            if (Done)
                return false;

            var nextToken = _tokens[_pos].Type;
            for(int i = 0; i < tokens.Length; i++)
                if (nextToken == tokens[i])
                {
                    //Match! Advance pos!
                    _pos++;
                    return true;
                }

            return false;
        }

        private Token Previous()
        {
            return _tokens[_pos - 1];
        }
    }
}
