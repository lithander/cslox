namespace cslox
{
    enum TokenType
    {
        // Single-character tokens.                      
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

        // One or two character tokens.                  
        BANG, BANG_EQUAL,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,

        // Literals.                                     
        IDENTIFIER, STRING, NUMBER,

        // Keywords.                                     
        AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
        PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

        EOF,

        ERROR, COMMENT
    }

    internal class Token
    {
        public readonly TokenType Type;
        public readonly string Lexeme;
        public readonly object Literal;
        public readonly int Position;

        public Token(TokenType type, string lexeme, object literal, int positon)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Position = positon;
        }

        public Token(TokenType type, string lexeme, int positon)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = null;
            Position = positon;
        }

        public override string ToString()
        {
            if (Literal != null)
                return Type + " " + Lexeme + " " + Literal;
            else
                return Type + " " + Lexeme;
        }

    }
}