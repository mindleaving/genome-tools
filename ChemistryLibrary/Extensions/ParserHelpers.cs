using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;

namespace GenomeTools.ChemistryLibrary.Extensions
{
    public static class ParserHelpers
    {
        public static List<string> QuoteAwareSplit(
            this string line,
            char separator,
            StringSplitOptions options = StringSplitOptions.None)
        {
            var rawSplit = new Queue<string>(line.Split(separator, options));
            var split = new List<string>();
            var currentSplit = string.Empty;
            while (rawSplit.Any())
            {
                var part = rawSplit.Dequeue();
                if (currentSplit != string.Empty)
                {
                    if (part.EndsWith('"'))
                    {
                        currentSplit += part;
                        split.Add(currentSplit.Substring(1, currentSplit.Length - 2));
                        currentSplit = string.Empty;
                    }
                    else
                        currentSplit += part;
                }
                else if (part.StartsWith('"'))
                {
                    if (part.EndsWith('"'))
                        split.Add(part.Substring(1, part.Length-2));
                    else
                        currentSplit += part;
                }
                else
                {
                    split.Add(part);
                }
            }
            if(currentSplit != string.Empty)
                split.Add(currentSplit);
            return split;
        }

        public static byte[] ParseHexString(string hexString)
        {
            if(hexString.Length.IsOdd())
                throw new FormatException("A hex string must have an even number of characters in the range 0-9A-F (lower case is also acceptable) to be valid. "
                                          + $"Length was {hexString.Length}.");
            var result = new byte[hexString.Length / 2];
            for (int byteIndex = 0; byteIndex < result.Length; byteIndex++)
            {
                result[byteIndex] = Convert.ToByte(hexString.Substring(byteIndex * 2, 2), 16);
            }
            return result;
        }
    }
}
