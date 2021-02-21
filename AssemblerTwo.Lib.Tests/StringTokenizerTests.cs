using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;

namespace AssemblerTwo.Lib.Tests
{
    [TestFixture]
    public static class StringTokenizerTests
    {
        [TestCase(null)]
        [TestCase("")]
        public static void NullOrEmptySourceText(string sourceText)
        {
            Assert.Throws<ArgumentException>(() => Assembler.StringTokenize(sourceText));
        }

        /*
        [Test, TestCaseSource(nameof(SingleTokenCases))]
        public static void SingleToken(string sourceText, StringToken expectedToken)
        {
            var stringTokenList = Assembler.StringTokenize(sourceText);
            Assert.That(stringTokenList.Count == 1, "should return exactly one token");

            StringTokenInfo token = stringTokenList.First();
            Assert.AreEqual(expectedToken, token.Token);
            Assert.AreEqual(sourceText, token.Value);
            Assert.AreEqual(0, token.GlobalCharacterIndex);
            Assert.AreEqual(0, token.LineIndex);
            Assert.AreEqual(0, token.LineCharacterIndex);
        }

        public static System.Collections.IEnumerable SingleTokenCases()
        {
            yield return new TestCaseData("\n", StringToken.EndOfLine);

            yield return new TestCaseData(" ", StringToken.Whitespace);
            yield return new TestCaseData("\t", StringToken.Whitespace);
            yield return new TestCaseData(" \t", StringToken.Whitespace);

            yield return new TestCaseData(";", StringToken.Comment);
            yield return new TestCaseData("; Hello World", StringToken.Comment);
            yield return new TestCaseData("; Hello\tWorld", StringToken.Comment);
            yield return new TestCaseData("; Hello 0123456789 World", StringToken.Comment);
            yield return new TestCaseData("; Hello 0xF00D World", StringToken.Comment);
            
            yield return new TestCaseData("\"\"", StringToken.String);
            yield return new TestCaseData("\" \"", StringToken.String);
            yield return new TestCaseData("\"Hello World\"", StringToken.String);
            yield return new TestCaseData("\"Hello\tWorld\"", StringToken.String);
            yield return new TestCaseData("\"Hello 0xF00D World\"", StringToken.String);
            yield return new TestCaseData("\"; Hello World\"", StringToken.String);
            yield return new TestCaseData("\"; Hello\tWorld\"", StringToken.String);
            yield return new TestCaseData("\"; Hello 0xF00D World\"", StringToken.String);

            yield return new TestCaseData("0x0", StringToken.HexNumber);
            yield return new TestCaseData("0xF00D", StringToken.HexNumber);
            yield return new TestCaseData("0x123456789", StringToken.HexNumber);
            yield return new TestCaseData("0xABCDEF", StringToken.HexNumber);
            yield return new TestCaseData("0xDEAD", StringToken.HexNumber);

            yield return new TestCaseData("0", StringToken.Number);
            yield return new TestCaseData("42", StringToken.Number);
            yield return new TestCaseData("8675309", StringToken.Number); // youtube.com/watch?v=tHL2XeuA6Yg
            yield return new TestCaseData("0123456789", StringToken.Number);
            yield return new TestCaseData("9876543210", StringToken.Number);

            yield return new TestCaseData("HelloWorld", StringToken.Identifier);
            yield return new TestCaseData("Hello_World", StringToken.Identifier);
            yield return new TestCaseData("H3110W0r1d", StringToken.Identifier);
            yield return new TestCaseData("H3110_W0r1d", StringToken.Identifier);
            yield return new TestCaseData("_HelloWorld", StringToken.Identifier);
            yield return new TestCaseData("_Hello_World", StringToken.Identifier);
            yield return new TestCaseData("_H3110W0r1d", StringToken.Identifier);
            yield return new TestCaseData("_H3110_W0r1d", StringToken.Identifier);
            yield return new TestCaseData("_42", StringToken.Identifier);
            yield return new TestCaseData("_0xDEAD", StringToken.Identifier);
        }
        */

        [Test, TestCaseSource(nameof(CompareFileUniformStructureCases))]
        public static void CompareFileUniformStructure(string sourceTextPath, StringToken expectedUniformToken)
        {
            var sourceText = File.ReadAllText(sourceTextPath);
            var sourceTextLines = sourceText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var stringTokenList = Assembler.StringTokenize(sourceText);
            var globalCharacterIndex = 0;
            var nextEndOfLineIndex = 0;
            for (int x = 0; x < stringTokenList.Count; x++)
            {
                var token = stringTokenList[x];
                if (x == 0)
                {
                    Assert.AreEqual(StringToken.Comment, token.Token, $"[{x + 1}] {token.Token}: {token.Value}");
                    Assert.AreEqual(sourceTextLines[0], token.Value, $"[{x + 1}] {token.Token}: {token.Value}");
                    Assert.AreEqual(0, token.GlobalCharacterIndex, nameof(token.GlobalCharacterIndex));
                    Assert.AreEqual(0, token.LineIndex, nameof(token.LineIndex));
                    Assert.AreEqual(0, token.LineCharacterIndex, nameof(token.LineCharacterIndex));
                    globalCharacterIndex = token.ValueLength;
                    nextEndOfLineIndex = token.ValueLength;
                }
                else if (x % 2 == 0)
                {
                    Assert.AreEqual(expectedUniformToken, token.Token, $"[{x + 1}] {token.Token}: {token.Value}");
                    Assert.AreEqual(sourceTextLines[x / 2], token.Value, $"[{x + 1}] {token.Token}: {token.Value}");
                    Assert.AreEqual(globalCharacterIndex, token.GlobalCharacterIndex, nameof(token.GlobalCharacterIndex));
                    Assert.AreEqual(x / 2, token.LineIndex, nameof(token.LineIndex));
                    Assert.AreEqual(0, token.LineCharacterIndex, nameof(token.LineCharacterIndex));
                    globalCharacterIndex += token.ValueLength;
                    nextEndOfLineIndex = token.ValueLength;
                }
                else
                {
                    Assert.AreEqual(StringToken.EndOfLine, token.Token, $"[{x + 1}] {token.Token}: {token.Value}");
                    Assert.That("\r\n" == token.Value || "\n" == token.Value, $"[{x + 1}] {token.Token}: {token.Value}"); // oh .. hmm, this is something to circle back too
                    Assert.AreEqual(globalCharacterIndex, token.GlobalCharacterIndex, nameof(token.GlobalCharacterIndex));
                    Assert.AreEqual(x / 2, token.LineIndex, nameof(token.LineIndex));
                    Assert.AreEqual(nextEndOfLineIndex, token.LineCharacterIndex, nameof(token.LineCharacterIndex));
                    globalCharacterIndex += token.ValueLength;
                }
            }
        }

        public static System.Collections.IEnumerable CompareFileUniformStructureCases()
        {
            yield return new TestCaseData("./Data/StringTokenizer/comment.input.txt", StringToken.Comment);
            yield return new TestCaseData("./Data/StringTokenizer/string.input.txt", StringToken.String);
            yield return new TestCaseData("./Data/StringTokenizer/hexnumber.input.txt", StringToken.HexNumber);
            yield return new TestCaseData("./Data/StringTokenizer/number.input.txt", StringToken.Number);
            yield return new TestCaseData("./Data/StringTokenizer/identifier.input.txt", StringToken.Identifier);
        }

        [Test, TestCaseSource(nameof(CompareFileAgainstDumpCases))]
        public static void CompareFileAgainstDump(string sourceTextPath, string expectedDumpPath)
        {
            var sourceText = File.ReadAllText(sourceTextPath);
            string expectedDumpText = File.ReadAllText(expectedDumpPath);

            var stringTokenList = Assembler.StringTokenize(sourceText);
            var stringTokenDump = DumpUtility.DumpStringTokens(stringTokenList, showIndices: false);
            Assert.AreEqual(expectedDumpText, stringTokenDump);
        }

        public static System.Collections.IEnumerable CompareFileAgainstDumpCases()
        {
            yield return new TestCaseData("./Data/StringTokenizer/qbf.input.txt", "./Data/StringTokenizer/qbf.output.txt");
            yield return new TestCaseData("./Data/StringTokenizer/lorem.input.txt", "./Data/StringTokenizer/lorem.output.txt");
            yield return new TestCaseData("./Data/StringTokenizer/ipsum.input.txt", "./Data/StringTokenizer/ipsum.output.txt");

            yield return new TestCaseData("./Data/StringTokenizer/comment.input.txt", "./Data/StringTokenizer/comment.output.txt");
            yield return new TestCaseData("./Data/StringTokenizer/string.input.txt", "./Data/StringTokenizer/string.output.txt");
            yield return new TestCaseData("./Data/StringTokenizer/hexnumber.input.txt", "./Data/StringTokenizer/hexnumber.output.txt");
            yield return new TestCaseData("./Data/StringTokenizer/number.input.txt", "./Data/StringTokenizer/number.output.txt");
            yield return new TestCaseData("./Data/StringTokenizer/identifier.input.txt", "./Data/StringTokenizer/identifier.output.txt");
        }

        [Test, TestCaseSource(nameof(FileTextRoundTripCases))]
        public static void FileTextRoundTrip(string sourceTextPath)
        {
            // Note: Test reqires a file with at least one line ending
            var sourceText = File.ReadAllText(sourceTextPath);
            var sourceTextLines = sourceText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var stringTokenList = Assembler.StringTokenize(sourceText);
            //var fileIsWindowsLineEndings = (stringTokenList.First(x => x.Token == StringToken.EndOfLine).Value == "\r\n");
            var roundTripSourceText = new StringBuilder();
            var globalCharacterIndex = 0;
            var lineIndex = 0;
            var lineCharacterIndex = 0;
            for (int x = 0; x < stringTokenList.Count; x++)
            {
                var token = stringTokenList[x];
                var sourceFragment = sourceText.Substring(token.GlobalCharacterIndex, token.ValueLength);
                Assert.AreEqual(sourceFragment, token.Value);
                Assert.AreEqual(globalCharacterIndex, token.GlobalCharacterIndex);
                Assert.AreEqual(lineIndex, token.LineIndex);
                Assert.AreEqual(lineCharacterIndex, token.LineCharacterIndex);
                globalCharacterIndex += token.ValueLength;
                lineCharacterIndex += token.ValueLength;
                if (token.Token != StringToken.EndOfLine)
                {
                    var lineFragment = sourceTextLines[token.LineIndex].Substring(token.LineCharacterIndex, token.ValueLength);
                    Assert.AreEqual(lineFragment, token.Value);
                }
                else
                {
                    lineIndex += 1;
                    lineCharacterIndex = 0;
                }
                roundTripSourceText.Append(token.Value);
            }
            var last = stringTokenList.Last();
            Assert.AreEqual(sourceText.Length, last.GlobalCharacterIndex + last.ValueLength);
            Assert.AreEqual(sourceText.Length, roundTripSourceText.Length);
            Assert.AreEqual(sourceText, roundTripSourceText.ToString());
        }

        public static System.Collections.IEnumerable FileTextRoundTripCases()
        {
            yield return new TestCaseData("./Data/StringTokenizer/qbf.input.txt");
            yield return new TestCaseData("./Data/StringTokenizer/lorem.input.txt");
            yield return new TestCaseData("./Data/StringTokenizer/ipsum.input.txt");

            yield return new TestCaseData("./Data/StringTokenizer/comment.input.txt");
            yield return new TestCaseData("./Data/StringTokenizer/string.input.txt");
            yield return new TestCaseData("./Data/StringTokenizer/hexnumber.input.txt");
            yield return new TestCaseData("./Data/StringTokenizer/number.input.txt");
            yield return new TestCaseData("./Data/StringTokenizer/identifier.input.txt");
        }
    }
}
