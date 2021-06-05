using System;

namespace AssemblerTwo.Machine
{
    public interface IMemoryBus
    {
        public byte MemoryBusRead(int address);
        public void MemoryBusWrite(int address, byte value);
    }
}
