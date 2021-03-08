using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerTwo.Lib
{
    public class OpcodeInstance
    {
        private OpcodeDefinition mOpcodeDef;

        public Opcode Opcode => mOpcodeDef.Name;

        public OpcodeDefinition Def => mOpcodeDef;

        private RegisterName mARegester;

        public RegisterName ARegister 
        { 
            get
            {
                switch (mOpcodeDef.ArgumentType)
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
                        throw new OpcodeInstanceException($"Can't set #{nameof(ARegister)} due to the state of the object ({mOpcodeDef.ArgumentType})!");
                    }
                }
            }
            set
            {
                switch (mOpcodeDef.ArgumentType)
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
                        throw new OpcodeInstanceException($"Can't get #{nameof(ARegister)} due to the state of the object ({mOpcodeDef.ArgumentType})!");
                    }
                }
            }
        }

        private RegisterName mBRegester;

        public RegisterName BRegister
        {
            get
            {
                switch (mOpcodeDef.ArgumentType)
                {
                    case OpcodeArgumentType.REG_REG:
                    case OpcodeArgumentType.REG_REG_IMMED:
                    {
                        return mBRegester;
                    }
                    default:
                    {
                        throw new OpcodeInstanceException($"Can't set #{nameof(BRegister)} due to the state of the object ({mOpcodeDef.ArgumentType})!");
                    }
                }
            }
            set
            {
                switch (mOpcodeDef.ArgumentType)
                {
                    case OpcodeArgumentType.REG_REG:
                    case OpcodeArgumentType.REG_REG_IMMED:
                    {
                        mBRegester = value;
                        break;
                    }
                    default:
                    {
                        throw new OpcodeInstanceException($"Can't get #{nameof(BRegister)} due to the state of the object ({mOpcodeDef.ArgumentType})!");
                    }
                }
            }
        }

        private UInt16 mImmediateValue;

        public UInt16 ImmediateValue
        {
            get
            {
                switch (mOpcodeDef.ArgumentType)
                {
                    case OpcodeArgumentType.IMMED:
                    case OpcodeArgumentType.REG_IMMED:
                    case OpcodeArgumentType.REG_REG_IMMED:
                    {
                        return mImmediateValue;
                    }
                    default:
                    {
                        throw new OpcodeInstanceException($"Can't set #{nameof(BRegister)} due to the state of the object ({mOpcodeDef.ArgumentType})!");
                    }
                }
            }
            set
            {
                switch (mOpcodeDef.ArgumentType)
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
                        throw new OpcodeInstanceException($"Can't get #{nameof(BRegister)} due to the state of the object ({mOpcodeDef.ArgumentType})!");
                    }
                }
            }
        }

        public OpcodeInstance(Opcode opcode, RegisterName? regA = null, RegisterName? regB = null, UInt16? immed = null)
        {
            mOpcodeDef = OpcodeDefinition.Get(opcode);
            switch (mOpcodeDef.ArgumentType)
            {
                case OpcodeArgumentType.NONE:
                {
                    if (regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({mOpcodeDef.ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({mOpcodeDef.ArgumentType})!", nameof(regB));
                    if (immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({mOpcodeDef.ArgumentType})!", nameof(immed));

                    // no-op
                    break;
                }
                case OpcodeArgumentType.REG:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({mOpcodeDef.ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({mOpcodeDef.ArgumentType})!", nameof(regB));
                    if (immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({mOpcodeDef.ArgumentType})!", nameof(immed));

                    mARegester = regA.Value;

                    break;
                }
                case OpcodeArgumentType.IMMED:
                {
                    if (regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({mOpcodeDef.ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({mOpcodeDef.ArgumentType})!", nameof(regB));
                    if (!immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({mOpcodeDef.ArgumentType})!", nameof(immed));

                    mImmediateValue = immed.Value;

                    break;
                }
                case OpcodeArgumentType.REG_REG:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({mOpcodeDef.ArgumentType})!", nameof(regA));
                    if (!regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({mOpcodeDef.ArgumentType})!", nameof(regB));
                    if (immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({mOpcodeDef.ArgumentType})!", nameof(immed));

                    mARegester = regA.Value;
                    mBRegester = regB.Value;

                    break;
                }
                case OpcodeArgumentType.REG_IMMED:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({mOpcodeDef.ArgumentType})!", nameof(regA));
                    if (regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects null for this argument ({mOpcodeDef.ArgumentType})!", nameof(regB));
                    if (!immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({mOpcodeDef.ArgumentType})!", nameof(immed));

                    mARegester = regA.Value;
                    mImmediateValue = immed.Value;

                    break;
                }
                case OpcodeArgumentType.REG_REG_IMMED:
                {
                    if (!regA.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({mOpcodeDef.ArgumentType})!", nameof(regA));
                    if (!regB.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({mOpcodeDef.ArgumentType})!", nameof(regB));
                    if (!immed.HasValue)
                        throw new ArgumentException($"{nameof(Opcode)}.{opcode} expects a value for this argument ({mOpcodeDef.ArgumentType})!", nameof(immed));

                    mARegester = regA.Value;
                    mBRegester = regB.Value;
                    mImmediateValue = immed.Value;

                    break;
                }
            }
        }

        public byte[] GetBytes()
        {
            switch (mOpcodeDef.ArgumentType)
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
                    throw new OpcodeInstanceException($"Unknown State {mOpcodeDef.ArgumentType}!");
                }
            }
        }

    }
}
