using System;

namespace AssemblerTwo.Lib
{
    public class ParserException : AssemblerException
    {
        public readonly LexicalTokenInfo LexicalTokenInfo;

        public ParserException() : base()
        {
        }

        public ParserException(string message) : base(message)
        {
        }

        public ParserException(string message, LexicalTokenInfo lexicalTokenInfo) : base(message)
        {
            LexicalTokenInfo = lexicalTokenInfo;
        }

        public ParserException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
