using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblerTwo.Machine
{
    public class DefaultMemoryBus : IMemoryBus
    {
        readonly byte[] mMemory = new byte[0x10000];

        public DefaultMemoryBus()
        {
        }

        public virtual byte MemoryBusRead(int address)
        {
            return mMemory[address & 0xFFFF];
        }

        public virtual void MemoryBusWrite(int address, byte value)
        {
            mMemory[address & 0xFFFF] = value;
        }

        public byte Read(int address)
        {
            return mMemory[address & 0xFFFF];
        }

        public void Write(int address, byte value)
        {
            mMemory[address & 0xFFFF] = value;
        }

        public ushort Read16(int address)
        {
            return (ushort)((Read(address) << 8) | Read(address + 1));
        }

        public void Write16(int address, ushort value)
        {
            Write(address, (byte)(value >> 8));
            Write(address + 1, (byte)(value & 0xFF));
        }

        public void CopyInto(byte[] source, int memoryStartIndex)
        {
            CopyInto(source, memoryStartIndex, source.Length);
        }

        public void CopyInto(byte[] source, int memoryStartIndex, int sourceLength)
        {
            Array.Copy(source, 0, mMemory, memoryStartIndex, sourceLength);
        }
    }
}
