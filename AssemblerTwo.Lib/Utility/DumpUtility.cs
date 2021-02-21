using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblerTwo.Lib
{
    public static class DumpUtility
    {
        public static void DumpBytes(StringBuilder sb, byte[] byteSequence, uint columns = 8)
        {
            if (columns == 0)
                throw new ArgumentException($"Argument {nameof(columns)} cannot be zero!", nameof(columns));

            for (int x = 1; x < byteSequence.Length + 1; x++)
            {
                sb.Append(byteSequence[x - 1].ToString("X2"));
                if ((x % (2 * 8) == 0) && (x != byteSequence.Length))
                {
                    sb.AppendLine();
                }
                else if ((x % 2 == 0) && (x != byteSequence.Length))
                {
                    sb.Append(' ');
                }
            }
        }

        public static string DumpBytes(byte[] byteSequence, uint columns = 8)
        {
            var sb = new StringBuilder();
            DumpBytes(sb, byteSequence, columns);
            return sb.ToString();
        }

        public static void DumpASTNodes(StringBuilder sb, IEnumerable<ASTNode> astNodes, bool padding = true, bool expandBinaryNodes = false)
        {
            foreach (var node in astNodes)
            {
                var nodeType = node.Type;
                switch (nodeType)
                {
                    case ASTNodeType.Opcode:
                    {
                        sb.AppendLine(node.ToString(padding));
                        continue;
                    }
                    case ASTNodeType.Binary:
                    {
                        if (expandBinaryNodes)
                        {
                            sb.AppendLine(node.ToString(padding));
                            if (expandBinaryNodes)
                            {
                                var astBytes = ((ASTBinary)node).Bytes;

                                var padLen = (padding) ? ASTNode.kLabelPad + 2 : 0;
                                var byteRows = DumpBytes(astBytes).Split(Environment.NewLine);
                                for (int x = 0; x < byteRows.Length; x++)
                                {
                                    sb.Append(new string(' ', padLen));
                                    sb.AppendLine(byteRows[x]);
                                }
                            }
                            continue;
                        }
                        else
                        {
                            sb.AppendLine(node.ToString(padding));
                            continue;
                        }
                    }
                }
            }
        }

        public static string DumpASTNodes(IEnumerable<ASTNode> astNodes, bool padding = true, bool expandBinaryNodes = false)
        {
            var sb = new StringBuilder();
            DumpASTNodes(sb, astNodes, padding, expandBinaryNodes);
            return sb.ToString();
        }

        public static string DumpStringTokens(IEnumerable<StringTokenInfo> stringTokens, bool showIndices = true, bool hideEoLCharacters = true, bool csvFormat = true)
        {
            var sb = new StringBuilder();
            foreach (var tk in stringTokens)
            {
                sb.Append(tk.ToString(showIndices, hideEoLCharacters, csvFormat));
                if (tk != stringTokens.Last())
                    sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
