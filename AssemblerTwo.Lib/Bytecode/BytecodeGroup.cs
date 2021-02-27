using System;
using System.Collections.Generic;

namespace AssemblerTwo.Lib
{
    public class BytecodeGroup
    {
        public Dictionary<string, int> LabelDefines;
        public Dictionary<string, List<int>> LabelRefs;
        public HashSet<int> RelocationIndicies;

        public SymbolTable SymbolTable;

        public byte[] Bytes;
    }
}
