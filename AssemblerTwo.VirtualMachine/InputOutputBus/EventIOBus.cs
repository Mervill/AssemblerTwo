using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerTwo.Machine
{
    public class EventIOBus : IInputOutputBus
    {
        public delegate int OnMachineReadInputDelegate(int portNumber);
        public delegate void OnMachineWriteOutputDelegate(int portNumber, int value);

        public OnMachineReadInputDelegate OnMachineReadInput { get; set; }
        public OnMachineWriteOutputDelegate OnMachineWriteOutput { get; set; }

        public int MachineReadInput(int portNumber)
        {
            return OnMachineReadInput?.Invoke(portNumber) ?? 0;
        }

        public void MachineWriteOutput(int portNumber, int value)
        {
            OnMachineWriteOutput?.Invoke(portNumber, value);
        }
    }
}
