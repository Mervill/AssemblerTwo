using System;

namespace AssemblerTwo.Machine.IOBusBridge
{
    // IBusNegotiator
    // IBusCoordinator
    // IBusProvider / IBusProviderClient
    public interface IIOBusBridge : IInputOutputBus
    {
        public void Connect(IIOBusBridgeClient clientDevice);
        public void Disconnect(IIOBusBridgeClient clientDevice);
    }
}
