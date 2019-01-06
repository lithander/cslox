using System;
using System.Collections.Generic;

namespace cslox
{
    internal class Scanner
    {
        private string source;

        public Scanner(string source)
        {
            this.source = source;
        }

        internal IEnumerable<Token> Scan()
        {
            yield return new Token(TokenType.EOF, "<EOF>", -1);
        }
    }
}