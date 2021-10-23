using System.Collections;
using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class HuffmanIntCramEncoding : ICramEncoding<int>
    {
        public HuffmanIntCramEncoding(List<int> symbols, List<int> codeWords)
        {
            Symbols = symbols;
            CodeWords = codeWords;
        }

        public Codec CodecId => Codec.Huffman;
        public List<int> Symbols { get; }
        public List<int> CodeWords { get; }


        public BitArray Encode(int item)
        {
            throw new System.NotImplementedException();
        }

        public int Decode(BitArray bits)
        {
            throw new System.NotImplementedException();
        }
    }

    public class HuffmanByteCramEncoding : ICramEncoding<byte>
    {
        public HuffmanByteCramEncoding(List<byte> symbols, List<int> codeWords)
        {
            Symbols = symbols;
            CodeWords = codeWords;
        }

        public Codec CodecId => Codec.Huffman;
        public List<byte> Symbols { get; }
        public List<int> CodeWords { get; }


        public BitArray Encode(byte item)
        {
            throw new System.NotImplementedException();
        }

        public byte Decode(BitArray bits)
        {
            throw new System.NotImplementedException();
        }
    }
}