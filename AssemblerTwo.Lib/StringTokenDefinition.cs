using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AssemblerTwo.Lib
{
    public enum StringToken
    {
        Unknown,
        EndOfLine,
        Whitespace,
        Comment,
        String,
        Symbol,
        HexNumber,
        Number,
        Identifier,
    }

    public class StringTokenDefinition
    {
        // Could probablly be converted to a system that can
        // read a stream character by character
        public static readonly StringTokenDefinition[] Table = new StringTokenDefinition[]
        {
            // What could still go here?
            // SignedNumber vs UnsignedNumber (maayyybeee)
            new (StringToken.EndOfLine , "^\r?\n"),
            new (StringToken.Whitespace, "^[ \t]+"),
            new (StringToken.Comment   , "^;[^\r\n]*"),
            new (StringToken.String    , "^\"[^\r\n]*?\""), // TODO: double quote escaped inside string
            //new (StringToken.Symbol    , "^[\\+\\-/*><=,:]"),
            new (StringToken.Symbol    , "^[`~!@#$%^&*()\\-+{}\\[\\]|\\:',.<>/?]"), // excluding _;"
            new (StringToken.HexNumber , "^0x([0-9A-F]+)"),
            new (StringToken.Number    , "^[0-9]+"),
            new (StringToken.Identifier, "^[A-Z_][A-Z0-9_]*"),
        };

        private readonly StringToken mToken;
        private readonly Regex mRegex;
        private int mPriority => (int)mToken;

        private StringTokenDefinition(StringToken token, string regexPattern, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            if (!regexPattern.StartsWith("^"))
                throw new ArgumentException($"{nameof(regexPattern)} must begin with the caret symbol: ^", nameof(regexPattern));
            
            mToken = token;
            mRegex = new Regex(regexPattern, regexOptions);
        }

        public (StringToken Token, String Value) Match(string inputString)
        {
#if DEBUG
            var matches = mRegex.Matches(inputString);
            if (matches.Count > 1)
                throw new AssemblerException("Token Match Conflict!");

            if (matches.Count == 0)
                return (StringToken.Unknown, null);

            return (mToken, matches[0].Value);
#elif RELEASE
            var match = mRegex.Match(inputString);
            return match.Success ? (mTokenType, match.Value) : (Token.Unknown, null);
#endif
        }

    }
}
