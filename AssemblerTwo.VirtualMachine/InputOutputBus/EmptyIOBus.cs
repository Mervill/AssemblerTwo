using System;

namespace AssemblerTwo.Machine
{
    public class EmptyIOBus : IInputOutputBus
    {
        public virtual int MachineReadInput(int portNumber)
        {
            return 0;
        }

        public virtual void MachineWriteOutput(int portNumber, int value)
        {
        }
    }
}
