using System;
using System.Collections.Generic;
using static cslox.TokenType;

namespace cslox
{
    internal class Scanner
    {
        private string _source;
        private int _pos;
        private int _start;

        bool Done => _pos >= _source.Length;

        public Scanner(string source)
        {
            _source = source;
        }

        internal IEnumerable<Token> Scan()
        {
            while(!Done)
            {
                _start = _pos;
                var token = NextToken();
                if (token != null)
                    yield return token;
            }
            yield return new Token(EOF, "<EOF>", -1);
        }

        private Token NextToken()
        {
            char c = _source[_pos++];
            switch(c)
            {
                //SINGLE TOKENS
                case '(': return MakeToken(LEFT_PAREN);
                case ')': return MakeToken(RIGHT_PAREN);
                case '{': return MakeToken(LEFT_BRACE);
                case '}': return MakeToken(RIGHT_BRACE);
                case ',': return MakeToken(COMMA);
                case '.': return MakeToken(DOT);
                case '-': return MakeToken(MINUS);
                case '+': return MakeToken(PLUS);
                case ';': return MakeToken(SEMICOLON);
                case '*': return MakeToken(STAR);
                //OPERATORS
                case '!': return MakeToken(TryParse('=') ? BANG_EQUAL : BANG);
                case '=': return MakeToken(TryParse('=') ? EQUAL_EQUAL : EQUAL);
                case '<': return MakeToken(TryParse('=') ? LESS_EQUAL: LESS);
                case '>': return MakeToken(TryParse('=') ? GREATER_EQUAL : GREATER);
                //COMMENTS
                case '/':
                    if (TryParse('/'))
                    {
                        SkipUntil('\n');
                        return null;//alt: MakeToken(COMMENT);
                    }
                    return MakeToken(SLASH);
                //WHITESPACE
                case ' ':
                case '\r':
                case '\t':
                case '\n':
                    return null;
                //DEFAULT -> ERROR
                default:
                    Error("Unexpected character.");
                    return null;//alt: MakeToken(ERROR);
            }
        }

        private void SkipUntil(char c)
        {
            while (!Done && _source[_pos++] != c);
        }

        private bool TryParse(char c)
        {
            if (Done)
                return false;
            if (_source[_pos] != c)
                return false;

            //Match! Advance pos!
            _pos++;
            return true;
        }

        private void Error(string message)
        {
            Lox.SyntaxError(_source, _start, message);
        }

        private Token MakeToken(TokenType type)
        {
            string text = _source.Substring(_start, _pos - _start);
            return new Token(type, text, _start);
        }
    }
}