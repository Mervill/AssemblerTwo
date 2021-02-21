using System;

namespace AssemblerTwo.Lib
{
    public class AssemblerException : Exception
    {
        public AssemblerException() : base()
        {
        }

        public AssemblerException(string message) : base(message)
        {
        }

        public AssemblerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
