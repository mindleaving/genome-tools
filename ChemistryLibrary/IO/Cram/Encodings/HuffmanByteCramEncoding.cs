using System.Collections;
using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class HuffmanByteCramEncoding : HuffmanIntCramEncoding, ICramEncoding<byte>
    {
        public HuffmanByteCramEncoding(List<HuffmanCodeSymbol> symbols)
            : base(symbols)
        {
        }


        public BitArray Encode(byte item)
        {
            return base.Encode(item);
        }

        byte ICramEncoding<byte>.Decode(BitArray bits)
        {
            return (byte)base.Decode(bits);
        }
    }
}