using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblerTwo.Lib
{
    public enum ChunkType : byte
    {
        Instructions,
        String,
        Binary,
    }

    public class ChunkInfo
    {
        public ushort Address;
        public ushort Length;
        public ChunkType Type;
    }
}
