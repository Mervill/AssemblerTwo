using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblerTwo.Lib
{
    public class BytecodeGroup
    {
        public Dictionary<string, int> LabelDefines;
        public Dictionary<string, List<int>> LabelRefs;
        public HashSet<int> RelocationIndicies;

        public byte[] Bytes;

        public string SerializeLabelData(bool padding = true)
        {
            var sb = new StringBuilder();
            
            var padLen = (padding) ? LabelDefines.Keys.Max(x => x.Length) + 1 : 0;

            foreach (var kvp in LabelDefines)
            {
                var str = $"{kvp.Key},";
                sb.AppendLine($"&{str.PadRight(padLen)} {kvp.Value:X4}");
            }

            foreach (var kvp in LabelRefs)
            {
                var str = $"{kvp.Key},";
                var refs = string.Empty;
                if (kvp.Value.Count != 0)
                    refs = kvp.Value.ConvertAll(x => x.ToString("X4"))
                                    .Aggregate((a, b) => $"{a}, {b}");
                sb.AppendLine($"*{str.PadRight(padLen)} {refs}");
            }

            return sb.ToString();
        }
    }
}
