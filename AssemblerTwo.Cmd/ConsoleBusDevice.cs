using System;
using System.Collections.Generic;
using System.Text;

using AssemblerTwo.Machine.IOBusBridge;

namespace AssemblerTwo.Cmd
{
    public class ConsoleBusDevice : IIOBusBridgeClient
    {
        // Ports:
        // 0: Command Port
        // 1: Character Port
        public int BusRequiredPorts => 2;

        public int BusPreferBaseAddress { get; private set;  }

        public bool BusCanRelocate => true;

        private int mPortBaseAddr;

        public ConsoleBusDevice(int portBase = 0x09)
        {
            BusPreferBaseAddress = portBase;
            mPortBaseAddr = portBase;
        }

        public int BusBridgeRead(int portNumber)
        {
            var portOffset = (portNumber - mPortBaseAddr);
            switch (portOffset)
            {
                case 0:
                {
                    return 0;
                }
                case 1:
                {
                    return 0;
                }
                default:
                {
                    throw new NotImplementedException();
                }
            }
        }

        public void BusBridgeWrite(int portNumber, int value)
        {
            var portOffset = (portNumber - mPortBaseAddr);
            switch (portOffset)
            {
                case 0:
                {
                    return;
                }
                case 1:
                {
                    var character = Convert.ToChar(value & 0xFF);
                    Console.Write(character);
                    return;
                }
                default:
                {
                    throw new NotImplementedException();
                }
            }
        }

        public void Relocated(int newBaseAddress)
        {
            mPortBaseAddr = newBaseAddress;
        }
    }
}
