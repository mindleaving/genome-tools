using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram
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


        public BitArray Encode(byte[] item)
        {
            throw new System.NotImplementedException();
        }

        public byte[] Decode(BitArray bits)
        {
            throw new System.NotImplementedException();
        }
    }
}