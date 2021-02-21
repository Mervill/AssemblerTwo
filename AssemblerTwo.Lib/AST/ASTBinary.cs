using System;
using System.Collections.Generic;

namespace AssemblerTwo.Lib
{
    [System.Diagnostics.DebuggerDisplay("{ToString(false),nq}")]
    public class ASTBinary : ASTNode
    {
        public byte[] Bytes;

        public ASTBinary()
        {
            Type = ASTNodeType.Binary;
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

            resultString += $"bytes[{Bytes.Length}]";

            return resultString;
        }
    }
}
