using System;

namespace AssemblerTwo.Lib
{
    public class OpcodeInstanceException : Exception
    {
        public OpcodeInstanceException() : base()
        {
        }

        public OpcodeInstanceException(string message) : base(message)
        {
        }

        public OpcodeInstanceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
