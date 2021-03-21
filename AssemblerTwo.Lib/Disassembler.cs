using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblerTwo.Lib
{
    public class Disassembler
    {
        /// <summary>
        /// Tries to disassemble all given bytes as instructions
        /// </summary>
        /// <param name="instructionBytes"></param>
        public static void PrimitiveDisassembleInstructions(DocumentBuilder outDoc, byte[] instructionBytes)
        {
            var byteList = new List<byte>(instructionBytes);
            var currentBinarySection = new List<byte>();
            while (byteList.Count != 0)
            {
                if (byteList.Count >= 2)
                {
                    var opcodeWord = (ushort)(byteList[0] << 8 | byteList[1]);
                    var opcodeInstance = OpcodeDefinition.DecodeInstance(opcodeWord);
                    if (opcodeInstance != null)
                    {
                        if (currentBinarySection.Count != 0)
                        {
                            outDoc.Append(currentBinarySection.ToArray());
                            currentBinarySection.Clear();
                        }

                        switch (opcodeInstance.ArgumentType)
                        {
                            case OpcodeArgumentType.IMMED:
                            case OpcodeArgumentType.REG_IMMED:
                            case OpcodeArgumentType.REG_REG_IMMED:
                            {
                                if (byteList.Count >= 4)
                                {
                                    ushort immediateValue = (ushort)(byteList[2] << 8 | byteList[3]);
                                    opcodeInstance.ImmediateValue = immediateValue;
                                    outDoc.Append(opcodeInstance);
                                    byteList.RemoveRange(0, 4);
                                }
                                else
                                {
                                    // if there's no immediate bytes because we are out of bytes,
                                    // just assume it's a binary chunk
                                    //outDoc.Append(byteList[0], byteList[1]);
                                    currentBinarySection.AddRange(new[] { byteList[0], byteList[1] });
                                    byteList.RemoveRange(0, 2);
                                }
                                break;
                            }
                            default:
                            {
                                outDoc.Append(opcodeInstance);
                                byteList.RemoveRange(0, 2);
                                break;
                            }
                        }
                    }
                    else
                    {
                        currentBinarySection.AddRange(new []{ byteList[0], byteList[1] });
                        byteList.RemoveRange(0, 2);
                    }
                }
                else
                {
                    //outDoc.Append(byteList[0]);
                    currentBinarySection.Add(byteList[0]);
                    byteList.RemoveRange(0, 1);
                }
            }
        }
    }
}
