using System;
using System.Collections.Generic;

namespace AssemblerTwo.Lib
{
    [System.Diagnostics.DebuggerDisplay("{ToString(false),nq}")]
    public class ASTOpcode : ASTNode
    {
        public LToken<Opcode> LOpcode;
        public Opcode Name => LOpcode.Value;

        public LToken<RegisterName> LRegisterA;
        public RegisterName RegisterA => LRegisterA.Value;

        public LToken<RegisterName> LRegisterB;
        public RegisterName RegisterB => LRegisterB.Value;

        public LexicalTokenInfo[] ImmedLTokens;
        public uint? ImmedConstant;
        public LToken<string> ImmedIdent;
        
        public ASTOpcode()
        {
            Type = ASTNodeType.Opcode;
        }

        public OpcodeDefinition GetDef()
        {
            return OpcodeDefinition.Get(LOpcode.Value);
        }

        public override string ToString(bool padding)
        {
            var resultString = string.Empty;

            if (Label != null)
            {
                if (padding)
                {
                    var extraSpace = Math.Max(0, kLabelPad - Label.Value.Length);
                    resultString += $"{Label.Value}: {new string(' ', extraSpace)}";
                }
                else
                {
                    resultString += $"{Label.Value}: ";
                }
            }
            else if (padding)
            {
                resultString += new string(' ', kLabelPad + 2);
            }

            if (padding)
            {
                resultString += $"{Name,-kNamePad}";
            }
            else
            {
                resultString += Name;
            }

            var opcodeDef = GetDef();
            switch (opcodeDef.ArgumentType)
            {
                case OpcodeArgumentType.NONE:
                {
                    break;
                }
                case OpcodeArgumentType.REG:
                {
                    resultString += $" {RegisterA}";
                    break;
                }
                case OpcodeArgumentType.IMMED:
                {
                    if (ImmedConstant.HasValue)
                    {
                        resultString += $" {ImmedConstant}";
                    }
                    else
                    {
                        resultString += $" {ImmedIdent.Value}";
                    }
                    break;
                }
                case OpcodeArgumentType.REG_REG:
                {
                    resultString += $" {RegisterA},{RegisterB}";
                    break;
                }
                case OpcodeArgumentType.REG_IMMED:
                {
                    resultString += $" {RegisterA},";
                    if (ImmedConstant.HasValue)
                    {
                        resultString += $"{ImmedConstant}";
                    }
                    else
                    {
                        resultString += $"{ImmedIdent.Value}";
                    }
                    break;
                }
                case OpcodeArgumentType.REG_REG_IMMED:
                {
                    resultString += $" {RegisterA},{RegisterB},";
                    if (ImmedConstant.HasValue)
                    {
                        resultString += $"{ImmedConstant}";
                    }
                    else
                    {
                        resultString += $"{ImmedIdent.Value}";
                    }
                    break;
                }
            }

            return resultString;
        }
    }
}
