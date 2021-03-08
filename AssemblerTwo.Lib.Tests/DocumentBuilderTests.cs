using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AssemblerTwo.Lib.Utility;

using NUnit.Framework;

namespace AssemblerTwo.Lib.Tests
{
    [TestFixture]
    public static class DocumentBuilderTests
    {
        [TestCase(Opcode.NOP,  null,           null,           null,   new[] { 0xFFF0 })]         // NONE
        [TestCase(Opcode.INC,  RegisterName.B, null,           null,   new[] { 0xAFC1 })]         // REG
        [TestCase(Opcode.JUMP, null,           null,           0xF00D, new[] { 0xDEAA, 0xF00D })] // IMMED
        [TestCase(Opcode.ADD,  RegisterName.B, RegisterName.C, null,   new[] { 0xA012 })]         // REG_REG
        [TestCase(Opcode.ADDI, RegisterName.B, null,           0xF00D, new[] { 0xAF01, 0xF00D })] // REG_IMMED
        [TestCase(Opcode.JLT,  RegisterName.B, RegisterName.C, 0xF00D, new[] { 0xB012, 0xF00D })] // REG_REG_IMMED
        public static void GeneratesCorrectBytes(Opcode opcode, RegisterName? regA, RegisterName? regB, int? immed, int[] expectedBytes)
        {
            var documentBuilder = new DocumentBuilder();
            ushort? immedShort = immed.HasValue ? (ushort)immed.Value : null;
            documentBuilder.Append(opcode, regA, regB, immedShort);
            var resultBytes = documentBuilder.GetBytes();
            var expectedBytesList = expectedBytes.SelectMany(x => BitConverter.GetBytes((UInt16)x).Reverse()).ToArray();
            Assert.AreEqual(expectedBytesList, resultBytes);
        }

        [Test]
        public static void ClassAppend()
        {
            var documentBuilder = new DocumentBuilder();
            Assert.AreEqual(0, documentBuilder.Count);
            Assert.IsEmpty(documentBuilder.GetBytes());
            documentBuilder.Append(Opcode.NOP);
            Assert.AreEqual(new byte[] { 0xFF, 0xF0 }, documentBuilder.GetBytes());
            documentBuilder.Append(Opcode.HALT);
            Assert.AreEqual(new byte[] { 0xFF, 0xF0, 0xDE, 0xAD }, documentBuilder.GetBytes());
        }

        [Test]
        public static void ClassClear()
        {
            var documentBuilder = new DocumentBuilder();
            Assert.AreEqual(0, documentBuilder.Count);
            Assert.IsEmpty(documentBuilder.GetBytes());
            documentBuilder.Append(Opcode.NOP);
            Assert.AreEqual(new byte[] { 0xFF, 0xF0 }, documentBuilder.GetBytes());
            documentBuilder.Append(Opcode.HALT);
            Assert.AreEqual(new byte[] { 0xFF, 0xF0, 0xDE, 0xAD }, documentBuilder.GetBytes());
            documentBuilder.Clear();
            Assert.AreEqual(0, documentBuilder.Count);
            Assert.IsEmpty(documentBuilder.GetBytes());
        }
    }
}
