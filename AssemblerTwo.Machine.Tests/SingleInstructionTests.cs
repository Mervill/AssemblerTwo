using System;
using System.Collections.Generic;
using System.Text;

using AssemblerTwo.Lib;
using AssemblerTwo.Lib.Utility;
using AssemblerTwo.Machine;

using NUnit.Framework;

namespace AssemblerTwo.Machine.Tests
{
    [TestFixture]
    public static class SingleInstructionTests
    {
        [TestCase(Opcode.ADD, 2, 3, 5)]
        [TestCase(Opcode.SUB, 7, 2, 5)]
        [TestCase(Opcode.MUL, 2, 3, 6)]
        //DIV
        //DIVS
        //MOD
        [TestCase(Opcode.AND, 3, 9, 1)]
        [TestCase(Opcode.OR, 1, 8, 9)]
        [TestCase(Opcode.XOR, 5, 1, 4)]
        //SHL
        //SHR
        [TestCase(Opcode.COPY, 5, 0, 5)]
        //[TestCase(Opcode.RXR, 5, 0, 5)] // needs more through test
        //[TestCase(Opcode.RXR, 0, 5, 0)] // ...
        [TestCase(Opcode.HI, 0xAABB, 0, 0xAA)]
        [TestCase(Opcode.LO, 0xBBAA, 0, 0xAA)]
        public static void RegRegResult(Opcode opcode, int valueA, int valueB, int result)
        {
            var opcodeBuilder = new OpcodeBuilder();
            opcodeBuilder.Append(opcode, RegisterName.A, RegisterName.B);

            var memBus = new DefaultMemoryBus();
            memBus.CopyInto(opcodeBuilder.GetBytes(), 0);

            var ioBus = new EmptyIOBus();

            var vm = new VirtualMachine(memBus, ioBus);
            vm.Registers[0] = valueA & 0xFFFF;
            vm.Registers[1] = valueB & 0xFFFF;

            Assert.AreEqual(1, vm.StepInstruction());
            Assert.AreEqual(2, vm.ProgramCounter);

            Assert.AreEqual(valueA & 0xFFFF, vm.Registers[0]);
            Assert.AreEqual(result, vm.Registers[1]);
        }

        [TestCase(Opcode.ADDI, 2, 3, 5)]
        [TestCase(Opcode.SUBI, 7, 2, 5)]
        [TestCase(Opcode.MULI, 2, 3, 6)]
        //DIVI
        //DIVSI
        //MODI
        [TestCase(Opcode.ANDI, 3, 9, 1)]
        [TestCase(Opcode.ORI, 1, 8, 9)]
        [TestCase(Opcode.XORI, 5, 1, 4)]
        //SHLI
        //SHRI
        public static void MathImmediateOp(Opcode opcode, int valueA, int valueB, int result)
        {
            var opcodeBuilder = new OpcodeBuilder();
            opcodeBuilder.Append(opcode, RegisterName.B, immed: (ushort)valueB);

            var memBus = new DefaultMemoryBus();
            memBus.CopyInto(opcodeBuilder.GetBytes(), 0);

            var ioBus = new EmptyIOBus();

            var vm = new VirtualMachine(memBus, ioBus);
            vm.Registers[1] = valueA & 0xFFFF;

            Assert.AreEqual(2, vm.StepInstruction());
            Assert.AreEqual(4, vm.ProgramCounter);

            Assert.AreEqual(result, vm.Registers[1]);
        }

        [Test]
        public static void Increment()
        {
            var expectedValue = (new Random().Next(Int16.MinValue + 1, Int16.MaxValue));

            var opcodeBuilder = new OpcodeBuilder();
            opcodeBuilder.Append(Opcode.INC, RegisterName.B);

            var memBus = new DefaultMemoryBus();
            memBus.CopyInto(opcodeBuilder.GetBytes(), 0);

            var ioBus = new EmptyIOBus();

            var vm = new VirtualMachine(memBus, ioBus);
            vm.Registers[1] = (expectedValue - 1);

            Assert.AreEqual(1, vm.StepInstruction());
            Assert.AreEqual(2, vm.ProgramCounter);

            Assert.AreEqual(expectedValue, vm.Registers[1]);
        }

        [Test]
        public static void Decrement()
        {
            var expectedValue = (new Random().Next(Int16.MinValue, Int16.MaxValue - 1));

            var opcodeBuilder = new OpcodeBuilder();
            opcodeBuilder.Append(Opcode.DEC, RegisterName.B);

            var memBus = new DefaultMemoryBus();
            memBus.CopyInto(opcodeBuilder.GetBytes(), 0);

            var ioBus = new EmptyIOBus();

            var vm = new VirtualMachine(memBus, ioBus);
            vm.Registers[1] = (expectedValue + 1);

            Assert.AreEqual(1, vm.StepInstruction());
            Assert.AreEqual(2, vm.ProgramCounter);

            Assert.AreEqual(expectedValue, vm.Registers[1]);
        }

        [Test]
        public static void BitwiseNot()
        {
            var expectedValue = (new Random().Next(Int16.MinValue, Int16.MaxValue));

            var opcodeBuilder = new OpcodeBuilder();
            opcodeBuilder.Append(Opcode.NOT, RegisterName.B);

            var memBus = new DefaultMemoryBus();
            memBus.CopyInto(opcodeBuilder.GetBytes(), 0);

            var ioBus = new EmptyIOBus();

            var vm = new VirtualMachine(memBus, ioBus);
            vm.Registers[1] = ~expectedValue;

            Assert.AreEqual(1, vm.StepInstruction());
            Assert.AreEqual(2, vm.ProgramCounter);

            Assert.AreEqual(expectedValue, vm.Registers[1]);
        }

        [Test]
        public static void SignNeg()
        {
            var expectedValue = (new Random().Next(Int16.MinValue, Int16.MaxValue));

            var opcodeBuilder = new OpcodeBuilder();
            opcodeBuilder.Append(Opcode.NEG, RegisterName.B);

            var memBus = new DefaultMemoryBus();
            memBus.CopyInto(opcodeBuilder.GetBytes(), 0);

            var ioBus = new EmptyIOBus();

            var vm = new VirtualMachine(memBus, ioBus);
            vm.Registers[1] = -expectedValue;

            Assert.AreEqual(1, vm.StepInstruction());
            Assert.AreEqual(2, vm.ProgramCounter);

            Assert.AreEqual(expectedValue, vm.Registers[1]);
        }

        [TestCase(Opcode.JLT,   5, 10, true )]
        [TestCase(Opcode.JLT,  10,  5, false)]
        [TestCase(Opcode.JLT,   5,  5, false)]
        [TestCase(Opcode.JLTE,  5, 10, true )]
        [TestCase(Opcode.JLTE, 10,  5, false)]
        [TestCase(Opcode.JLTE,  5,  5, true )]
        [TestCase(Opcode.JGT,   5, 10, false)]
        [TestCase(Opcode.JGT,  10,  5, true )]
        [TestCase(Opcode.JGT,   5,  5, false)]
        [TestCase(Opcode.JGTE,  5, 10, false)]
        [TestCase(Opcode.JGTE, 10,  5, true )]
        [TestCase(Opcode.JGTE,  5,  5, true )]
        //jb
        //jbe
        //ja
        //jae
        [TestCase(Opcode.JEQ,   5, 10, false)]
        [TestCase(Opcode.JEQ,  10,  5, false)]
        [TestCase(Opcode.JEQ,   5,  5, true )]
        [TestCase(Opcode.JNEQ,  5, 10, true )]
        [TestCase(Opcode.JNEQ, 10,  5, true )]
        [TestCase(Opcode.JNEQ,  5,  5, false)]
        public static void JumpCond(Opcode opcode, int valueA, int valueB, bool expectJump)
        {
            var opcodeBuilder = new OpcodeBuilder();
            opcodeBuilder.Append(opcode, RegisterName.B, RegisterName.C, 0xF00D);

            var memBus = new DefaultMemoryBus();
            memBus.CopyInto(opcodeBuilder.GetBytes(), 0);

            var ioBus = new EmptyIOBus();

            var vm = new VirtualMachine(memBus, ioBus);
            vm.Registers[1] = valueA & 0xFFFF;
            vm.Registers[2] = valueB & 0xFFFF;

            Assert.AreEqual(2, vm.StepInstruction());
            var assertAddress = expectJump ? 0xF00D : 0x0004;
            Assert.AreEqual(assertAddress, vm.ProgramCounter);
        }

        [TestCase(Opcode.RLT,   5, 10, true )]
        [TestCase(Opcode.RLT,  10,  5, false)]
        [TestCase(Opcode.RLT,   5,  5, false)]
        [TestCase(Opcode.RLTE,  5, 10, true )]
        [TestCase(Opcode.RLTE, 10,  5, false)]
        [TestCase(Opcode.RLTE,  5,  5, true )]
        [TestCase(Opcode.RGT,   5, 10, false)]
        [TestCase(Opcode.RGT,  10,  5, true )]
        [TestCase(Opcode.RGT,   5,  5, false)]
        [TestCase(Opcode.RGTE,  5, 10, false)]
        [TestCase(Opcode.RGTE, 10,  5, true )]
        [TestCase(Opcode.RGTE,  5,  5, true )]
        //rb
        //rbe
        //ra
        //rae
        [TestCase(Opcode.REQ,   5, 10, false)]
        [TestCase(Opcode.REQ,  10,  5, false)]
        [TestCase(Opcode.REQ,   5,  5, true )]
        [TestCase(Opcode.RNEQ,  5, 10, true )]
        [TestCase(Opcode.RNEQ, 10,  5, true )]
        [TestCase(Opcode.RNEQ,  5,  5, false)]
        public static void RetCond(Opcode opcode, int valueA, int valueB, bool expectReturn)
        {
            Assert.Ignore();
            
            var opcodeBuilder = new OpcodeBuilder();
            opcodeBuilder.Append(opcode, RegisterName.B, RegisterName.C);

            var memBus = new DefaultMemoryBus();
            memBus.CopyInto(opcodeBuilder.GetBytes(), 0);

            var ioBus = new EmptyIOBus();

            var vm = new VirtualMachine(memBus, ioBus);
            vm.Registers[1] = valueA & 0xFFFF;
            vm.Registers[2] = valueB & 0xFFFF;

            Assert.AreEqual(1, vm.StepInstruction());
            var assertAddress = expectReturn ? 0xF00D : 0x0004;
            Assert.AreEqual(assertAddress, vm.ProgramCounter);
        }

        [Test]
        public static void Push()
        {
            const int stackOrigin = 0xFFFF;

            var opcodeBuilder = new OpcodeBuilder();
            opcodeBuilder.Append(Opcode.PUSH, RegisterName.B);

            var memBus = new DefaultMemoryBus();
            memBus.CopyInto(opcodeBuilder.GetBytes(), 0);

            var ioBus = new EmptyIOBus();

            var vm = new VirtualMachine(memBus, ioBus);
            vm.StackPointer = stackOrigin;
            vm.Registers[1] = 0xF00D;

            Assert.AreEqual(1, vm.StepInstruction());
            Assert.AreEqual(2, vm.ProgramCounter);
            Assert.AreEqual(stackOrigin - 2, vm.StackPointer);
            //Assert.AreEqual(0xF0, memBus.Read(stackOrigin - 2));
            //Assert.AreEqual(0x0D, memBus.Read(stackOrigin - 1));
            Assert.AreEqual(0xF00D, memBus.Read16(stackOrigin - 2));
        }

        [Test]
        public static void Pop()
        {
            const int stackOrigin = 0xFFFF;

            var opcodeBuilder = new OpcodeBuilder();
            opcodeBuilder.Append(Opcode.POP, RegisterName.B);

            var memBus = new DefaultMemoryBus();
            memBus.CopyInto(opcodeBuilder.GetBytes(), 0);
            //memBus.Write(0xFFFE, 0xF0);
            //memBus.Write(0xFFFF, 0x0D);
            memBus.Write16(stackOrigin - 2, 0xF00D);

            var ioBus = new EmptyIOBus();

            var vm = new VirtualMachine(memBus, ioBus);
            vm.StackPointer = stackOrigin - 2;

            Assert.AreEqual(1, vm.StepInstruction());
            Assert.AreEqual(2, vm.ProgramCounter);
            Assert.AreEqual(stackOrigin, vm.StackPointer);
            Assert.AreEqual(vm.Registers[1], 0xF00D);
        }
    }
}
