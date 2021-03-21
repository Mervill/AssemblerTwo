using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace AssemblerTwo.Lib
{
    using OP = Opcode;
    
    [DebuggerDisplay("{Name} ({ArgumentType})")]
    public class OpcodeDefinition
    {
        const OpcodeArgumentType N   = OpcodeArgumentType.NONE;
        const OpcodeArgumentType R   = OpcodeArgumentType.REG;
        const OpcodeArgumentType I   = OpcodeArgumentType.IMMED;
        const OpcodeArgumentType RR  = OpcodeArgumentType.REG_REG;
        const OpcodeArgumentType RI  = OpcodeArgumentType.REG_IMMED;
        const OpcodeArgumentType RRI = OpcodeArgumentType.REG_REG_IMMED;
        
        private static readonly OpcodeDefinition[] Table = new OpcodeDefinition[]
        {
            new (OP.ADD,   RR,  1),
            new (OP.SUB,   RR,  1),
            new (OP.MUL,   RR,  1),
            new (OP.DIV,   RR,  1),
            new (OP.MOD,   RR,  1),
            new (OP.AND,   RR,  1),
            new (OP.OR,    RR,  1),
            new (OP.XOR,   RR,  1),
            new (OP.SHL,   RR,  1),
            new (OP.SHR,   RR,  1),
            new (OP.ADDI,  RI,  2),
            new (OP.SUBI,  RI,  2),
            new (OP.MULI,  RI,  2),
            new (OP.MODI,  RI,  2),
            new (OP.ANDI,  RI,  2),
            new (OP.ORI,   RI,  2),
            new (OP.XORI,  RI,  2),
            new (OP.SHLI,  RI,  2),
            new (OP.SHRI,  RI,  2),
            new (OP.INC,   R,   1),
            new (OP.DEC,   R,   1),
            new (OP.NOT,   R,   1),
            new (OP.NEG,   R,   1),
            new (OP.JLT,   RRI, 2),
            new (OP.JLTE,  RRI, 2),
            new (OP.JGT,   RRI, 2),
            new (OP.JGTE,  RRI, 2),
            new (OP.JB,    RRI, 2),
            new (OP.JBE,   RRI, 2),
            new (OP.JA,    RRI, 2),
            new (OP.JAE,   RRI, 2),
            new (OP.JEQ,   RRI, 2),
            new (OP.JNEQ,  RRI, 2),
            new (OP.RLT,   RR,  1),
            new (OP.RLTE,  RR,  1),
            new (OP.RGT,   RR,  1),
            new (OP.RGTE,  RR,  1),
            new (OP.RB,    RR,  1),
            new (OP.RBE,   RR,  1),
            new (OP.RA,    RR,  1),
            new (OP.RAE,   RR,  1),
            new (OP.REQ,   RR,  1),
            new (OP.RNEQ,  RR,  1),
            new (OP.COPY,  RR,  1),
            new (OP.LOAD,  RR,  1),
            new (OP.STOR,  RR,  1),
            new (OP.RXR,   RR,  1),
            new (OP.RXM,   RR,  1),
            new (OP.HI,    RR,  1),
            new (OP.LO,    RR,  1),
            new (OP.IN,    RR,  1),
            new (OP.COPYI, RI,  2),
            new (OP.INI,   RI,  2),
            new (OP.OUTI,  RI,  2),
            new (OP.SHAL,  RI,  2),
            new (OP.SHAR,  RI,  2),
            new (OP.JUMPR, R,   1),
            //new (OP.SYSCALL, N, 1),
            //new (OP.SYSRET,N    1),
            new (OP.POPR,  N,   1),
            new (OP.PUSHR, N,   1),
            new (OP.JUMP,  I,   2),
            new (OP.CALL,  I,   2),
            new (OP.RET,   N,   1),
            new (OP.HALT,  N,   1),
            new (OP.EI,    N,   1),
            new (OP.DI,    N,   1),
            new (OP.CALLR, R,   1),
            new (OP.PUSH,  R,   1),
            new (OP.POP,   R,   1),
            //new (OP.INTE,  R,   1),
            new (OP.OUT,   R,   1),
            new (OP.JEZ,   RI,  2),
            new (OP.JLZ,   RI,  2),
            new (OP.JLEZ,  RI,  2),
            new (OP.JGZ,   RI,  2),
            new (OP.JGEZ,  RI,  2),
            new (OP.REZ,   R,   1),
            new (OP.RLZ,   R,   1),
            new (OP.RLEZ,  R,   1),
            new (OP.RGZ,   R,   1),
            new (OP.RGEZ,  R,   1),
            new (OP.NOP,   N,   1),
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

        public static OpcodeDefinition Decode(UInt16 encodedOpcode, out RegisterName? registerA, out RegisterName? registerB)
        {
            registerA = null;
            registerB = null;

            int testCode = encodedOpcode;
            OpcodeDefinition testResult = null;

            testResult = Table.FirstOrDefault(x => x.CodeHint == testCode);
            if (testResult == null)
            {
                testCode = encodedOpcode & 0xFFF0;
                testResult = Table.FirstOrDefault(x => x.CodeHint == testCode);
                if (testResult == null)
                {
                    testCode = encodedOpcode & 0xFF00;
                    testResult = Table.FirstOrDefault(x => x.CodeHint == testCode);
                    if (testResult == null)
                    {
                        return null;
                    }
                    else
                    {
                        registerA = (RegisterName)((encodedOpcode & 0x00F0) >> 4);
                        registerB = (RegisterName)(encodedOpcode & 0x000F);
                        return testResult;
                    }
                }
                else
                {
                    if (testResult.RegisterCount == 1)
                    {
                        registerA = (RegisterName)(encodedOpcode & 0x000F);
                        return testResult;
                    }
                    else
                    {
                        registerA = (RegisterName)((encodedOpcode & 0x00F0) >> 4);
                        registerB = (RegisterName)(encodedOpcode & 0x000F);
                        return testResult;
                    }
                }
            }
            return testResult;

            /*
            registerA = (RegisterName)((encodedOpcode & 0x00F0) >> 4);
            registerB = (RegisterName)(encodedOpcode & 0x000F);
            var opNone = Table.FirstOrDefault(x => x.CodeHint == encodedOpcode);
            if (opNone == null)
            {
                var opValueReg = encodedOpcode & 0xFFF0;
                var opReg = Table.FirstOrDefault(x => x.CodeHint == opValueReg);
                if (opReg == null)
                {
                    var opValueRegReg = encodedOpcode & 0xFF00;
                    var opRegReg = Table.FirstOrDefault(x => x.CodeHint == opValueRegReg);
                    if (opRegReg == null)
                    {
                        registerA = null;
                        registerB = null;
                        return null;
                    }
                    return opRegReg;
                }
                registerB = null;
                return opReg;
            }
            registerA = null;
            registerB = null;
            return opNone;
            */
        }

        public static OpcodeInstance DecodeInstance(UInt16 encodedOpcode)
        {
            RegisterName? regA;
            RegisterName? regB;
            OpcodeDefinition def = Decode(encodedOpcode, out regA, out regB);
            if (def != null)
            {
                if (def.RequiresImmediate)
                {
                    return new OpcodeInstance(def.Name, regA, regB, 0);
                }
                else
                {
                    return new OpcodeInstance(def.Name, regA, regB);
                }
            }
            return null;
        }

        public static readonly int LogestInstructionName;

        public readonly Opcode Name;
        public readonly OpcodeArgumentType ArgumentType;
        public readonly int Cycles;

        public UInt16 CodeHint => (UInt16)Name;

        public int ByteLength => GetByteLength(ArgumentType);
        public int RegisterCount => GetRegisterCount(ArgumentType);
        public bool RequiresImmediate => GetRequiresImmediate(ArgumentType);

        static OpcodeDefinition()
        {
            foreach (var def in Table)
            {
                var nameString = def.Name.ToString();
                if (nameString.Length > LogestInstructionName)
                    LogestInstructionName = nameString.Length;
            }
        }

        private OpcodeDefinition(Opcode name, OpcodeArgumentType argumentType, int cycles)
        {
            Name = name;
            ArgumentType = argumentType;
            Cycles = cycles;
        }

        public static int GetByteLength(OpcodeArgumentType argumentType)
        {
            switch (argumentType)
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

        public static int GetRegisterCount(OpcodeArgumentType argumentType)
        {
            switch (argumentType)
            {
                case OpcodeArgumentType.REG:
                case OpcodeArgumentType.REG_IMMED:
                    return 1;
                case OpcodeArgumentType.REG_REG:
                case OpcodeArgumentType.REG_REG_IMMED:
                    return 2;
                default:
                    return 0;
            }
        }

        public static bool GetRequiresImmediate(OpcodeArgumentType argumentType)
        {
            switch (argumentType)
            {
                case OpcodeArgumentType.IMMED:
                case OpcodeArgumentType.REG_IMMED:
                case OpcodeArgumentType.REG_REG_IMMED:
                {
                    return true;
                }
                default:
                {
                    return false;
                }
            }
        }
    }
}
