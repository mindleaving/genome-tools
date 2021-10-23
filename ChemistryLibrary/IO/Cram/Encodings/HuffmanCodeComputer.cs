using System;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class HuffmanCodeComputer
    {
        public HuffmanIntCramEncoding Compute(IReadOnlyCollection<int> values)
        {
            var alphabet = values.GroupBy(x => x).OrderByDescending(g => g.Count()).Select(g => g.Key).ToList();
            throw new NotImplementedException();
        }

        public HuffmanByteCramEncoding Compute(IReadOnlyCollection<byte> values)
        {
            var alphabet = values.GroupBy(x => x).OrderByDescending(g => g.Count()).Select(g => g.Key).ToList();
            throw new NotImplementedException();
        }
    }
}
