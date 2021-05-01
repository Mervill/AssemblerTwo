using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;

namespace AssemblerTwo.Lib
{
    public class Assembler
    {
        public enum LogLevel
        {
            None,

            Error,
            Warn,
            Info,

            Debug,

            Verbose,
        }

        public LogLevel MaxLogLevel { get; set; } = LogLevel.Verbose;

        StreamWriter mLogWriter;
        
        string mSourceText;

        List<StringTokenInfo> mStringTokens;
        List<LexicalTokenInfo> mLexedTokens;
        ParseResult mParseResult;
        BytecodeGroup mBytecodeGroup;

        public Assembler(StreamWriter logWriter)
        {
            mLogWriter = logWriter;
        }

        public AssemblerBuildResult Build(string sourceText)
        {
            var result = new AssemblerBuildResult();
            result.SourceName = string.Empty;

            mSourceText = sourceText;

            // Tokenize

            mStringTokens = StaticAssembler.StringTokenize(mSourceText);
            result.TokenizerResult = mStringTokens.ToArray();

            if (IsLogLevel(LogLevel.Debug))
            {
                WriteLine(LogLevel.Debug, "String Tokens");
                var dump = DumpUtility.DumpStringTokens(mStringTokens, hideEoLCharacters: false, csvFormat: false);
                Write(LogLevel.Debug, dump);
            }

            // Lex

            mLexedTokens = StaticAssembler.Lex(mStringTokens);
            result.LexerResult = mLexedTokens.ToArray();

            // Parse

            mParseResult = Parse();
            if (mParseResult == null)
            {
                // TODO: Set final error state
                return result;
            }
            result.ParseResult = mParseResult;

            if (mParseResult.NameDirectiveToken != null)
            {
                WriteLine(LogLevel.Info, $"AST Name: {mParseResult.NameDirectiveToken.Value}");
            }

            if (IsLogLevel(LogLevel.Debug))
            {
                WriteLine(LogLevel.Debug, "AST Nodes");
                var dump = DumpUtility.DumpASTNodes(mParseResult.SyntaxTree, true, true);
                Write(LogLevel.Debug, dump);
            }

            // Bytecode

            mBytecodeGroup = StaticAssembler.GenerateBytecode(mParseResult.SyntaxTree, true, true);
            result.FinalBytes = mBytecodeGroup;

            return result;
        }

        private ParseResult Parse()
        {
            ParseResult parseResult = null;
            try
            {
                parseResult = Parser.Parse(mLexedTokens);
            }
            catch (ParserException parserEx)
            {
                var firstErrorToken = parserEx.LexicalTokenInfo.StringTokens[0];
                var firstErrorIndex = mStringTokens.IndexOf(firstErrorToken);

                StringTokenInfo firstLineToken = null;
                var firstSearchIndex = firstErrorIndex;
                while (firstLineToken == null)
                {
                    if (mStringTokens[firstSearchIndex].LineCharacterIndex == 0)
                    {
                        firstLineToken = mStringTokens[firstSearchIndex];
                    }
                    else
                    {
                        firstSearchIndex--;
                    }
                }

                StringTokenInfo lastLineToken = null;
                var lastSearchIndex = firstErrorIndex;
                while (lastLineToken == null)
                {
                    var next = lastSearchIndex + 1;
                    if (next < mStringTokens.Count)
                    {
                        if (mStringTokens[next].LineIndex != firstErrorToken.LineIndex)
                        {
                            lastLineToken = mStringTokens[lastSearchIndex];
                        }
                        else
                        {
                            lastSearchIndex++;
                        }
                    }
                    else
                    {
                        lastLineToken = mStringTokens[lastSearchIndex];
                    }
                }

                var firstCharacterIndex = firstLineToken.GlobalCharacterIndex;
                var lastCharacterIndex = lastLineToken.GlobalCharacterIndex + lastLineToken.ValueLength;
                var lineString = mSourceText.Substring(firstCharacterIndex, lastCharacterIndex - firstCharacterIndex);
                var lineColumn = $"{firstErrorToken.LineIndex + 1,3}|";
                //Console.ForegroundColor = ConsoleColor.Red;
                WriteLine(LogLevel.Error, true, "ERROR");
                Write(LogLevel.Error, $"{lineColumn}{lineString}");
                Write(LogLevel.Error, new string(' ', lineColumn.Length + firstErrorToken.LineCharacterIndex));
                Write(LogLevel.Error, "^");
                WriteLine(LogLevel.Error, false, "");
                WriteLine(LogLevel.Error, false, parserEx.Message);
                //Console.ResetColor();
            }
            return parseResult;
        }

        void WriteLine(LogLevel logLevel, string message)
        {
            WriteLine(logLevel, true, message);
        }

        void WriteLine(LogLevel logLevel, bool prefix, string message)
        {
            if (IsLogLevel(logLevel))
            {
                var finalMessage = message;
                if (prefix)
                {
                    finalMessage = string.Format("[{0}] {1}", logLevel, message);
                }
                mLogWriter.WriteLine(finalMessage);
            }
        }

        void Write(LogLevel logLevel, string message)
        {
            if (IsLogLevel(logLevel))
            {
                mLogWriter.Write(message);
            }
        }

        bool IsLogLevel(LogLevel logLevel)
            => logLevel <= MaxLogLevel;
    }
}
