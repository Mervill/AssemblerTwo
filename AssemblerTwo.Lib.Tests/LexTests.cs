using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;

namespace AssemblerTwo.Lib.Tests
{
    [TestFixture]
    public class LexTests
    {
        [Test, TestCaseSource(nameof(DoesntAcceptNullOrEmptyCases))]
        public static void DoesntAcceptNullOrEmpty(List<StringTokenInfo> sourceTokens)
        {
            Assert.Throws<ArgumentException>(() => StaticAssembler.Lex(sourceTokens));
        }

        public static System.Collections.IEnumerable DoesntAcceptNullOrEmptyCases()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(new List<StringTokenInfo>());
        }

        [Test]
        public static void ThowOnUnknownToken()
        {
            // Lexer should thow an exception if it encounters an unknown token
            // anywhere in the input list
            var stringToken = new StringTokenInfo(StringToken.Unknown, string.Empty);
            var stringTokenList = new List<StringTokenInfo>();
            stringTokenList.Add(stringToken);
            Assert.Throws<AssemblerException>(() => StaticAssembler.Lex(stringTokenList));
        }

        public static void SimpleLexTest()
        {

        }
    }
}
