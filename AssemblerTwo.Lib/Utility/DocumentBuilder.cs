using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblerTwo.Lib
{
    public class DocumentBuilder
    {
        public const string FancyPrintColumnSep = "│";
        
        readonly List<DocumentChunkType> mChunksIndex;
        readonly Dictionary<int, OpcodeInstance> mOpcodeChunks;
        readonly Dictionary<int, byte[]> mBinaryChunks;

        public int Count => mChunksIndex.Count;

        public DocumentBuilder()
        {
            mChunksIndex = new List<DocumentChunkType>();
            mOpcodeChunks = new Dictionary<int, OpcodeInstance>();
            mBinaryChunks = new Dictionary<int, byte[]>();
        }

        public void Append(Opcode opcode, RegisterName? regA = null, RegisterName? regB = null, UInt16? immed = null)
        {
            var opcodeInstance = new OpcodeInstance(opcode, regA, regB, immed);
            Append(opcodeInstance);
        }

        public void Append(OpcodeInstance opcodeInstance)
        {
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
                        continue;
                    }
                    case DocumentChunkType.String:
                    {
                        throw new NotImplementedException();
                    }
                    case DocumentChunkType.Binary:
                    {
                        bytes.AddRange(mBinaryChunks[x]);
                        continue;
                    }
                }
            }
            return bytes.ToArray();
        }

        public IEnumerable<(int index, DocumentChunkType chunkType, object dataObject)> GetChunkEnumerable()
        {
            for (int x = 0; x < mChunksIndex.Count; x++)
            {
                switch (mChunksIndex[x])
                {
                    case DocumentChunkType.Instruction:
                    {
                        yield return (x, mChunksIndex[x], mOpcodeChunks[x]);
                        continue;
                    }
                    case DocumentChunkType.String:
                    {
                        throw new NotImplementedException();
                    }
                    case DocumentChunkType.Binary:
                    {
                        yield return (x, mChunksIndex[x], mBinaryChunks[x]);
                        continue;
                    }
                }
            }
        }

        public void Clear()
        {
            mChunksIndex.Clear();
            mOpcodeChunks.Clear();
            mBinaryChunks.Clear();
        }
    }
}
