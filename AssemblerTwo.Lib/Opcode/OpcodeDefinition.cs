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
            new OpcodeDefinition(OP.ADD,   RR,  1),
            new OpcodeDefinition(OP.SUB,   RR,  1),
            new OpcodeDefinition(OP.MUL,   RR,  1),
            new OpcodeDefinition(OP.DIV,   RR,  1),
            new OpcodeDefinition(OP.MOD,   RR,  1),
            new OpcodeDefinition(OP.AND,   RR,  1),
            new OpcodeDefinition(OP.OR,    RR,  1),
            new OpcodeDefinition(OP.XOR,   RR,  1),
            new OpcodeDefinition(OP.SHL,   RR,  1),
            new OpcodeDefinition(OP.SHR,   RR,  1),
            new OpcodeDefinition(OP.ADDI,  RI,  2),
            new OpcodeDefinition(OP.SUBI,  RI,  2),
            new OpcodeDefinition(OP.MULI,  RI,  2),
            new OpcodeDefinition(OP.MODI,  RI,  2),
            new OpcodeDefinition(OP.ANDI,  RI,  2),
            new OpcodeDefinition(OP.ORI,   RI,  2),
            new OpcodeDefinition(OP.XORI,  RI,  2),
            new OpcodeDefinition(OP.SHLI,  RI,  2),
            new OpcodeDefinition(OP.SHRI,  RI,  2),
            new OpcodeDefinition(OP.INC,   R,   1),
            new OpcodeDefinition(OP.DEC,   R,   1),
            new OpcodeDefinition(OP.NOT,   R,   1),
            new OpcodeDefinition(OP.NEG,   R,   1),
            new OpcodeDefinition(OP.JLT,   RRI, 2),
            new OpcodeDefinition(OP.JLTE,  RRI, 2),
            new OpcodeDefinition(OP.JGT,   RRI, 2),
            new OpcodeDefinition(OP.JGTE,  RRI, 2),
            new OpcodeDefinition(OP.JB,    RRI, 2),
            new OpcodeDefinition(OP.JBE,   RRI, 2),
            new OpcodeDefinition(OP.JA,    RRI, 2),
            new OpcodeDefinition(OP.JAE,   RRI, 2),
            new OpcodeDefinition(OP.JEQ,   RRI, 2),
            new OpcodeDefinition(OP.JNEQ,  RRI, 2),
            new OpcodeDefinition(OP.RLT,   RR,  1),
            new OpcodeDefinition(OP.RLTE,  RR,  1),
            new OpcodeDefinition(OP.RGT,   RR,  1),
            new OpcodeDefinition(OP.RGTE,  RR,  1),
            new OpcodeDefinition(OP.RB,    RR,  1),
            new OpcodeDefinition(OP.RBE,   RR,  1),
            new OpcodeDefinition(OP.RA,    RR,  1),
            new OpcodeDefinition(OP.RAE,   RR,  1),
            new OpcodeDefinition(OP.REQ,   RR,  1),
            new OpcodeDefinition(OP.RNEQ,  RR,  1),
            new OpcodeDefinition(OP.COPY,  RR,  1),
            new OpcodeDefinition(OP.LOAD,  RR,  1),
            new OpcodeDefinition(OP.STOR,  RR,  1),
            new OpcodeDefinition(OP.RXR,   RR,  1),
            new OpcodeDefinition(OP.RXM,   RR,  1),
            new OpcodeDefinition(OP.HI,    RR,  1),
            new OpcodeDefinition(OP.LO,    RR,  1),
            new OpcodeDefinition(OP.IN,    RR,  1),
            new OpcodeDefinition(OP.COPYI, RI,  2),
            new OpcodeDefinition(OP.INI,   RI,  2),
            new OpcodeDefinition(OP.OUTI,  RI,  2),
            new OpcodeDefinition(OP.SHAL,  RI,  2),
            new OpcodeDefinition(OP.SHAR,  RI,  2),
            new OpcodeDefinition(OP.JUMPR, R,   1),
            //new OpcodeDefinition(OP.SYSCALL, N, 1),
            //new OpcodeDefinition(OP.SYSRET,N    1),
            new OpcodeDefinition(OP.POPR,  N,   1),
            new OpcodeDefinition(OP.PUSHR, N,   1),
            new OpcodeDefinition(OP.JUMP,  I,   2),
            new OpcodeDefinition(OP.CALL,  I,   2),
            new OpcodeDefinition(OP.RET,   N,   1),
            new OpcodeDefinition(OP.HALT,  N,   1),
            new OpcodeDefinition(OP.EI,    N,   1),
            new OpcodeDefinition(OP.DI,    N,   1),
            new OpcodeDefinition(OP.CALLR, R,   1),
            new OpcodeDefinition(OP.PUSH,  R,   1),
            new OpcodeDefinition(OP.POP,   R,   1),
            //new OpcodeDefinition(OP.INTE,  R,   1),
            new OpcodeDefinition(OP.OUT,   R,   1),
            new OpcodeDefinition(OP.JEZ,   RI,  2),
            new OpcodeDefinition(OP.JLZ,   RI,  2),
            new OpcodeDefinition(OP.JLEZ,  RI,  2),
            new OpcodeDefinition(OP.JGZ,   RI,  2),
            new OpcodeDefinition(OP.JGEZ,  RI,  2),
            new OpcodeDefinition(OP.REZ,   R,   1),
            new OpcodeDefinition(OP.RLZ,   R,   1),
            new OpcodeDefinition(OP.RLEZ,  R,   1),
            new OpcodeDefinition(OP.RGZ,   R,   1),
            new OpcodeDefinition(OP.RGEZ,  R,   1),
            new OpcodeDefinition(OP.NOP,   N,   1),
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
