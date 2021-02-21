using System;

namespace AssemblerTwo.Lib
{
    [System.Diagnostics.DebuggerDisplay("{Token}")]
    public class LexicalTokenInfo
    {
        public LexicalToken Token { get; set; }
        public StringTokenInfo[] StringTokens { get; set; }

        public LexicalTokenInfo(LexicalToken token, params StringTokenInfo[] stringTokens)
        {
            Token = token;
            StringTokens = stringTokens;
        }
    }
}
