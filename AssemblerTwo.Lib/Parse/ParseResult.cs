using System;
using System.Collections.Generic;

namespace AssemblerTwo.Lib
{
    public class ParseResult
    {
        public LToken<string> NameDirectiveToken;

        public Dictionary<string, LToken<string>> LabelsDefined;
        public List<LToken<string>> LabelsRefrenced;

        public List<ASTNode> SyntaxTree;

        public ParseResult()
        {
            LabelsDefined = new Dictionary<string, LToken<string>>();
            LabelsRefrenced = new List<LToken<string>>();
            SyntaxTree = new List<ASTNode>();
        }
    }
}
