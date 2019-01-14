using System;
using System.Collections.Generic;
using static cslox.TokenType;

namespace cslox
{
    internal class Scanner
    {
        private readonly static Dictionary<string, TokenType> KEYWORDS = new Dictionary<string, TokenType>
        {
            { "and",    AND     },
            { "class",  CLASS   },
            { "else",   ELSE    },
            { "false",  FALSE   },
            { "fun",    FUN     },
            { "for",    FOR     },
            { "if",     IF      },
            { "nil",    NIL     },
            { "or",     OR      },
            { "print",  PRINT   },
            { "return", RETURN  },
            { "super",  SUPER   },
            { "this",   THIS    },
            { "true",   TRUE    },
            { "var",    VAR     },
            { "while",  WHILE   }
        };

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
                case '!': return MakeToken(TryParse ('=') ? BANG_EQUAL : BANG);
                case '=': return MakeToken(TryParse('=') ? EQUAL_EQUAL : EQUAL);
                case '<': return MakeToken(TryParse('=') ? LESS_EQUAL: LESS);
                case '>': return MakeToken(TryParse('=') ? GREATER_EQUAL : GREATER);
                //WHITESPACES
                case ' ':
                case '\r':
                case '\t':
                case '\n':
                    return null;
                //SLASH vs COMMENT
                case '/':
                    if (!TryParse('/'))
                        return MakeToken(SLASH);
                    SkipPast('\n');
                    return null;//alt: MakeToken(COMMENT);
                //STRINGS
                case '"':
                    if (SkipPast('"'))
                        return MakeString();
                    Error("Unterminated string.");
                    return null;
                //DEFAULT -> ERROR
                default:
                    if (IsDigit(c))
                    {
                        Skip(IsDigit);
                        if(TryParse('.'))
                            Skip(IsDigit);

                        return MakeNumber();
                    }
                    else if(IsAlpha(c))
                    {
                        Skip(IsAlpha);
                        return MakeIdentifier();
                    }
                    Error("Unexpected character.");
                    return null;//alt: MakeToken(ERROR);
            }
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void Skip(Predicate<char> condition)
        {
            while (!Done && condition(_source[_pos]))
                _pos++;
        }

        private bool SkipPast(char c)
        {
            while (!Done)
                if (_source[_pos++] == c)
                    return true;

            return false;
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

        private Token MakeString()
        {
            string text = _source.Substring(_start, _pos - _start);
            return new Token(STRING, text, text, _start);
        }

        private Token MakeNumber()
        {
            string text = _source.Substring(_start, _pos - _start);
            var number = double.Parse(text);
            return new Token(NUMBER, text, number, _start);
        }
        
        private Token MakeIdentifier()
        {
            string text = _source.Substring(_start, _pos - _start);
            if (KEYWORDS.TryGetValue(text, out TokenType type))
                return new Token(type, text, _start);
            else
                return new Token(IDENTIFIER, text, _start);
        }

    }
}