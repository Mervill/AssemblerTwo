using System;

namespace AssemblerTwo.Machine
{
    public interface IMemoryBus
    {
        public byte MachineRead(int address);
        public void MachineWrite(int address, byte value);
    }
}
