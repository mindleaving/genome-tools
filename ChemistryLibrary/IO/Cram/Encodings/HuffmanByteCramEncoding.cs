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


        public void Encode(byte item, BitStream stream)
        {
            base.Encode(item, stream);
        }

        byte ICramEncoding<byte>.Decode(BitStream bits)
        {
            return (byte)base.Decode(bits);
        }
    }
}