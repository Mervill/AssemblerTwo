using System;

namespace AssemblerTwo.Lib
{
    public class UnknownStateException : AssemblerException
    {
        public UnknownStateException() : base()
        {
        }

        public UnknownStateException(string message) : base(message)
        {
        }

        public UnknownStateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
