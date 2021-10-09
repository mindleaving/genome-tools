using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class HuffmanCramEncoding : CramEncoding
    {
        public HuffmanCramEncoding(List<int> symbols, List<int> weights)
        {
            Symbols = symbols;
            Weights = weights;
        }

        public override Codec CodecId => Codec.Huffman;
        public List<int> Symbols { get; }
        public List<int> Weights { get; }
    }
}