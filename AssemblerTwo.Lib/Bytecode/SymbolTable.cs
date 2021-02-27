using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblerTwo.Lib
{
    // SymbolTable
    // - int16: Magic Number
    // - int16: Version Number
    // - int16: List Length
    //   List of all label define sites
    //     - int16: address
    // - int16: List Length
    //   List of all non-external reference sites
    //     - int16: list length
    //     List of all addresses
    //       - int16: address
    // - int16: List Length
    //   List of external reference sites
    //     - int16: string length
    //     - string: external name
    //     - int16: list length
    //     List of all addresses
    //       - int16: address
    // - int16 List Length
    //   List of label define sites marked as public
    //     - int16: define site list index
    //     - int16: string length
    //     - string: public name
    // - int16 List Length
    //   List of exta delta addresses
    //     - int16: address
    // { Debug Section }
    // - int16 List Length
    //   List of label define site names (label names)
    //     - int16: string length
    //     - string: label name

    public class SymbolTable
    {
        public const ushort MagicNumber = 0xABEF;
        public const byte VersionNumber = 0x01;
        public const byte ReleaseFlag = 0x00;
        public const byte DebugFlag = 0x02;

        public byte Flag;

        public ushort[] SymbolDefineAddresses;
        public ushort[][] NonExternalReferenceAddresses;
        public ExternalSymbol[] ExternalSymbols;
        public PublicSymbol[] PublicSymbols;
        public ushort[] ExtraDeltaAddresses;
        public string[] SymbolDefineNames;
        //DataSectionHints

        public string GetDump(bool padding = true)
        {
            var sb = new StringBuilder();

            var isDebug = Flag == DebugFlag;
            sb.AppendLine($"Debug: {isDebug}");

            var namePadLen = 0;
            if (isDebug && padding)
            {
                namePadLen = SymbolDefineNames.Max(x => x.Length);
            }

            var indexPad = (!padding) ? 0 : SymbolDefineAddresses.Length.ToString().Length;

            sb.AppendLine($"{nameof(SymbolDefineAddresses)} ({SymbolDefineAddresses.Length})");
            for (int x = 0; x < SymbolDefineAddresses.Length; x++)
            {
                var indexSection = x.ToString().PadLeft(indexPad);
                var nameSection = (!isDebug) ? string.Empty : $"{SymbolDefineNames[x].PadRight(namePadLen)}, ";
                sb.AppendLine($"{indexSection}, {nameSection}{SymbolDefineAddresses[x]:X4}");
            }

            sb.AppendLine($"{nameof(NonExternalReferenceAddresses)} ({NonExternalReferenceAddresses.Length})");
            for (int x = 0; x < SymbolDefineAddresses.Length; x++)
            {
                var indexSection = x.ToString().PadLeft(indexPad);
                var nameSection = (!isDebug) ? string.Empty : $"{SymbolDefineNames[x].PadRight(namePadLen)}, ";
                var valuesSection = NonExternalReferenceAddresses[x].Select(x => x.ToString("X4"));
                sb.AppendLine($"{indexSection}, {nameSection}{string.Join(", ", valuesSection)}");
            }

            sb.AppendLine($"{nameof(ExternalSymbols)} ({ExternalSymbols.Length})");
            sb.AppendLine($"{nameof(PublicSymbols)} ({PublicSymbols.Length})");
            sb.AppendLine($"{nameof(ExtraDeltaAddresses)} ({ExtraDeltaAddresses.Length})");

            /*if (isDebug)
            {
                sb.AppendLine($"{nameof(SymbolDefineNames)} ({SymbolDefineNames.Length})");
                for (int x = 0; x < SymbolDefineAddresses.Length; x++)
                {
                    var indexSection = x.ToString().PadLeft(indexPad);
                    var nameSection = $"{SymbolDefineNames[x].PadRight(namePadLen)}";
                    sb.AppendLine($"{indexSection}, {nameSection}");
                }
            }*/

            return sb.ToString();
        }

        public byte[] GetBytes()
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter memWriter = new BinaryWriter(memStream))
                {
                    BinWrite16(memWriter, MagicNumber);
                    //BinWrite16(memWriter, SymbolTable.VersionNumber);
                    memWriter.Write(VersionNumber);

                    var isDebug = Flag == DebugFlag;
                    memWriter.Write(Flag);

                    BinWrite16(memWriter, (ushort)SymbolDefineAddresses.Length);
                    for (int x = 0; x < SymbolDefineAddresses.Length; x++)
                    {
                        BinWrite16(memWriter, SymbolDefineAddresses[x]);
                    }

                    BinWrite16(memWriter, (ushort)NonExternalReferenceAddresses.Length);
                    for (int x = 0; x < NonExternalReferenceAddresses.Length; x++)
                    {
                        var subList = NonExternalReferenceAddresses[x];
                        BinWrite16(memWriter, (ushort)subList.Length);
                        for (int y = 0; y < subList.Length; y++)
                        {
                            BinWrite16(memWriter, subList[y]);
                        }
                    }

                    const ushort zero = 0x0000;
                    BinWrite16(memWriter, zero); // ExternalSymbols
                    BinWrite16(memWriter, zero); // PublicSymbols
                    BinWrite16(memWriter, zero); // ExtraDeltaAddresses

                    if (isDebug)
                    {
                        BinWrite16(memWriter, (ushort)SymbolDefineNames.Length);
                        for (int x = 0; x < SymbolDefineNames.Length; x++)
                        {
                            var symbolName = SymbolDefineNames[x];
                            var lst = Assembler.TxtEncoding.GetBytes(symbolName);
                            BinWrite16(memWriter, (ushort)lst.Length);
                            memWriter.Write(lst);
                        }
                    }
                }
                return memStream.ToArray();
            }
        }

        private static void BinWrite16(BinaryWriter writer, ushort value)
        {
            var bytes = BitConverter.GetBytes(value).Reverse().ToArray();
            writer.Write(bytes);
        }

        public static SymbolTable FromFile(string filename)
        {
            using (StreamReader streamReader = new StreamReader(filename))
            {
                return FromStream(streamReader.BaseStream);
            }
        }

        public static SymbolTable FromStream(Stream stream)
        {
            using (BinaryReader binReader = new BinaryReader(stream))
            {
                var newSymbolTable = new SymbolTable();

                var magicNumber = BinRead16(binReader);
                if (magicNumber != MagicNumber)
                {
                    throw new InvalidDataException($"Invalid Magic Number! Got {magicNumber}, Expected {MagicNumber}!");
                }

                var versionNumber = binReader.ReadByte();
                if (versionNumber != VersionNumber)
                {
                    throw new InvalidDataException($"Invalid Version Number! Got {versionNumber}, Expected {VersionNumber}!");
                }

                var flag = binReader.ReadByte();
                var isDebug = flag == DebugFlag;

                var lenSymbolDefineAddresses = BinRead16(binReader);
                var symbolDefineAddresses = new ushort[lenSymbolDefineAddresses];
                for (int x = 0; x < symbolDefineAddresses.Length; x++)
                {
                    symbolDefineAddresses[x] = BinRead16(binReader);
                }

                var lenNonExternalReferenceAddresses = BinRead16(binReader);
                var nonExternalReferenceAddresses = new ushort[lenNonExternalReferenceAddresses][];
                for (int x = 0; x < symbolDefineAddresses.Length; x++)
                {
                    var lenSubList = BinRead16(binReader);
                    ushort[] subList = new ushort[lenSubList];
                    for (int y = 0; y < subList.Length; y++)
                    {
                        subList[y] = BinRead16(binReader);
                    }
                    nonExternalReferenceAddresses[x] = subList;
                }

                BinRead16(binReader); // ExternalSymbols
                BinRead16(binReader); // PublicSymbols
                BinRead16(binReader); // ExtraDeltaAddresses

                newSymbolTable.Flag = flag;
                newSymbolTable.SymbolDefineAddresses = symbolDefineAddresses;
                newSymbolTable.NonExternalReferenceAddresses = nonExternalReferenceAddresses;
                newSymbolTable.ExternalSymbols = Array.Empty<ExternalSymbol>();
                newSymbolTable.PublicSymbols = Array.Empty<PublicSymbol>();
                newSymbolTable.ExtraDeltaAddresses = Array.Empty<ushort>();

                if (isDebug)
                {
                    var lenSymbolDefineNames = BinRead16(binReader);
                    var symbolDefineNames = new string[lenSymbolDefineNames];
                    for (int x = 0; x < symbolDefineAddresses.Length; x++)
                    {
                        var stringLength = BinRead16(binReader);
                        var lst = binReader.ReadBytes(stringLength);
                        symbolDefineNames[x] = Assembler.TxtEncoding.GetString(lst);
                    }

                    newSymbolTable.SymbolDefineNames = symbolDefineNames;
                }
                else
                {
                    newSymbolTable.SymbolDefineNames = Array.Empty<string>();
                }

                return newSymbolTable;
            }
        }

        private static ushort BinRead16(BinaryReader reader)
        {
            var hibyte = reader.ReadByte();
            var lobyte = reader.ReadByte();
            return (ushort)((hibyte << 8) + lobyte);
        }
    }
}
