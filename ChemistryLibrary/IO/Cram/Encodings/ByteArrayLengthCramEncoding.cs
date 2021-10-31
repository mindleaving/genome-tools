using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class ByteArrayLengthCramEncoding : ICramEncoding<byte[]>
    {
        public ByteArrayLengthCramEncoding(ICramEncoding<int> arrayLength, ICramEncoding<byte> bytes)
        {
            ArrayLength = arrayLength;
            Bytes = bytes;
        }

        public Codec CodecId => Codec.ByteArrayLength;
        public ICramEncoding<int> ArrayLength { get; }
        public ICramEncoding<byte> Bytes { get; }


        public void Encode(byte[] item, BitStream stream)
        {
            throw new System.NotImplementedException();
        }

        public byte[] Decode(BitStream bits)
        {
            throw new System.NotImplementedException();
        }
    }
}