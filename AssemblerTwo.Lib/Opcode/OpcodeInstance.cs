using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AssemblerTwo.Lib
{
    [DebuggerDisplay("{Opcode}")]
    public class OpcodeInstance
    {
        private OpcodeDefinition mOpcodeDef;

        public Opcode Opcode => mOpcodeDef.Name;

        public OpcodeArgumentType ArgumentType => mOpcodeDef.ArgumentType;

        public OpcodeDefinition Def => mOpcodeDef;

        private RegisterName mARegester;

        public RegisterName ARegister
        { 
            get
            {
                switch (ArgumentType)
                {
                    case OpcodeArgumentType.REG:
                    case OpcodeArgumentType.REG_IMMED:
                    case OpcodeArgumentType.REG_REG:
                    case OpcodeArgumentType.REG_REG_IMMED:
                    {
                        return mARegester;
                    }
                    default:
                    {
                        throw new OpcodeInstanceException($"Can't set #{nameof(ARegister)} due to the state of the object ({ArgumentType})!");
                    }
                }
            }
            set
            {
                switch (ArgumentType)
                {
                    case OpcodeArgumentType.REG:
                    case OpcodeArgumentType.REG_IMMED:
                    case OpcodeArgumentType.REG_REG:
                    case OpcodeArgumentType.REG_REG_IMMED:
                    {
                        mARegester = value;
                        break;
                    }
                    default:
                    {
                        throw new OpcodeInstanceException($"Can't get #{nameof(ARegister)} due to the state of the object ({ArgumentType})!");
                    }
                }
            }
        }

        private RegisterName mBRegester;

        public RegisterName BRegister
        {
            get
            {
                switch (ArgumentType)
                {
                    case OpcodeArgumentType.REG_REG:
                    case OpcodeArgumentType.REG_REG_IMMED:
                    {
                        return mBRegester;
                    }
                    default:
                    {
                        throw new OpcodeInstanceException($"Can't set #{nameof(BRegister)} due to the state of the object ({ArgumentType})!");
                    }
                }
            }
            set
            {
                switch (ArgumentType)
                {
                    case OpcodeArgumentType.REG_REG:
                    case OpcodeArgumentType.REG_REG_IMMED:
                    {
                        mBRegester = value;
                        break;
                    }
                    default:
                    {
                        throw new OpcodeInstanceException($"Can't get #{nameof(BRegister)} due to the state of the object ({ArgumentType})!");
                    }
                }
            }
        }

        private UInt16 mImmediateValue;

        public UInt16 ImmediateValue
        {
            get
            {
                switch (ArgumentType)
                {
                    case OpcodeArgumentType.IMMED:
                    case OpcodeArgumentType.REG_IMMED:
                    case OpcodeArgumentType.REG_REG_IMMED:
                    {
                        return mImmediateValue;
                    }
                    default:
                    {
                        throw new OpcodeInstanceException($"Can't set #{nameof(BRegister)} due to the state of the object ({ArgumentType})!");
                    }
                }
            }
            set
            {
                switch (ArgumentType)
                {
                    case OpcodeArgumentType.IMMED:
                    case OpcodeArgumentType.REG_IMMED:
                    case OpcodeArgumentType.REG_REG_IMMED:
                    {
                        mImmediateValue = value;
                        break;
                    }
                    default:
                    {
                        throw new OpcodeInstanceException($"Can't get #{nameof(BRegister)} due to the state of the object ({ArgumentType})!");
                    }
                }
            }
        }

        public OpcodeInstance(Opcode opcode, RegisterName? regA = null, RegisterName? regB = null, UInt16? immed = null)
        {
            mOpcodeDef = OpcodeDefinition.Get(opcode);
            switch (ArgumentType)
            {
                case OpcodeArgumentType.NONE:
                {
                    if (regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({ArgumentType})!", nameof(regB));
                    if (immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({ArgumentType})!", nameof(immed));

                    // no-op
                    break;
                }
                case OpcodeArgumentType.REG:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({ArgumentType})!", nameof(regB));
                    if (immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({ArgumentType})!", nameof(immed));

                    mARegester = regA.Value;

                    break;
                }
                case OpcodeArgumentType.IMMED:
                {
                    if (regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({ArgumentType})!", nameof(regB));
                    if (!immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({ArgumentType})!", nameof(immed));

                    mImmediateValue = immed.Value;

                    break;
                }
                case OpcodeArgumentType.REG_REG:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({ArgumentType})!", nameof(regA));
                    if (!regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({ArgumentType})!", nameof(regB));
                    if (immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({ArgumentType})!", nameof(immed));

                    mARegester = regA.Value;
                    mBRegester = regB.Value;

                    break;
                }
                case OpcodeArgumentType.REG_IMMED:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({ArgumentType})!", nameof(regB));
                    if (!immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({ArgumentType})!", nameof(immed));

                    mARegester = regA.Value;
                    mImmediateValue = immed.Value;

                    break;
                }
                case OpcodeArgumentType.REG_REG_IMMED:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({ArgumentType})!", nameof(regA));
                    if (!regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({ArgumentType})!", nameof(regB));
                    if (!immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({ArgumentType})!", nameof(immed));

                    mARegester = regA.Value;
                    mBRegester = regB.Value;
                    mImmediateValue = immed.Value;

                    break;
                }
                default:
                {
                    throw new OpcodeInstanceException($"Unknown State {ArgumentType}!");
                }
            }
        }

        public byte[] GetBytes()
        {
            switch (ArgumentType)
            {
                case OpcodeArgumentType.NONE:
                {
                    var opcodeWord = (UInt16)(mOpcodeDef.CodeHint);
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();

                    return opcodeBytes;
                }
                case OpcodeArgumentType.REG:
                {
                    var opcodeWord = (UInt16)(mOpcodeDef.CodeHint + (byte)mARegester);
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();

                    return opcodeBytes;
                }
                case OpcodeArgumentType.IMMED:
                {
                    var opcodeWord = (UInt16)(mOpcodeDef.CodeHint);
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                    var immedBytes = BitConverter.GetBytes(mImmediateValue).Reverse().ToArray();

                    return new[] { opcodeBytes[0], opcodeBytes[1], immedBytes[0], immedBytes[1] };
                }
                case OpcodeArgumentType.REG_REG:
                {
                    var opcodeWord = (UInt16)(mOpcodeDef.CodeHint);
                    opcodeWord += (UInt16)(((byte)mARegester) << 4);
                    opcodeWord += (byte)mBRegester;
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();

                    return opcodeBytes;
                }
                case OpcodeArgumentType.REG_IMMED:
                {
                    var opcodeWord = (UInt16)(mOpcodeDef.CodeHint + (byte)mARegester);
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                    var immedBytes = BitConverter.GetBytes(mImmediateValue).Reverse().ToArray();

                    return new[] { opcodeBytes[0], opcodeBytes[1], immedBytes[0], immedBytes[1] };
                }
                case OpcodeArgumentType.REG_REG_IMMED:
                {
                    var opcodeWord = (UInt16)(mOpcodeDef.CodeHint);
                    opcodeWord += (UInt16)(((byte)mARegester) << 4);
                    opcodeWord += (byte)mBRegester;
                    var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                    var immedBytes = BitConverter.GetBytes(mImmediateValue).Reverse().ToArray();

                    return new[] { opcodeBytes[0], opcodeBytes[1], immedBytes[0], immedBytes[1] };
                }
                default:
                {
                    throw new OpcodeInstanceException($"Unknown State {ArgumentType}!");
                }
            }
        }

        public string GetString(bool immedateFormatHex = true)
        {
            switch (ArgumentType)
            {
                case OpcodeArgumentType.NONE:
                {
                    return $"{Opcode}";
                }
                case OpcodeArgumentType.REG:
                {
                    var opcodeString = Opcode.ToString().PadRight(OpcodeDefinition.LogestInstructionName);
                    return $"{opcodeString} {mARegester}";
                }
                case OpcodeArgumentType.IMMED:
                {
                    var opcodeString = Opcode.ToString().PadRight(OpcodeDefinition.LogestInstructionName);
                    var immediateString = mImmediateValue.ToString((immedateFormatHex) ? "X4" : null);
                    return $"{opcodeString} {immediateString}";
                }
                case OpcodeArgumentType.REG_REG:
                {
                    var opcodeString = Opcode.ToString().PadRight(OpcodeDefinition.LogestInstructionName);
                    return $"{opcodeString} {mARegester},{mBRegester}";
                }
                case OpcodeArgumentType.REG_IMMED:
                {
                    var opcodeString = Opcode.ToString().PadRight(OpcodeDefinition.LogestInstructionName);
                    var immediateString = mImmediateValue.ToString((immedateFormatHex) ? "X4" : null);
                    return $"{opcodeString} {mARegester},{immediateString}";
                }
                case OpcodeArgumentType.REG_REG_IMMED:
                {
                    var opcodeString = Opcode.ToString().PadRight(OpcodeDefinition.LogestInstructionName);
                    var immediateString = mImmediateValue.ToString((immedateFormatHex) ? "X4" : null);
                    return $"{opcodeString} {mARegester},{mBRegester},{immediateString}";
                }
                default:
                {
                    throw new OpcodeInstanceException($"Unknown State {ArgumentType}!");
                }
            }
        }
    }
}
