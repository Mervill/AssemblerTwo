using System;

namespace AssemblerTwo.Machine.IOBusBridge
{
    public interface IIOBusBridgeClient
    {
        public int BusRequiredPorts { get; }
        public int BusPreferBaseAddress { get; }
        public bool BusCanRelocate { get; }

        public void Relocated(int newBaseAddress);
        
        public int BusBridgeRead(int portNumber);
        
        public void BusBridgeWrite(int portNumber, int value);
    }
}
