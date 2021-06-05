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

        public override int IOBusRead(int portNumber)
        {
            return base.IOBusRead(portNumber);
        }

        public override void IOBusWrite(int portNumber, int value)
        {
            if (portNumber == mWriterPortCode)
            {
                var character = Convert.ToChar(value & 0xFF);
                mWriterAction(new string(character, 1));
            }
            base.IOBusWrite(portNumber, value);
        }
    }
}
