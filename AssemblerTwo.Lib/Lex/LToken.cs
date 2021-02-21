using System;

namespace AssemblerTwo.Lib
{
    [System.Diagnostics.DebuggerDisplay("{Token} {Value,nq}")]
    public class LToken<T> : LexicalTokenInfo
    {
        public T Value { get; set; }

        public LToken(LexicalToken token, T genericValue, params StringTokenInfo[] stringTokens) : base (token, stringTokens)
        {
            Value = genericValue;
        }

    }
}
