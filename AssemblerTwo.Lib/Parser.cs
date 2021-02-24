using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace AssemblerTwo.Lib
{
    public static class Parser
    {
        public static ParseResult Parse(List<LexicalTokenInfo> lexicalTokens)
        {
            // TODO: LabelInfo object for better label control
            if (lexicalTokens == null || lexicalTokens.Count == 0)
                throw new ArgumentException($"{nameof(lexicalTokens)} must not be null or empty", nameof(lexicalTokens));

            var partialResult = new ParseResult();

            // shallow copy
            lexicalTokens = new(lexicalTokens);

            LToken<string> unanchoredLabel = null;
            Dictionary<string, LToken<string>> identsDefined = new Dictionary<string, LToken<string>>();
            List<LToken<string>> identsRefrenced = new List<LToken<string>>();

            int linePosition = 0;
#if DEBUG
            var lastlexicalTokensCount = 0;
#endif
            while (lexicalTokens.Count != 0)
            {
#if DEBUG
                if (lastlexicalTokensCount != lexicalTokens.Count)
                {
                    lastlexicalTokensCount = lexicalTokens.Count;
                }
                else
                {
                    throw new AssemblerException("No tokens consumed!");
                }
#endif
                var currentLToken = lexicalTokens.First();
                switch (currentLToken.Token)
                {
                    case LexicalToken.Unknown:
                    {
                        throw new AssemblerException();
                    }
                    case LexicalToken.EndOfLine:
                    {
                        linePosition = 0;
                        lexicalTokens.RemoveRange(0, 1);
                        break;
                    }
                    case LexicalToken.Opcode:
                    {
                        ParseOpcode(currentLToken, lexicalTokens, partialResult, ref unanchoredLabel);
                        break;
                    }
                    case LexicalToken.Directive:
                    {
                        ParseDirective(currentLToken, lexicalTokens, partialResult, ref unanchoredLabel);
                        break;
                    }
                    case LexicalToken.Identifier:
                    {
                        var lIdent = (LToken<string>)currentLToken;
                        if (linePosition == 0)
                        {
                            var nextLToken = ExpectLToken(lexicalTokens, 1, LexicalToken.SymColon, false);
                            if (nextLToken != null)
                            {
                                if (identsDefined.TryAdd(lIdent.Value, lIdent))
                                {
                                    unanchoredLabel = lIdent;
                                    linePosition = 1;
                                    lexicalTokens.RemoveRange(0, 2);
                                    break;
                                }
                                else
                                {
                                    throw new ParserException($"Duplicate label `{lIdent.Value}`", lIdent);
                                }
                            }
                            else
                            {
                                var message = $"Unknown opcode or directive `{lIdent.Value}`";
                                var location = lIdent.StringTokens[0].ToString(true, true, false);
                                throw new ParserException($"{message}{Environment.NewLine}{location}", lIdent);
                            }
                        }
                        else
                        {
                            var message = $"Unknown opcode or directive `{lIdent.Value}`";
                            var location = lIdent.StringTokens[0].ToString(true, true, false);
                            throw new ParserException($"{message}{Environment.NewLine}{location}", lIdent);
                        }
                    }
                    default:
                    {
                        throw new AssemblerException();
                    }
                }
            }

            return partialResult;
        }

        private static void ParseOpcode(LexicalTokenInfo currentLToken, List<LexicalTokenInfo> lexicalTokens, ParseResult partialResult, ref LToken<string> unanchoredLabel)
        {
            var lOpcode = (LToken<Opcode>)currentLToken;
            var opcodeDef = OpcodeDefinition.Get(lOpcode.Value);
            switch (opcodeDef.ArgumentType)
            {
                case OpcodeArgumentType.NONE:
                {
                    var astOpcode = new ASTOpcode();
                    astOpcode.LOpcode = lOpcode;

                    if (unanchoredLabel != null)
                    {
                        astOpcode.Label = unanchoredLabel;
                        unanchoredLabel = null;
                    }

                    astOpcode.LexTokens = lexicalTokens.GetRange(0, 1);
                    lexicalTokens.RemoveRange(0, 1);

                    partialResult.SyntaxTree.Add(astOpcode);
                    return;
                }
                case OpcodeArgumentType.REG:
                {
                    var nextLToken = ExpectLToken(lexicalTokens, 1, LexicalToken.Register);
                    
                    var lRegisterA = (LToken<RegisterName>)nextLToken;

                    var astOpcode = new ASTOpcode();
                    astOpcode.LOpcode = lOpcode;
                    astOpcode.LRegisterA = lRegisterA;

                    if (unanchoredLabel != null)
                    {
                        astOpcode.Label = unanchoredLabel;
                        unanchoredLabel = null;
                    }

                    astOpcode.LexTokens = lexicalTokens.GetRange(0, 2);
                    lexicalTokens.RemoveRange(0, 2);

                    partialResult.SyntaxTree.Add(astOpcode);
                    return;
                }
                case OpcodeArgumentType.IMMED:
                {
                    var astOpcode = new ASTOpcode();
                    astOpcode.LOpcode = lOpcode;

                    var immediateTokens = GetImmediateValue(lexicalTokens, 1,
                            out astOpcode.ImmedConstant,
                            out astOpcode.ImmedIdent);

                    //if (astOpcode.ImmedIdent != null)
                    //{
                    //    identsRefrenced.Add(astOpcode.ImmedIdent);
                    //}

                    astOpcode.ImmedLTokens = immediateTokens;

                    if (unanchoredLabel != null)
                    {
                        astOpcode.Label = unanchoredLabel;
                        unanchoredLabel = null;
                    }

                    var len = 1 + immediateTokens.Length;
                    astOpcode.LexTokens = lexicalTokens.GetRange(0, len);
                    lexicalTokens.RemoveRange(0, len);

                    partialResult.SyntaxTree.Add(astOpcode);
                    return;
                }
                case OpcodeArgumentType.REG_REG:
                {
                    var nextLToken1 = ExpectLToken(lexicalTokens, 1, LexicalToken.Register);
                    var nextLToken2 = ExpectLToken(lexicalTokens, 2, LexicalToken.SymComma);
                    var nextLToken3 = ExpectLToken(lexicalTokens, 3, LexicalToken.Register);

                    var lRegisterA = (LToken<RegisterName>)nextLToken1;
                    var lRegisterB = (LToken<RegisterName>)nextLToken3;

                    var astOpcode = new ASTOpcode();
                    astOpcode.LOpcode = lOpcode;
                    astOpcode.LRegisterA = lRegisterA;
                    astOpcode.LRegisterB = lRegisterB;

                    if (unanchoredLabel != null)
                    {
                        astOpcode.Label = unanchoredLabel;
                        unanchoredLabel = null;
                    }

                    astOpcode.LexTokens = lexicalTokens.GetRange(0, 4);
                    lexicalTokens.RemoveRange(0, 4);

                    partialResult.SyntaxTree.Add(astOpcode);
                    return;
                }
                case OpcodeArgumentType.REG_IMMED:
                {
                    var nextLToken1 = ExpectLToken(lexicalTokens, 1, LexicalToken.Register);
                    var nextLToken2 = ExpectLToken(lexicalTokens, 2, LexicalToken.SymComma);

                    var lRegisterA = (LToken<RegisterName>)nextLToken1;

                    var astOpcode = new ASTOpcode();
                    astOpcode.LOpcode = lOpcode;
                    astOpcode.LRegisterA = lRegisterA;

                    var immediateTokens = GetImmediateValue(lexicalTokens, 3,
                        out astOpcode.ImmedConstant,
                        out astOpcode.ImmedIdent);

                    //if (astOpcode.ImmedIdent != null)
                    //{
                    //    identsRefrenced.Add(astOpcode.ImmedIdent);
                    //}

                    astOpcode.ImmedLTokens = immediateTokens;

                    if (unanchoredLabel != null)
                    {
                        astOpcode.Label = unanchoredLabel;
                        unanchoredLabel = null;
                    }

                    var len = 3 + immediateTokens.Length;
                    astOpcode.LexTokens = lexicalTokens.GetRange(0, len);
                    lexicalTokens.RemoveRange(0, len);

                    partialResult.SyntaxTree.Add(astOpcode);
                    return;
                }
                case OpcodeArgumentType.REG_REG_IMMED:
                {
                    var nextLToken1 = ExpectLToken(lexicalTokens, 1, LexicalToken.Register);
                    var nextLToken2 = ExpectLToken(lexicalTokens, 2, LexicalToken.SymComma);
                    var nextLToken3 = ExpectLToken(lexicalTokens, 3, LexicalToken.Register);
                    var nextLToken4 = ExpectLToken(lexicalTokens, 4, LexicalToken.SymComma);
                    
                    var lRegisterA = (LToken<RegisterName>)nextLToken1;
                    var lRegisterB = (LToken<RegisterName>)nextLToken3;

                    var astOpcode = new ASTOpcode();
                    astOpcode.LOpcode = lOpcode;
                    astOpcode.LRegisterA = lRegisterA;
                    astOpcode.LRegisterB = lRegisterB;

                    var immediateTokens = GetImmediateValue(lexicalTokens, 5,
                        out astOpcode.ImmedConstant,
                        out astOpcode.ImmedIdent);

                    //if (astOpcode.ImmedIdent != null)
                    //{
                    //    identsRefrenced.Add(astOpcode.ImmedIdent);
                    //}

                    astOpcode.ImmedLTokens = immediateTokens;

                    if (unanchoredLabel != null)
                    {
                        astOpcode.Label = unanchoredLabel;
                        unanchoredLabel = null;
                    }

                    var len = 5 + immediateTokens.Length;
                    astOpcode.LexTokens = lexicalTokens.GetRange(0, len);
                    lexicalTokens.RemoveRange(0, len);

                    partialResult.SyntaxTree.Add(astOpcode);
                    return;
                }
                default: throw new UnknownStateException();
            }
            throw new UnknownStateException();
        }

        private static void ParseDirective(LexicalTokenInfo currentLToken, List<LexicalTokenInfo> lexicalTokens, ParseResult partialResult, ref LToken<string> unanchoredLabel)
        {
            var lDirective = (LToken<Directive>)currentLToken;
            switch (lDirective.Value)
            {
                case Directive.NAME:
                {
                    if (partialResult.NameDirectiveToken == null)
                    {
                        var nextLToken1 = lexicalTokens[1];
                        if (nextLToken1.Token == LexicalToken.String)
                        {
                            var docName = ((LToken<string>)nextLToken1);
                            partialResult.NameDirectiveToken = docName;
                            lexicalTokens.RemoveRange(0, 2);
                            break;
                        }
                        else
                        {
                            throw new AssemblerException();
                        }
                    }
                    else
                    {
                        throw new AssemblerException();
                    }
                }
                case Directive.PUT:
                {
                    var nextLToken1 = lexicalTokens[1];
                    switch (nextLToken1.Token)
                    {
                        case LexicalToken.String:
                        {
                            var putStringToken = ((LToken<string>)nextLToken1);

                            var astBinary = new ASTBinary();

                            // Use ascii encoding for now, but we should actually use 'no encoding'
                            // http://stackoverflow.com/questions/472906
                            var unescape = System.Text.RegularExpressions.Regex.Unescape(putStringToken.Value);
                            var lst = Assembler.TxtEncoding.GetBytes(unescape).ToList();
                            //lst.Add(0);
                            astBinary.Bytes = lst.ToArray();

                            if (unanchoredLabel != null)
                            {
                                astBinary.Label = unanchoredLabel;
                                unanchoredLabel = null;
                            }

                            astBinary.LexTokens = lexicalTokens.GetRange(0, 2);
                            lexicalTokens.RemoveRange(0, 2);

                            partialResult.SyntaxTree.Add(astBinary);
                            break;
                        }
                        case LexicalToken.Number:
                        {
                            var putNumberToken = ((LToken<uint>)nextLToken1);

                            var astBinary = new ASTBinary();
                            astBinary.Bytes = BitConverter.GetBytes((ushort)putNumberToken.Value).Reverse().ToArray();

                            if (unanchoredLabel != null)
                            {
                                astBinary.Label = unanchoredLabel;
                                unanchoredLabel = null;
                            }

                            astBinary.LexTokens = lexicalTokens.GetRange(0, 2);
                            lexicalTokens.RemoveRange(0, 2);

                            partialResult.SyntaxTree.Add(astBinary);
                            break;
                        }
                        default:
                        {
                            throw new AssemblerException();
                        }
                    }
                    break;
                }
                default:
                {
                    throw new AssemblerException();
                }
            }
        }

        // Immediates must resolve to either exactly one number (constant) value or exactly one identifier
        static LexicalTokenInfo[] GetImmediateValue(List<LexicalTokenInfo> lexicalTokens, int tokensStartPos, out uint? immedConstant, out LToken<string> immedIdent)
        {
            var potentialTokens = new List<LexicalTokenInfo>();
            var currentPos = tokensStartPos;
            while (lexicalTokens[currentPos].Token != LexicalToken.EndOfLine)
            {
                potentialTokens.Add(lexicalTokens[currentPos]);
                currentPos++;
            }

            if (potentialTokens.Count == 1)
            {
                var ltoken = potentialTokens.First();
                switch (ltoken.Token)
                {
                    case LexicalToken.Number:
                    {
                        immedIdent = null;
                        immedConstant = ((LToken<uint>)ltoken).Value;
                        return new[] { ltoken };
                    }
                    case LexicalToken.Identifier:
                    {
                        immedIdent = (LToken<string>)potentialTokens[0];
                        immedConstant = null;
                        return new[] { ltoken };
                    }
                    default:
                    {
                        throw new AssemblerException();
                    }
                }
            }
            else
            {
                throw new AssemblerException();
            }
        }

        static LexicalTokenInfo ExpectLToken(List<LexicalTokenInfo> lexicalTokens, int index, LexicalToken expectType, bool shouldThrow = true)
        {
            if (index < lexicalTokens.Count)
            {
                var token = lexicalTokens[index];
                if (token.Token == expectType)
                {
                    return token;
                }
                else
                {
                    var message = $"Expected {expectType} but got {token.Token}";
                    var location = token.StringTokens[0].ToString(true, true, false);
                    if (shouldThrow)
                    {
                        throw new ParserException($"{message}{Environment.NewLine}{location}", token);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                if (shouldThrow)
                {
                    throw new ParserException($"Expected {expectType} but got (EOS)");
                }
                else
                {
                    return null;
                }
            }
        }
    }
}