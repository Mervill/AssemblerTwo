using System;

namespace AssemblerTwo.Lib
{
    [System.Diagnostics.DebuggerDisplay("{Token}, {Value}")]
    public class StringTokenInfo
    {
        public StringToken Token { get; }
        public string Value { get; }
        public int ValueLength => Value?.Length ?? 0;

        public int GlobalCharacterIndex { get; set; }
        public int LineIndex { get; set; }
        public int LineCharacterIndex { get; set; }
        
        public StringTokenInfo()
        {
            Token = StringToken.Unknown;
            Value = null;
        }

        public StringTokenInfo(StringToken token, String value)
        {
            Token = token;
            Value = value;
        }

        public string ToString(bool showIndices = true, bool hideEoLCharacters = true, bool csvFormat = true)
        {
            var sep = csvFormat ? "," : string.Empty;
            var indices = showIndices ? $"(l:{LineIndex,3} c:{LineCharacterIndex,3} p:{GlobalCharacterIndex,4}){sep} " : string.Empty;
            var value = Value.Replace("\t", "\\t")
                             .Replace("\r", "\\r")
                             .Replace("\n", "\\n");
            if (Token == StringToken.EndOfLine && hideEoLCharacters)
            {
                value = "{EndOfLine}";
            }
            else
            {
                value = $"`{value}`";
            }
            return $"{indices}{Token,-10}{sep} {value}";
        }
    }
}
