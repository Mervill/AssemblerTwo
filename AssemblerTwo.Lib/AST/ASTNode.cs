using System;
using System.Collections.Generic;

namespace AssemblerTwo.Lib
{
    public abstract class ASTNode
    {
        public const int kLabelPad = 6;
        public const int kNamePad = 6;

        public List<LexicalTokenInfo> LexTokens;
        public LToken<string> Label;
        public ASTNodeType Type;

        public override string ToString()
        {
            return ToString(false);
        }

        public abstract string ToString(bool padding);
    }
}
