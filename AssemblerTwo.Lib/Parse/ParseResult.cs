using System;
using System.Collections.Generic;

namespace AssemblerTwo.Lib
{
    public class ParseResult
    {
        public LToken<string> NameDirectiveToken;

        public List<ASTNode> SyntaxTree;

        public ParseResult()
        {
            SyntaxTree = new List<ASTNode>();
        }
    }
}
