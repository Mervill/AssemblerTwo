using System;

namespace AssemblerTwo.Machine
{
    public class VirtualMachine
    {
        IMemoryBus mMemoryBus;
        IInputOutputBus mInputOutputBus;

        public readonly int[] Registers = new int[0x10];

        public int ProgramCounter { get; set; }
        public int StackPointer
        {
            get { return Registers[0xF];  }
            set { Registers[0xF] = value; }
        }
        public bool IsHalted { get; set; }
        public bool CanInterrupt { get; private set; }
        public bool PendingInterrupt { get; private set; }
        public int PendingInterruptVector { get; private set; }

        public VirtualMachine(IMemoryBus memoryBus, IInputOutputBus inputOutputBus)
        {
            mMemoryBus = memoryBus;
            mInputOutputBus = inputOutputBus;
        }

        int MemoryRead(int address)
        {
            return (mMemoryBus.MachineRead(address) << 8) | mMemoryBus.MachineRead(address + 1);
        }

        void MemoryWrite(int address, int value)
        {
            mMemoryBus.MachineWrite(address, (byte)(value >> 8));
            mMemoryBus.MachineWrite(address + 1, (byte)(value & 0xFF));
        }

        int StackPop()
        {
            var top = MemoryRead(StackPointer);
            StackPointer = (StackPointer + 2) & 0xFFFF;
            return top;
        }

        void StackPush(int value)
        {
            StackPointer = (StackPointer - 2) & 0xFFFF;
            MemoryWrite(StackPointer, value);
        }

        int FetchAndAdvance()
        {
            var value = MemoryRead(ProgramCounter);
            ProgramCounter = (ProgramCounter + 2) & 0xFFFF;
            return value;
        }

        void Call(int address)
        {
            StackPush(ProgramCounter);
            ProgramCounter = address;
        }

        void Ret()
        {
            ProgramCounter = StackPop();
        }

        public bool RequestInterrupt(int vector)
        {
            if (!IsHalted)
            {
                if (CanInterrupt)
                {
                    PendingInterruptVector = vector & 0xFFFF;
                    PendingInterrupt = true;
                }
                return PendingInterrupt;
            }
            else
            {
                return false;
            }
        }

        public int StepInstruction()
        {
            if (!IsHalted)
            {
                if (PendingInterrupt) // && CanInterrupt?
                {
                    int result = DoInstruction(PendingInterruptVector);
                    PendingInterrupt = false;
                    PendingInterruptVector = 0;
                    return result;
                }
                var next_instruction = FetchAndAdvance();
                return DoInstruction(next_instruction);
            }
            else
            {
                return 0;
            }
        }

        int DoInstruction(int opcode)
        {
            if (opcode < 0xA000)
            {
                throw new IndexOutOfRangeException();
            }

            if (opcode >= 0xA000 && opcode <= 0xAFFF)
            {
                if (opcode < 0xAF00)
                {
                    int oper = (opcode & 0x0F00) >> 8;
                    int regA = (opcode & 0x00F0) >> 4;
                    int regB = (opcode & 0x000F);
                    switch (oper)
                    {
                        case 0x0:
                        {
                            // ADD
                            Registers[regB] = Registers[regA] + Registers[regB];
                            return 1;
                        }
                        case 0x1:
                        {
                            // SUB
                            Registers[regB] = Registers[regA] - Registers[regB];
                            return 1;
                        }
                        case 0x2:
                        {
                            // MUL
                            Registers[regB] = Registers[regA] * Registers[regB];
                            return 1;
                        }
                        case 0x3:
                        {
                            // DIV
                            int dividend = Registers[regA];
                            int divisor  = Registers[regB];
                            int quotient = dividend / divisor;
                            Registers[regB] = quotient;
                            return 1;
                        }
                        case 0x4:
                        {
                            //DIVS
                            throw new NotImplementedException();
                            //Registers[regB] = Registers[regA] / Registers[regB];
                            //return 1;
                        }
                        case 0x5:
                        {
                            // MOD
                            Registers[regB] = Registers[regA] % Registers[regB];
                            return 1;
                        }
                        case 0x6:
                        {
                            // AND
                            Registers[regB] = Registers[regA] & Registers[regB];
                            return 1;
                        }
                        case 0x7:
                        {
                            // OR
                            Registers[regB] = Registers[regA] | Registers[regB];
                            return 1;
                        }
                        case 0x8:
                        {
                            // XOR
                            Registers[regB] = Registers[regA] ^ Registers[regB];
                            return 1;
                        }
                        case 0x9:
                        {
                            // SHL
                            Registers[regB] = Registers[regA] << Registers[regB];
                            return 1;
                        }
                        case 0xA:
                        {
                            // SHR
                            Registers[regB] = Registers[regA] >> Registers[regB];
                            return 1;
                        }
                        case 0xB:
                        {
                            // POW
                            throw new NotImplementedException();
                        }
                        case 0xC: // Unallocated
                        case 0xD: // Unallocated
                        case 0xE: // Unallocated
                        {
                            throw new IndexOutOfRangeException();
                        }
                    }
                }
                else
                {
                    int oper  = (opcode & 0x00F0) >> 4;
                    int regA  = (opcode & 0x000F);
                    switch (oper)
                    {
                        case 0x0:
                        {
                            // ADDI
                            Registers[regA] = Registers[regA] + FetchAndAdvance();
                            return 2;
                        }
                        case 0x1:
                        {
                            // SUBI
                            Registers[regA] = Registers[regA] - FetchAndAdvance();
                            return 2;
                        }
                        case 0x2:
                        {
                            // MULI
                            Registers[regA] = Registers[regA] * FetchAndAdvance();
                            return 2;
                        }
                        case 0x3:
                        {
                            // DIVI
                            throw new NotImplementedException();
                        }
                        case 0x4:
                        {
                            // DIVSI
                            throw new NotImplementedException();
                        }
                        case 0x5:
                        {
                            // MODI
                            Registers[regA] = Registers[regA] % FetchAndAdvance();
                            return 2;
                        }
                        case 0x6:
                        {
                            // ANDI
                            Registers[regA] = Registers[regA] & FetchAndAdvance();
                            return 2;
                        }
                        case 0x7:
                        {
                            // ORI
                            Registers[regA] = Registers[regA] | FetchAndAdvance();
                            return 2;
                        }
                        case 0x8:
                        {
                            // XORI
                            Registers[regA] = Registers[regA] ^ FetchAndAdvance();
                            return 2;
                        }
                        case 0x9:
                        {
                            // SHLI
                            Registers[regA] = Registers[regA] << FetchAndAdvance();
                            return 2;
                        }
                        case 0xA:
                        {
                            // SHRI
                            Registers[regA] = Registers[regA] >> FetchAndAdvance();
                            return 2;
                        }
                        case 0xB:
                        {
                            // POWI
                            throw new NotImplementedException();
                        }
                        case 0xC:
                        {
                            // INC
                            Registers[regA] = Registers[regA] + 1;
                            return 1;
                        }
                        case 0xD:
                        {
                            // DEC
                            Registers[regA] = Registers[regA] - 1;
                            return 1;
                        }
                        case 0xE:
                        {
                            // NOT
                            Registers[regA] = ~Registers[regA];
                            return 1;
                        }
                        case 0xF:
                        {
                            // NEG
                            Registers[regA] = -Registers[regA];
                            return 1;
                        }
                    }
                }
            }

            if (opcode >= 0xB000 && opcode <= 0xBFFF)
            {
                int oper = (opcode & 0x0F00) >> 8;
                int regA = (opcode & 0x00F0) >> 4;
                int regB = (opcode & 0x000F);
                int immediate = FetchAndAdvance();
                switch (oper)
                {
                    case 0x0:
                    {
                        // JLT
                       if (Registers[regA] < Registers[regB])
                        {
                            ProgramCounter = immediate;
                        }
                        return 2;
                    }
                    case 0x1:
                    {
                        // JLTE
                        if (Registers[regA] <= Registers[regB])
                        {
                            ProgramCounter = immediate;
                        }
                        return 2;
                    }
                    case 0x2:
                    {
                        // JGT
                        if (Registers[regA] > Registers[regB])
                        {
                            ProgramCounter = immediate;
                        }
                        return 2;
                    }
                    case 0x3:
                    {
                        // JGTE
                        if (Registers[regA] >= Registers[regB])
                        {
                            ProgramCounter = immediate;
                        }
                        return 2;
                    }
                    case 0x4: // JB
                    case 0x5: // JBE
                    case 0x6: // JA
                    case 0x7: // JAE
                    {
                        throw new NotImplementedException();
                    }
                    case 0x8:
                    {
                        // JEQ
                        if (Registers[regA] == Registers[regB])
                        {
                            ProgramCounter = immediate;
                        }
                        return 2;
                    }
                    case 0x9:
                    {
                        // JNEQ
                        if (Registers[regA] != Registers[regB])
                        {
                            ProgramCounter = immediate;
                        }
                        return 2;
                    }
                    case 0xA: // CLT
                    case 0xB: // CLTE
                    case 0xC: // CGT
                    case 0xD: // CGTE
                    case 0xE: // CB
                    case 0xF: // CBE
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            if (opcode >= 0xC000 && opcode <= 0xCFFF)
            {
                if (opcode < 0xC300)
                {
                    // CA
                    // CAE
                    // CEQ
                    // CNEQ
                    throw new NotImplementedException();
                }
                else
                {
                    int oper = (opcode & 0x0F00) >> 8;
                    int regA = (opcode & 0x00F0) >> 4;
                    int regB = (opcode & 0x000F);
                    switch (oper)
                    {
                        case 0x4:
                        {
                            // RLT
                            if (Registers[regA] < Registers[regB])
                            {
                                Ret();
                            }
                            return 1;
                        }
                        case 0x5:
                        {
                            // RLTE
                            if (Registers[regA] <= Registers[regB])
                            {
                                Ret();
                            }
                            return 1;
                        }
                        case 0x6:
                        {
                            // RGT
                            if (Registers[regA] > Registers[regB])
                            {
                                Ret();
                            }
                            return 1;
                        }
                        case 0x7:
                        {
                            // RGTE
                            if (Registers[regA] >= Registers[regB])
                            {
                                Ret();
                            }
                            return 1;
                        }
                        case 0x8: // RB
                        case 0x9: // RBE
                        case 0xA: // RA
                        case 0xB: // RAE
                        {
                            throw new NotImplementedException();
                        }
                        case 0xC:
                        {
                            // REQ
                            if (Registers[regA] == Registers[regB])
                            {
                                Ret();
                            }
                            return 1;
                        }
                        case 0xD:
                        {
                            // RNEQ
                            if (Registers[regA] != Registers[regB])
                            {
                                Ret();
                            }
                            return 1;
                        }
                        case 0xE: // Unallocated
                        case 0xF: // Unallocated
                        {
                            throw new IndexOutOfRangeException();
                        }
                    }
                }
            }

            if (opcode >= 0xD000 && opcode <= 0xDFFF)
            {
                if (opcode < 0xDE00)
                {
                    int oper = (opcode & 0x0F00) >> 8;
                    int regA = (opcode & 0x00F0) >> 4;
                    int regB = (opcode & 0x000F);
                    switch (oper)
                    {
                        case 0x0:
                        {
                            // COPY
                            Registers[regB] = Registers[regA];
                            return 1;
                        }
                        case 0x1:
                        {
                            // LOAD
                            Registers[regB] = MemoryRead(Registers[regA]);
                            return 1;
                        }
                        case 0x2:
                        {
                            // STOR
                            MemoryWrite(Registers[regB], Registers[regA]);
                            return 1;
                        }
                        case 0x3: // Unallocated - PLOAD
                        case 0x4: // Unallocated - PSTOR
                        case 0x5: // Unallocated
                        case 0x6: // Unallocated
                        case 0x7: // Unallocated
                        case 0x8: // Unallocated
                        {
                            throw new IndexOutOfRangeException();
                        }
                        case 0x9:
                        {
                            // RXR
                            int t = Registers[regA];
                            Registers[regA] = Registers[regB];
                            Registers[regB] = t;
                            return 1;
                        }
                        case 0xA:
                        {
                            // RXM
                            int t = MemoryRead(Registers[regA]);
                            MemoryWrite(Registers[regA], Registers[regB]);
                            Registers[regB] = t;
                            return 1;
                        }
                        case 0xB:
                        {
                            // HI
                            Registers[regB] = Registers[regA] >> 8;
                            return 1;
                        }
                        case 0xC:
                        {
                            // LO
                            Registers[regB] = Registers[regA] & 0x00FF;
                            return 1;
                        }
                        case 0xD:
                        {
                            // IN
                            Registers[regA] = mInputOutputBus.MachineReadInput(Registers[regB]);
                            return 1;
                        }
                    }
                }
                else if (opcode < 0xDF00)
                {
                    int oper = (opcode & 0x00F0) >> 4;
                    int valu = (opcode & 0x000F);
                    switch (oper)
                    {
                        case 0x0:
                        {
                            // COPYI
                            Registers[valu] = FetchAndAdvance();
                            return 2;
                        }
                        case 0x1: // Unallocated - PLOADI
                        case 0x2: // Unallocated - PSTORI
                        {
                            throw new IndexOutOfRangeException();
                        }
                        case 0x3:
                        {
                            // INI
                            int portNumber = FetchAndAdvance();
                            Registers[valu] = mInputOutputBus.MachineReadInput(portNumber);
                            return 2;
                        }
                        case 0x4: // OUTI
                        {
                            int portNumber = FetchAndAdvance();
                            mInputOutputBus.MachineWriteOutput(portNumber, Registers[valu]);
                            return 2;
                        }
                        case 0x5: // Unallocated - SHAL
                        case 0x6: // Unallocated - SHAR
                        case 0x7: // Unallocated - ROTL
                        case 0x8: // Unallocated - ROTR
                        {
                            throw new IndexOutOfRangeException();
                        }
                        case 0x9:
                        {
                            // JUMPR
                            ProgramCounter = Registers[valu];
                            return 1;
                        }
                        case 0xA:
                        {
                            switch (valu)
                            {
                                case 0x0: // Unallocated - SYSCALL
                                case 0x1: // Unallocated - SYSRET
                                case 0x2: // Unallocated
                                case 0x5: // Unallocated
                                case 0x6: // Unallocated
                                case 0x7: // Unallocated
                                case 0x8: // Unallocated
                                case 0x9: // Unallocated
                                {
                                    throw new IndexOutOfRangeException();
                                }
                                case 0xA:
                                {
                                    // JUMP (JUMPI)
                                    ProgramCounter = MemoryRead(ProgramCounter);
                                    return 2;
                                }
                                case 0xB:
                                {
                                    // CALL (CALLI)
                                    Call(FetchAndAdvance());
                                    return 2;
                                }
                                case 0xC:
                                {
                                    // RET
                                    Ret();
                                    return 1;
                                }
                                case 0xD:
                                {
                                    // HALT
                                    IsHalted = true;
                                    return 1;
                                }
                                case 0xE:
                                {
                                    // EI
                                    CanInterrupt = true;
                                    return 1;
                                }
                                case 0xF:
                                {
                                    // DI
                                    CanInterrupt = false;
                                    return 1;
                                }
                                default:
                                {
                                    throw new IndexOutOfRangeException();
                                }
                            }
                        }
                        case 0xB:
                        {
                            // CALLR
                            Call(Registers[valu]);
                            return 1;
                        }
                        case 0xC:
                        {
                            // PUSH
                            StackPush(Registers[valu]);
                            return 1;
                        }
                        case 0xD:
                        {
                            // POP
                            Registers[valu] = StackPop();
                            return 1;
                        }
                        case 0xE:
                        {
                            // INTE
                            Registers[valu] = Convert.ToInt32(CanInterrupt);
                            return 1;
                        }
                        case 0xF:
                        {
                            // Unallocated
                            throw new IndexOutOfRangeException();
                        }
                    }
                }
                else if (opcode >= 0xDF00 && opcode <= 0xDFFF)
                {
                    // OUT
                    int regA = (opcode & 0x00F0) >> 4;
                    int regB = (opcode & 0x000F);
                    mInputOutputBus.MachineWriteOutput(Registers[regA], Registers[regB]);
                    return 1;
                }
            }

            if (opcode >= 0xE000 && opcode <= 0xEFFF)
            {
                throw new IndexOutOfRangeException();
            }

            if (opcode >= 0xF000 && opcode <= 0xFFFF)
            {
                if (opcode >= 0xFF00 && opcode <= 0xFFFF)
                {
                    int oper = (opcode & 0x00F0) >> 4;
                    int valu = (opcode & 0x000F);
                    switch (oper)
                    {
                        case 0x0:
                        {
                            // JEZ
                            int immediate = FetchAndAdvance();
                            if (Registers[valu] == 0)
                            {
                                ProgramCounter = immediate;
                            }
                            return 2;
                        }
                        case 0x1:
                        {
                            // JLZ
                            int immediate = FetchAndAdvance();
                            if (Registers[valu] < 0)
                            {
                                ProgramCounter = immediate;
                            }
                            return 2;
                        }
                        case 0x2:
                        {
                            // JLEZ
                            int immediate = FetchAndAdvance();
                            if (Registers[valu] <= 0)
                            {
                                ProgramCounter = immediate;
                            }
                            return 2;
                        }
                        case 0x3:
                        {
                            // JGZ
                            int immediate = FetchAndAdvance();
                            if (Registers[valu] > 0)
                            {
                                ProgramCounter = immediate;
                            }
                            return 2;
                        }
                        case 0x4:
                        {
                            // JGEZ
                            int immediate = FetchAndAdvance();
                            if (Registers[valu] >= 0)
                            {
                                ProgramCounter = immediate;
                            }
                            return 2;
                        }
                        case 0x5: // CEZ
                        case 0x6: // CLZ
                        case 0x7: // CLEZ
                        case 0x8: // CGZ
                        case 0x9: // CGEZ
                        {
                            throw new NotImplementedException();
                        }
                        case 0xA:
                        {
                            // REZ
                            if (Registers[valu] == 0)
                            {
                                Ret();
                            }
                            return 1;
                        }
                        case 0xB:
                        {
                            // RLZ
                            if (Registers[valu] < 0)
                            {
                                Ret();
                            }
                            return 1;
                        }
                        case 0xC:
                        {
                            // RLEZ
                            if (Registers[valu] <= 0)
                            {
                                Ret();
                            }
                            return 1;
                        }
                        case 0xD:
                        {
                            // RGZ
                            if (Registers[valu] > 0)
                            {
                                Ret();
                            }
                            return 1;
                        }
                        case 0xE:
                        {
                            // RGEZ
                            if (Registers[valu] >= 0)
                            {
                                Ret();
                            }
                            return 1;
                        }
                        case 0xF:
                        {
                            if (opcode == 0xFFF0)
                            {
                                // NOP
                                return 1;
                            }
                            else
                            {
                                throw new IndexOutOfRangeException();
                            }
                        }
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }

            throw new InvalidOperationException();
        }
    }
}
