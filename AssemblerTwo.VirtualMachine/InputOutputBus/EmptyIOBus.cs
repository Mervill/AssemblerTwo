using System;

namespace AssemblerTwo.Machine
{
    public class EmptyIOBus : IInputOutputBus
    {
        public virtual int IOBusRead(int portNumber)
        {
            return 0;
        }

        public virtual void IOBusWrite(int portNumber, int value)
        {
        }
    }
}
