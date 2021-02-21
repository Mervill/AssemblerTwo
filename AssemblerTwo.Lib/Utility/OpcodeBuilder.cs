using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblerTwo.Lib.Utility
{
    public class OpcodeBuilder
    {
        const int kDefaultCapacity = 16;

        readonly List<byte> mBytes;

        public int Count => mBytes.Count;

        public OpcodeBuilder(int capacity = kDefaultCapacity)
        {
            mBytes = new List<byte>(capacity);
        }

        public void Append(Opcode opcode, RegisterName? regA = null, RegisterName? regB = null, UInt16? immed = null)
        {
            mBytes.AddRange(Create(opcode, regA, regB, immed));
        }

        public void Append(params byte[] bytes)
        {
            mBytes.AddRange(bytes);
        }

        public byte[] GetBytes()
        {
            return mBytes.ToArray();
        }

        public void Clear()
        {
            mBytes.Clear();
        }

        public static byte[] Create(Opcode opcode, RegisterName? regA = null, RegisterName? regB = null, UInt16? immed = null)
        {
            var opcodeDef = OpcodeDefinition.Get(opcode);
            switch (opcodeDef.ArgumentType)
            {
                case OpcodeArgumentType.NONE:
                {
                    if (regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({opcodeDef.ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({opcodeDef.ArgumentType})!", nameof(regB));
                    if (immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({opcodeDef.ArgumentType})!", nameof(immed));

                    var opcodeWord = (UInt16)(opcodeDef.CodeHint);
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();

                    return opcodeBytes;
                }
                case OpcodeArgumentType.REG:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({opcodeDef.ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({opcodeDef.ArgumentType})!", nameof(regB));
                    if (immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({opcodeDef.ArgumentType})!", nameof(immed));

                    var opcodeWord = (UInt16)(opcodeDef.CodeHint + (byte)regA.Value);
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();

                    return opcodeBytes;
                }
                case OpcodeArgumentType.IMMED:
                {
                    if (regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({opcodeDef.ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({opcodeDef.ArgumentType})!", nameof(regB));
                    if (!immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({opcodeDef.ArgumentType})!", nameof(immed));

                    var opcodeWord = (UInt16)(opcodeDef.CodeHint);
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                    var immedBytes = BitConverter.GetBytes(immed.Value).Reverse().ToArray();

                    return new[] { opcodeBytes[0], opcodeBytes[1], immedBytes[0], immedBytes[1] };
                }
                case OpcodeArgumentType.REG_REG:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({opcodeDef.ArgumentType})!", nameof(regA));
                    if (!regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({opcodeDef.ArgumentType})!", nameof(regB));
                    if (immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({opcodeDef.ArgumentType})!", nameof(immed));

                    var opcodeWord = (UInt16)(opcodeDef.CodeHint);
                    opcodeWord += (UInt16)(((byte)regA.Value) << 4);
                    opcodeWord += (byte)regB.Value;
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();

                    return opcodeBytes;
                }
                case OpcodeArgumentType.REG_IMMED:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({opcodeDef.ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({opcodeDef.ArgumentType})!", nameof(regB));
                    if (!immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({opcodeDef.ArgumentType})!", nameof(immed));

                    var opcodeWord = (UInt16)(opcodeDef.CodeHint + (byte)regA.Value);
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                    var immedBytes = BitConverter.GetBytes(immed.Value).Reverse().ToArray();

                    return new[] { opcodeBytes[0], opcodeBytes[1], immedBytes[0], immedBytes[1] };
                }
                case OpcodeArgumentType.REG_REG_IMMED:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({opcodeDef.ArgumentType})!", nameof(regA));
                    if (!regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({opcodeDef.ArgumentType})!", nameof(regB));
                    if (!immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({opcodeDef.ArgumentType})!", nameof(immed));

                    var opcodeWord = (UInt16)(opcodeDef.CodeHint);
                    opcodeWord += (UInt16)(((byte)regA.Value) << 4);
                    opcodeWord += (byte)regB.Value;
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                    var immedBytes = BitConverter.GetBytes(immed.Value).Reverse().ToArray();

                    return new[] { opcodeBytes[0], opcodeBytes[1], immedBytes[0], immedBytes[1] };
                }
            }

            return null;
        }

    }
}
