using System;
using System.Collections.Generic;

namespace AssemblerTwo.Lib.AST
{
    public class AbstractSyntaxTree
    {
        LToken<string> Name;
        List<ASTNode> Nodes;
    }
}
