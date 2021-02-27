using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;

namespace AssemblerTwo.Lib
{
    public static class Assembler
    {
        // stream: read in chunks up to next \n or eof
        // do we even want to care about stream reading / char walking if we want
        // to do things like error printing? probablly since streams are about
        // doing the processing inline with the file reading itself rather then waiting
        // for the whole file to become loaded into memory. we want the whole file in memory anyway
        // but streams are fancy and we can hold the data in something other then a single string variable

        public static readonly Encoding TxtEncoding = Encoding.ASCII;

        public static BuildResult Build(string sourceText)
        {
            var result = new BuildResult();
            result.SourceName = string.Empty;

            var strTokens = StringTokenize(sourceText);
            result.TokenizerResult = strTokens.ToArray();

            var lexTokens = Lex(strTokens);
            result.LexerResult = lexTokens.ToArray();

            ParseResult parseResult;
            try
            {
                parseResult = Parser.Parse(lexTokens);
            }
            catch (ParserException parserEx)
            {
                var firstErrorToken = parserEx.LexicalTokenInfo.StringTokens[0];
                var firstErrorIndex = strTokens.IndexOf(firstErrorToken);

                StringTokenInfo firstLineToken = null;
                var firstSearchIndex = firstErrorIndex;
                while (firstLineToken == null)
                {
                    if (strTokens[firstSearchIndex].LineCharacterIndex == 0)
                    {
                        firstLineToken = strTokens[firstSearchIndex];
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
                    if (next < strTokens.Count)
                    {
                        if (strTokens[next].LineIndex != firstErrorToken.LineIndex)
                        {
                            lastLineToken = strTokens[lastSearchIndex];
                        }
                        else
                        {
                            lastSearchIndex++;
                        }
                    }
                    else
                    {
                        lastLineToken = strTokens[lastSearchIndex];
                    }
                }

                var firstCharacterIndex = firstLineToken.GlobalCharacterIndex;
                var lastCharacterIndex = lastLineToken.GlobalCharacterIndex + lastLineToken.ValueLength;
                var lineString = sourceText.Substring(firstCharacterIndex, lastCharacterIndex - firstCharacterIndex);
                var lineColumn = $"{firstErrorToken.LineIndex,3}|";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR");
                Console.Write($"{lineColumn}{lineString}");
                Console.Write(new string(' ', lineColumn.Length + firstErrorToken.LineCharacterIndex));
                Console.WriteLine("^");
                Console.WriteLine(parserEx.Message);
                Console.ResetColor();

                return result;
            }
            result.ParseResult = parseResult;

            var byteGroup = GenerateBytecode(parseResult.SyntaxTree, true, true);
            result.FinalBytes = byteGroup;

            return result;
        }

        /*public ParseResult ParseWithErrorWrap(List<LexicalTokenInfo> lexicalTokens)
        {

        }*/

        public static BuildResult UnprotectedBuild(string sourceText, string sourceName)
        {
            var result = new BuildResult();
            result.SourceName = sourceName;

            var strTokens = StringTokenize(sourceText);
            result.TokenizerResult = strTokens.ToArray();

            var lexTokens = Lex(strTokens);
            result.LexerResult = lexTokens.ToArray();

            var parseResult = Parser.Parse(lexTokens);
            result.ParseResult = parseResult;

            var byteGroup = GenerateBytecode(parseResult.SyntaxTree, true, true);
            result.FinalBytes = byteGroup;

            return result;
        }

        #region String Tokenizer

        /// <summary>
        /// Turn the given string into a series of tokens
        /// </summary>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        /// <remarks>
        /// The string tokenizers job is to get the source file / source text out of the way as quickly as possible.
        /// It should strike a balance between the granularity of its tokens and the ease of finishing its work
        /// and passing the token list to the next steps, where operating on groups of symbols will be much easier
        /// than working with raw text.
        /// </remarks>
        public static List<StringTokenInfo> StringTokenize(string sourceText)
        {
            if (string.IsNullOrEmpty(sourceText))
                throw new ArgumentException($"{nameof(sourceText)} must not be null or empty", nameof(sourceText));

            var stringTokens = new List<StringTokenInfo>();
            var remainingText = sourceText;

            int globalCharacterIndex = 0;
            int lineCharacterIndex = 0;
            int lineIndex = 0;

            while (remainingText.Length != 0)
            {
                var tokenInfo = FindStringTokenDefinition(remainingText);

                if (tokenInfo.Token == StringToken.Unknown)
                {
                    throw new AssemblerException($"Unknown token at ({lineIndex}:{lineCharacterIndex}): `{remainingText[0]}`");
                }

                tokenInfo.GlobalCharacterIndex = globalCharacterIndex;
                tokenInfo.LineIndex = lineIndex;
                tokenInfo.LineCharacterIndex = lineCharacterIndex;

                var tokenLength = tokenInfo.ValueLength;

                globalCharacterIndex += tokenLength;
                lineCharacterIndex += tokenLength;
                if (tokenInfo.Token == StringToken.EndOfLine)
                {
                    lineIndex++;
                    lineCharacterIndex = 0;
                }

                remainingText = remainingText.Substring(tokenLength);
                stringTokens.Add(tokenInfo);
            }

            return stringTokens;
        }

        static StringTokenInfo FindStringTokenDefinition(string remainingText)
        {
            foreach (var tokenDef in StringTokenDefinition.Table)
            {
                var (Token, Value) = tokenDef.Match(remainingText);
                if (Token != StringToken.Unknown)
                {
                    return new StringTokenInfo(Token, Value);
                }
            }
            return new StringTokenInfo(); // Token.Unknown by default
        }

        #endregion

        static readonly Dictionary<string, LexicalToken> sSymbolLexicalTokenStrings = new Dictionary<string, LexicalToken>()
        {
            ["!"] = LexicalToken.SymExclaim,
            ["#"] = LexicalToken.SymSharp,
            ["$"] = LexicalToken.SymDollar,
            ["%"] = LexicalToken.SymPercent,
            ["&"] = LexicalToken.SymAmp,
            ["'"] = LexicalToken.SymQuote,
            ["("] = LexicalToken.SymLParen,
            [")"] = LexicalToken.SymRParen,
            ["*"] = LexicalToken.SymAsterisk,
            ["+"] = LexicalToken.SymPlus,
            [","] = LexicalToken.SymComma,
            ["-"] = LexicalToken.SymMinus,
            ["."] = LexicalToken.SymPeriod,
            ["/"] = LexicalToken.SymFwdslash,
            [":"] = LexicalToken.SymColon,
            ["<"] = LexicalToken.SymLessThan,
            ["="] = LexicalToken.SymEquals,
            [">"] = LexicalToken.SymGrtrThan,
            ["?"] = LexicalToken.SymQuestMark,
            ["@"] = LexicalToken.SymAt,
            ["["] = LexicalToken.SymLSBrack,
            ["\\"] = LexicalToken.SymBackslash,
            ["]"] = LexicalToken.SymRSBrack,
            ["^"] = LexicalToken.SymCaret,
            ["`"] = LexicalToken.SymBacktick,
            ["{"] = LexicalToken.SymLCBrack,
            ["|"] = LexicalToken.SymVertBar,
            ["}"] = LexicalToken.SymRCBrack,
            ["~"] = LexicalToken.SymTilde,
        };

        public static List<LexicalTokenInfo> Lex(List<StringTokenInfo> stringTokens)
        {
            if (stringTokens == null || stringTokens.Count == 0)
                throw new ArgumentException($"{nameof(stringTokens)} must not be null or empty", nameof(stringTokens));

            var lexicalTokens = new List<LexicalTokenInfo>();
            var stringTokenQueue = new Queue<StringTokenInfo>(stringTokens);
            while (stringTokenQueue.Count != 0)
            {
                var currentStringToken = stringTokenQueue.Dequeue();
                var currentStringValue = currentStringToken.Value;
                switch (currentStringToken.Token)
                {
                    case StringToken.Unknown:
                    {
                        throw new AssemblerException($"Can't lex {nameof(StringToken.Unknown)}");
                    }
                    case StringToken.EndOfLine:
                    {
                        // Discard leading newlines in the stringTokens list, as well as collapse
                        // groups of 2 or more newlines into a single newline entry
                        if (lexicalTokens.Count() > 1 && lexicalTokens.Last().Token != LexicalToken.EndOfLine)
                        {
                            lexicalTokens.Add(new LexicalTokenInfo(LexicalToken.EndOfLine, currentStringToken));
                        }
                        continue;
                    }
                    case StringToken.Whitespace:
                    case StringToken.Comment:
                    {
                        continue;
                    }
                    case StringToken.String:
                    {
                        currentStringValue = currentStringValue.Substring(1);
                        currentStringValue = currentStringValue.Substring(0, currentStringValue.Length - 1);
                        lexicalTokens.Add(new LToken<string>(LexicalToken.String, currentStringValue, currentStringToken));
                        continue;
                    }
                    case StringToken.Symbol:
                    {
                        if (sSymbolLexicalTokenStrings.TryGetValue(currentStringValue, out LexicalToken lexicalToken))
                        {
                            lexicalTokens.Add(new LToken<string>(lexicalToken, currentStringValue, currentStringToken));
                            continue;
                        }
                        else
                        {
                            throw new AssemblerException();
                        }
                    }
                    case StringToken.HexNumber:
                    {
                        var parseString = currentStringValue.Substring(2);
                        // TODO: impossible to fail parsing?
                        var hexNumberValue = uint.Parse(parseString, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                        lexicalTokens.Add(new LToken<uint>(LexicalToken.Number, hexNumberValue, currentStringToken));
                        continue;
                    }
                    case StringToken.Number:
                    {
                        // TODO: impossible to fail parsing?
                        var numberValue = uint.Parse(currentStringValue);
                        lexicalTokens.Add(new LToken<uint>(LexicalToken.Number, numberValue, currentStringToken));
                        continue;
                    }
                    case StringToken.Identifier:
                    {
                        if (Enum.TryParse(currentStringValue, true, out RegisterName registerName))
                        {
                            lexicalTokens.Add(new LToken<RegisterName>(LexicalToken.Register, registerName, currentStringToken));
                            continue;
                        }
                        else if (Enum.TryParse(currentStringValue, true, out Opcode opcode))
                        {
                            lexicalTokens.Add(new LToken<Opcode>(LexicalToken.Opcode, opcode, currentStringToken));
                            continue;
                        }
                        else if (Enum.TryParse(currentStringValue, true, out Directive directiveName))
                        {
                            lexicalTokens.Add(new LToken<Directive>(LexicalToken.Directive, directiveName, currentStringToken));
                            continue;
                        }
                        else
                        {
                            lexicalTokens.Add(new LToken<string>(LexicalToken.Identifier, currentStringValue, currentStringToken));
                            continue;
                        }
                        throw new UnknownStateException("Unknown State! Should have continued from the preceding statement!");
                    }
                    default:
                    {
                        throw new AssemblerException($"Unhandled case {nameof(StringToken)}.{currentStringToken.Token}");
                    }
                }
                throw new UnknownStateException("Unknown State! Should always continue inside preceding switch!");
            }
            return lexicalTokens;
        }

        public static BytecodeGroup GenerateBytecode(List<ASTNode> astNodes, bool generateSymbolTable, bool symbolTableDebugMode)
        {
            var totalBytes = 0;
            var labelDefines = new Dictionary<string, int>();
            var labelRefs = new Dictionary<string, List<int>>();
            var relocationIndicies = new HashSet<int>(); // TODO

            for (int x = 0; x < astNodes.Count; x++)
            {
                var node = astNodes[x];
                if (node.Label != null)
                {
                    labelDefines.Add(node.Label.Value, totalBytes);
                    labelRefs.Add(node.Label.Value, new List<int>());
                }

                switch (node.Type)
                {
                    case ASTNodeType.Binary:
                    {
                        var astBinary = (ASTBinary)node;
                        totalBytes += astBinary.Bytes.Length;
                        break;
                    }
                    case ASTNodeType.Opcode:
                    {
                        var astOpcode = (ASTOpcode)node;
                        totalBytes += astOpcode.GetDef().ByteLength;
                        break;
                    }
                }
            }

            var finalBytes = new byte[totalBytes];
            var byteIndex = 0;
            for (int x = 0; x < astNodes.Count; x++)
            {
                var node = astNodes[x];
                switch (node.Type)
                {
                    case ASTNodeType.Binary:
                    {
                        var astBinary = (ASTBinary)node;
                        var binaryBytes = astBinary.Bytes;
                        Array.Copy(binaryBytes, 0, finalBytes, byteIndex, binaryBytes.Length);
                        byteIndex += binaryBytes.Length;
                        break;
                    }
                    case ASTNodeType.Opcode:
                    {
                        var astOpcode = (ASTOpcode)node;
                        var opcodeDef = astOpcode.GetDef();
                        switch (opcodeDef.ArgumentType)
                        {
                            case OpcodeArgumentType.NONE:
                            {
                                var opcodeWord = opcodeDef.CodeHint;
                                var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                                Array.Copy(opcodeBytes, 0, finalBytes, byteIndex, opcodeBytes.Length);
                                byteIndex += opcodeBytes.Length;
                                break;
                            }
                            case OpcodeArgumentType.REG:
                            {
                                var opcodeWord = opcodeDef.CodeHint;
                                opcodeWord += (byte)astOpcode.RegisterA;
                                var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                                Array.Copy(opcodeBytes, 0, finalBytes, byteIndex, opcodeBytes.Length);
                                byteIndex += opcodeBytes.Length;
                                break;
                            }
                            case OpcodeArgumentType.IMMED:
                            {
                                var opcodeWord = opcodeDef.CodeHint;
                                var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                                Array.Copy(opcodeBytes, 0, finalBytes, byteIndex, opcodeBytes.Length);
                                byteIndex += opcodeBytes.Length;

                                UInt16 immedWord;
                                if (astOpcode.ImmedConstant.HasValue)
                                {
                                    immedWord = (UInt16)astOpcode.ImmedConstant.Value;
                                }
                                else
                                {
                                    immedWord = (UInt16)labelDefines[astOpcode.ImmedIdent.Value];
                                    labelRefs[astOpcode.ImmedIdent.Value].Add(byteIndex);
                                }
                                var immedBytes = BitConverter.GetBytes(immedWord).Reverse().ToArray();
                                Array.Copy(immedBytes, 0, finalBytes, byteIndex, immedBytes.Length);
                                byteIndex += immedBytes.Length;
                                break;
                            }
                            case OpcodeArgumentType.REG_REG:
                            {
                                var opcodeWord = opcodeDef.CodeHint;
                                opcodeWord += (UInt16)(((byte)astOpcode.RegisterA) << 4);
                                opcodeWord += (byte)astOpcode.RegisterB;
                                var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                                Array.Copy(opcodeBytes, 0, finalBytes, byteIndex, opcodeBytes.Length);
                                byteIndex += opcodeBytes.Length;
                                break;
                            }
                            case OpcodeArgumentType.REG_IMMED:
                            {
                                var opcodeWord = opcodeDef.CodeHint;
                                opcodeWord += (byte)astOpcode.RegisterA;
                                var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                                Array.Copy(opcodeBytes, 0, finalBytes, byteIndex, opcodeBytes.Length);
                                byteIndex += opcodeBytes.Length;

                                UInt16 immedWord;
                                if (astOpcode.ImmedConstant.HasValue)
                                {
                                    immedWord = (UInt16)astOpcode.ImmedConstant.Value;
                                }
                                else
                                {
                                    immedWord = (UInt16)labelDefines[astOpcode.ImmedIdent.Value];
                                    labelRefs[astOpcode.ImmedIdent.Value].Add(byteIndex);
                                }
                                var immedBytes = BitConverter.GetBytes(immedWord).Reverse().ToArray();
                                Array.Copy(immedBytes, 0, finalBytes, byteIndex, immedBytes.Length);
                                byteIndex += immedBytes.Length;
                                break;
                            }
                            case OpcodeArgumentType.REG_REG_IMMED:
                            {
                                var opcodeWord = opcodeDef.CodeHint;
                                opcodeWord += (UInt16)(((byte)astOpcode.RegisterA) << 4);
                                opcodeWord += (byte)astOpcode.RegisterB;
                                var opcodeBytes = BitConverter.GetBytes(opcodeWord).Reverse().ToArray();
                                Array.Copy(opcodeBytes, 0, finalBytes, byteIndex, opcodeBytes.Length);
                                byteIndex += opcodeBytes.Length;

                                UInt16 immedWord;
                                if (astOpcode.ImmedConstant.HasValue)
                                {
                                    immedWord = (UInt16)astOpcode.ImmedConstant.Value;
                                }
                                else
                                {
                                    immedWord = (UInt16)labelDefines[astOpcode.ImmedIdent.Value];
                                    labelRefs[astOpcode.ImmedIdent.Value].Add(byteIndex);
                                }
                                var immedBytes = BitConverter.GetBytes(immedWord).Reverse().ToArray();
                                Array.Copy(immedBytes, 0, finalBytes, byteIndex, immedBytes.Length);
                                byteIndex += immedBytes.Length;
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            SymbolTable symbolTable = null;
            if (generateSymbolTable)
            {
                symbolTable = GenerateSymbolTable(labelDefines, labelRefs, symbolTableDebugMode);
            }

            return new BytecodeGroup
            {
                LabelDefines = labelDefines,
                LabelRefs = labelRefs,
                RelocationIndicies = relocationIndicies,
                SymbolTable = symbolTable,
                Bytes = finalBytes,
            };
        }

        private static SymbolTable GenerateSymbolTable(Dictionary<string, int> labelDefines, Dictionary<string, List<int>> labelRefs, bool preserveNames)
        {
            var newSymbolTable = new SymbolTable();

            var labelDefinePairs = labelDefines.ToArray();
            var labelReferencePairs = labelRefs.ToArray();

            ushort[] symbolDefineAddresses = new ushort[labelDefinePairs.Length];
            for (int x = 0; x < labelDefinePairs.Length; x++)
            {
                symbolDefineAddresses[x] = (ushort)labelDefinePairs[x].Value;
            }

            ushort[][] nonExternalReferenceAddresses = new ushort[labelReferencePairs.Length][];
            for (int x = 0; x < labelReferencePairs.Length; x++)
            {
                var references = labelReferencePairs[x].Value;
                if (references != null)
                {
                    var shortsArray = references.Select(x => (ushort)x).ToArray();
                    nonExternalReferenceAddresses[x] = shortsArray;
                }
                else
                {
                    nonExternalReferenceAddresses[x] = Array.Empty<ushort>();
                }
            }

            string[] symbolDefineNames = Array.Empty<string>();
            if (preserveNames)
            {
                symbolDefineNames = new string[labelDefinePairs.Length];
                for (int x = 0; x < labelDefinePairs.Length; x++)
                {
                    symbolDefineNames[x] = labelDefinePairs[x].Key;
                }
            }

            var flag = (preserveNames) ? SymbolTable.DebugFlag : SymbolTable.ReleaseFlag;

            return new SymbolTable
            {
                Flag = flag,
                SymbolDefineAddresses = symbolDefineAddresses,
                NonExternalReferenceAddresses = nonExternalReferenceAddresses,
                ExternalSymbols = Array.Empty<ExternalSymbol>(),
                PublicSymbols = Array.Empty<PublicSymbol>(),
                ExtraDeltaAddresses = Array.Empty<ushort>(),
                SymbolDefineNames = symbolDefineNames,
            };
        }

        public class BuildResult
        {
            public string SourceName;
            public StringTokenInfo[] TokenizerResult;
            public LexicalTokenInfo[] LexerResult;
            public ParseResult ParseResult;
            public BytecodeGroup FinalBytes;
        }
    }
}
