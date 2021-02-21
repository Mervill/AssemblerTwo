using System;

namespace AssemblerTwo.Machine
{
    public interface IInputOutputBus
    {
        public int MachineReadInput(int portNumber);
        public void MachineWriteOutput(int portNumber, int value);
    }
}
