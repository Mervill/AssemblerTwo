using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblerTwo.Lib.Utility;

using NUnit.Framework;

namespace AssemblerTwo.Lib.Tests
{
    [TestFixture]
    public static class OpcodeBuilderTests
    {
        [TestCase(Opcode.NOP,  null,           null,           null,   new[] { 0xFFF0 })]         // NONE
        [TestCase(Opcode.INC,  RegisterName.B, null,           null,   new[] { 0xAFC1 })]         // REG
        [TestCase(Opcode.JUMP, null,           null,           0xF00D, new[] { 0xDEAA, 0xF00D })] // IMMED
        [TestCase(Opcode.ADD,  RegisterName.B, RegisterName.C, null,   new[] { 0xA012 })]         // REG_REG
        [TestCase(Opcode.ADDI, RegisterName.B, null,           0xF00D, new[] { 0xAF01, 0xF00D })] // REG_IMMED
        [TestCase(Opcode.JLT,  RegisterName.B, RegisterName.C, 0xF00D, new[] { 0xB012, 0xF00D })] // REG_REG_IMMED
        public static void GeneratesCorrectBytes(Opcode opcode, RegisterName? regA, RegisterName? regB, int? immed, int[] expectedBytes)
        {
            ushort? immedShort = immed.HasValue ? (ushort)immed.Value : null;
            var resultBytes = OpcodeBuilder.Create(opcode, regA, regB, immedShort);
            var expectedBytesList = expectedBytes.SelectMany(x => BitConverter.GetBytes((UInt16)x).Reverse()).ToArray();
            Assert.AreEqual(expectedBytesList, resultBytes);
        }

        [Test]
        public static void ClassAppend()
        {
            var opcodeBuilder = new OpcodeBuilder();
            Assert.AreEqual(0, opcodeBuilder.Count);
            Assert.IsEmpty(opcodeBuilder.GetBytes());
            opcodeBuilder.Append(Opcode.NOP);
            Assert.AreEqual(new byte[] { 0xFF, 0xF0 }, opcodeBuilder.GetBytes());
            opcodeBuilder.Append(Opcode.HALT);
            Assert.AreEqual(new byte[] { 0xFF, 0xF0, 0xDE, 0xAD }, opcodeBuilder.GetBytes());
        }

        [Test]
        public static void ClassClear()
        {
            var opcodeBuilder = new OpcodeBuilder();
            Assert.AreEqual(0, opcodeBuilder.Count);
            Assert.IsEmpty(opcodeBuilder.GetBytes());
            opcodeBuilder.Append(Opcode.NOP);
            Assert.AreEqual(new byte[] { 0xFF, 0xF0 }, opcodeBuilder.GetBytes());
            opcodeBuilder.Append(Opcode.HALT);
            Assert.AreEqual(new byte[] { 0xFF, 0xF0, 0xDE, 0xAD }, opcodeBuilder.GetBytes());
            opcodeBuilder.Clear();
            Assert.AreEqual(0, opcodeBuilder.Count);
            Assert.IsEmpty(opcodeBuilder.GetBytes());
        }
    }
}
