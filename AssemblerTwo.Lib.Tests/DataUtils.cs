using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblerTwo.Lib.Tests
{
    public class DataUtils
    {
        public const string Whitespace     = " \t\r";
        public const string UpperCaseAlpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string LowerCaseAlpha = "abcdefghijklmnopqrstuvwxyz";
        public const string NumberDigits   = "0123456789";

        public static Random rnd = new Random();

        public static string WhitespaceString(int minLength, int maxLength, string including = "")
        {
            var length = rnd.Next(minLength, maxLength);
            return WhitespaceString(length, including);
        }

        public static string WhitespaceString(int length, string including = "")
        {
            List<char> sources = new List<char>();
            sources.AddRange(Whitespace);
            sources.AddRange(including);
            return RandomFromSources(length, sources);
        }

        public static string AlphaString(int minLength, int maxLength, string including = "")
        {
            var length = rnd.Next(minLength, maxLength);
            return AlphaString(length, including);
        }

        public static string AlphaString(int length, string including = "")
        {
            List<char> sources = new List<char>();
            sources.AddRange(UpperCaseAlpha);
            sources.AddRange(LowerCaseAlpha);
            sources.AddRange(including);
            return RandomFromSources(length, sources);
        }

        public static string NumberString(int minLength, int maxLength, string including = "")
        {
            var length = rnd.Next(minLength, maxLength);
            return NumberString(length, including);
        }

        public static string NumberString(int length, string including = "")
        {
            List<char> sources = new List<char>();
            sources.AddRange(NumberDigits);
            sources.AddRange(including);
            return RandomFromSources(length, sources);
        }

        public static string RandomFromSources(int minLength, int maxLength, List<char> sources)
        {
            var length = rnd.Next(minLength, maxLength);
            return RandomFromSources(length, sources);
        }

        public static string RandomFromSources(int length, List<char> sources)
        {
            var sb = new StringBuilder(length);
            for (int x = 0; x < length; x++)
            {
                var nextChar = sources[rnd.Next(0, sources.Count)];
                sb.Append(nextChar);
            }
            return sb.ToString();
        }

    }
}
