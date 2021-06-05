using System;

namespace AssemblerTwo.Machine
{
    public interface IInputOutputBus
    {
        public int IOBusRead(int portNumber);
        public void IOBusWrite(int portNumber, int value);
    }
}
