using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblerTwo.Lib.Utility
{
    public class OpcodeBuilder
    {
        readonly List<DocumentChunkType> mChunksIndex;
        readonly Dictionary<int, OpcodeInstance> mOpcodeChunks;
        readonly Dictionary<int, byte[]> mBinaryChunks;

        public int Count => mChunksIndex.Count;

        public OpcodeBuilder()
        {
            mChunksIndex = new List<DocumentChunkType>();
            mOpcodeChunks = new Dictionary<int, OpcodeInstance>();
            mBinaryChunks = new Dictionary<int, byte[]>();
        }

        public void Append(Opcode opcode, RegisterName? regA = null, RegisterName? regB = null, UInt16? immed = null)
        {
            var opcodeInstance = new OpcodeInstance(opcode, regA, regB, immed);
            int objectIndex = mChunksIndex.Count;
            mChunksIndex.Add(DocumentChunkType.Instruction);
            mOpcodeChunks.Add(objectIndex, opcodeInstance);
        }

        public void Append(params byte[] bytes)
        {
            int objectIndex = mChunksIndex.Count;
            mChunksIndex.Add(DocumentChunkType.Binary);
            mBinaryChunks.Add(objectIndex, bytes);
        }

        public byte[] GetBytes()
        {
            // TODO: Should be possible to pre-caculate 
            // the final size of this byte array
            var bytes = new List<byte>();
            for (int x = 0; x < mChunksIndex.Count; x++)
            {
                switch (mChunksIndex[x])
                {
                    case DocumentChunkType.Instruction:
                    {
                        bytes.AddRange(mOpcodeChunks[x].GetBytes());
                        break;
                    }
                    case DocumentChunkType.String:
                    {
                        throw new NotImplementedException();
                        //break;
                    }
                    case DocumentChunkType.Binary:
                    {
                        bytes.AddRange(mBinaryChunks[x]);
                        break;
                    }
                }
            }
            return bytes.ToArray();
        }

        public void Clear()
        {
            mChunksIndex.Clear();
            mOpcodeChunks.Clear();
            mBinaryChunks.Clear();
        }
    }
}
