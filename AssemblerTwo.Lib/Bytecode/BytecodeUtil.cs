using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblerTwo.Lib
{
    public static class BytecodeUtil
    {
        // TODO: This should return a new SymbolTable that reflects the relocation
        // TODO: Also it's broken
        public static byte[] Relocate(BytecodeGroup codeGroup, short newBaseAddress)
        {
            if (codeGroup.SymbolTable == null)
                throw new ArgumentException($"{nameof(codeGroup)} must have a valid {nameof(SymbolTable)}!", $"{nameof(codeGroup)}");

            var symbolTable = codeGroup.SymbolTable;
            var bytes = (byte[])codeGroup.Bytes.Clone();

            for (int x = 0; x < symbolTable.NonExternalReferenceAddresses.Length; x++)
            {
                var addrList = symbolTable.NonExternalReferenceAddresses[x];
                for (int y = 0; y < addrList.Length; y++)
                {
                    var addr = addrList[y];
                    var origHiByte = bytes[addr];
                    var origLoByte = bytes[addr + 1];
                    var shortValue = (origHiByte << 8) + origLoByte;
                    var newValue = (shortValue + newBaseAddress) & short.MaxValue;
                    bytes[addr] = (byte)(newValue >> 8);
                    bytes[addr + 1] = (byte)(newValue & 0xF);
                }
            }

            return bytes;
        }
    }
}
