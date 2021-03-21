using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerTwo.Lib
{
    public class DisassemblerException : Exception
    {
        public DisassemblerException() : base()
        {
        }

        public DisassemblerException(string message) : base(message)
        {
        }

        public DisassemblerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
