namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class ByteArrayLengthCramEncoding : CramEncoding
    {
        public ByteArrayLengthCramEncoding(CramEncoding arrayLength, CramEncoding bytes)
        {
            ArrayLength = arrayLength;
            Bytes = bytes;
        }

        public override Codec CodecId => Codec.ByteArrayLength;
        public CramEncoding ArrayLength { get; }
        public CramEncoding Bytes { get; }
    }
}