using System;
using System.Collections;

namespace AssemblerTwo.Lib
{
    public class OpcodeDefinition
    {
        private static readonly OpcodeDefinition[] Table = new OpcodeDefinition[]
        {
            new (Opcode.ADD,   OpcodeArgumentType.REG_REG),
            new (Opcode.SUB,   OpcodeArgumentType.REG_REG),
            new (Opcode.MUL,   OpcodeArgumentType.REG_REG),
            new (Opcode.MOD,   OpcodeArgumentType.REG_REG),
            new (Opcode.AND,   OpcodeArgumentType.REG_REG),
            new (Opcode.OR,    OpcodeArgumentType.REG_REG),
            new (Opcode.XOR,   OpcodeArgumentType.REG_REG),
            new (Opcode.SHL,   OpcodeArgumentType.REG_REG),
            new (Opcode.SHR,   OpcodeArgumentType.REG_REG),
            new (Opcode.ADDI,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.SUBI,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.MULI,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.MODI,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.ANDI,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.ORI,   OpcodeArgumentType.REG_IMMED),
            new (Opcode.XORI,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.SHLI,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.SHRI,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.INC,   OpcodeArgumentType.REG),
            new (Opcode.DEC,   OpcodeArgumentType.REG),
            new (Opcode.NOT,   OpcodeArgumentType.REG),
            new (Opcode.NEG,   OpcodeArgumentType.REG),
            new (Opcode.JLT,   OpcodeArgumentType.REG_REG_IMMED),
            new (Opcode.JLTE,  OpcodeArgumentType.REG_REG_IMMED),
            new (Opcode.JGT,   OpcodeArgumentType.REG_REG_IMMED),
            new (Opcode.JGTE,  OpcodeArgumentType.REG_REG_IMMED),
            new (Opcode.JB,    OpcodeArgumentType.REG_REG_IMMED),
            new (Opcode.JBE,   OpcodeArgumentType.REG_REG_IMMED),
            new (Opcode.JA,    OpcodeArgumentType.REG_REG_IMMED),
            new (Opcode.JAE,   OpcodeArgumentType.REG_REG_IMMED),
            new (Opcode.JEQ,   OpcodeArgumentType.REG_REG_IMMED),
            new (Opcode.JNEQ,  OpcodeArgumentType.REG_REG_IMMED),
            new (Opcode.RLT,   OpcodeArgumentType.REG_REG),
            new (Opcode.RLTE,  OpcodeArgumentType.REG_REG),
            new (Opcode.RGT,   OpcodeArgumentType.REG_REG),
            new (Opcode.RGTE,  OpcodeArgumentType.REG_REG),
            new (Opcode.RB,    OpcodeArgumentType.REG_REG),
            new (Opcode.RBE,   OpcodeArgumentType.REG_REG),
            new (Opcode.RA,    OpcodeArgumentType.REG_REG),
            new (Opcode.RAE,   OpcodeArgumentType.REG_REG),
            new (Opcode.REQ,   OpcodeArgumentType.REG_REG),
            new (Opcode.RNEQ,  OpcodeArgumentType.REG_REG),
            new (Opcode.COPY,  OpcodeArgumentType.REG_REG),
            new (Opcode.LOAD,  OpcodeArgumentType.REG_REG),
            new (Opcode.STOR,  OpcodeArgumentType.REG_REG),
            new (Opcode.RXR,   OpcodeArgumentType.REG_REG),
            new (Opcode.RXM,   OpcodeArgumentType.REG_REG),
            new (Opcode.HI,    OpcodeArgumentType.REG_REG),
            new (Opcode.LO,    OpcodeArgumentType.REG_REG),
            new (Opcode.IN,    OpcodeArgumentType.REG_REG),
            new (Opcode.COPYI, OpcodeArgumentType.REG_IMMED),
            new (Opcode.INI,   OpcodeArgumentType.REG_IMMED),
            new (Opcode.OUTI,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.SHAL,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.SHAR,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.JUMPR, OpcodeArgumentType.REG),
            //new (Opcode.SYSCALL, OpcodeArgumentType.NONE),
            //new (Opcode.SYSRET, OpcodeArgumentType.NONE),
            new (Opcode.POPR,  OpcodeArgumentType.NONE),
            new (Opcode.PUSHR, OpcodeArgumentType.NONE),
            new (Opcode.JUMP,  OpcodeArgumentType.IMMED),
            new (Opcode.CALL,  OpcodeArgumentType.IMMED),
            new (Opcode.RET,   OpcodeArgumentType.NONE),
            new (Opcode.HALT,  OpcodeArgumentType.NONE),
            new (Opcode.EI,    OpcodeArgumentType.NONE),
            new (Opcode.DI,    OpcodeArgumentType.NONE),
            new (Opcode.CALLR, OpcodeArgumentType.REG),
            new (Opcode.PUSH,  OpcodeArgumentType.REG),
            new (Opcode.POP,   OpcodeArgumentType.REG),
            //new (Opcode.INTE,  OpcodeArgumentType.REG),
            new (Opcode.OUT,   OpcodeArgumentType.REG),
            new (Opcode.JEZ,   OpcodeArgumentType.REG_IMMED),
            new (Opcode.JLZ,   OpcodeArgumentType.REG_IMMED),
            new (Opcode.JLEZ,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.JGZ,   OpcodeArgumentType.REG_IMMED),
            new (Opcode.JGEZ,  OpcodeArgumentType.REG_IMMED),
            new (Opcode.REZ,   OpcodeArgumentType.REG),
            new (Opcode.RLZ,   OpcodeArgumentType.REG),
            new (Opcode.RLEZ,  OpcodeArgumentType.REG),
            new (Opcode.RGZ,   OpcodeArgumentType.REG),
            new (Opcode.RGEZ,  OpcodeArgumentType.REG),
            new (Opcode.NOP,   OpcodeArgumentType.NONE),
        };

        public static IEnumerator GetDefinitionEnumerator()
        {
            return Table.GetEnumerator();
        }

        public static OpcodeDefinition Get(Opcode opcode)
        {
            foreach (var entry in Table)
                if (entry.Name == opcode)
                    return entry;

            throw new NullReferenceException();
        }

        public readonly Opcode Name;
        public readonly OpcodeArgumentType ArgumentType;
        public UInt16 CodeHint => (UInt16)Name;

        public int ByteLength
        {
            get
            {
                switch (ArgumentType)
                {
                    case OpcodeArgumentType.NONE:
                    case OpcodeArgumentType.REG:
                    case OpcodeArgumentType.REG_REG:
                        return 2;
                    case OpcodeArgumentType.IMMED:
                    case OpcodeArgumentType.REG_IMMED:
                    case OpcodeArgumentType.REG_REG_IMMED:
                        return 4;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private OpcodeDefinition(Opcode name, OpcodeArgumentType argumentType)
        {
            Name = name;
            ArgumentType = argumentType;
        }

    }
}
