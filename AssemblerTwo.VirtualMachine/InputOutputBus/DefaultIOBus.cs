using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblerTwo.Machine
{
    public class DefaultIOBus : EmptyIOBus
    {
        readonly int mWriterPortCode;
        readonly Action<string> mWriterAction;

        public DefaultIOBus(Action<string> writerAction, int writerPortCode = 0x0A)
        {
            mWriterPortCode = writerPortCode;
            mWriterAction = writerAction;
        }

        public override int MachineReadInput(int portNumber)
        {
            return base.MachineReadInput(portNumber);
        }

        public override void MachineWriteOutput(int portNumber, int value)
        {
            if (portNumber == mWriterPortCode)
            {
                var character = Convert.ToChar(value & 0xFF);
                mWriterAction(new string(character, 1));
            }
            base.MachineWriteOutput(portNumber, value);
        }
    }
}
