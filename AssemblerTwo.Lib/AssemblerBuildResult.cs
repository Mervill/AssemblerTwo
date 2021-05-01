using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerTwo.Lib
{
    public class AssemblerBuildResult
    {
        public string SourceName;
        public StringTokenInfo[] TokenizerResult;
        public LexicalTokenInfo[] LexerResult;
        public ParseResult ParseResult;
        public BytecodeGroup FinalBytes;
    }
}
