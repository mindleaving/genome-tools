namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class ByteArrayLengthCramEncoding : ICramEncoding<byte[]>
    {
        public ByteArrayLengthCramEncoding(ICramEncoding<int> arrayLengthEncoding, ICramEncoding<byte> valuesEncoding)
        {
            ArrayLengthEncoding = arrayLengthEncoding;
            ValuesEncoding = valuesEncoding;
        }

        public Codec CodecId => Codec.ByteArrayLength;
        public ICramEncoding<int> ArrayLengthEncoding { get; }
        public ICramEncoding<byte> ValuesEncoding { get; }


        public void Encode(byte[] item, BitStream stream)
        {
            throw new System.NotSupportedException();
        }

        public byte[] Decode(BitStream bits)
        {
            throw new System.NotSupportedException();
        }
    }
}