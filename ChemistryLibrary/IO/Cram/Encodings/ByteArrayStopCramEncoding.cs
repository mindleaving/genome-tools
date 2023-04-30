namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class ByteArrayStopCramEncoding : ICramEncoding<byte[]>
    {
        public ByteArrayStopCramEncoding(byte stopValue, int externalBlockContentId)
        {
            StopValue = stopValue;
            ExternalBlockContentId = externalBlockContentId;
        }

        public Codec CodecId => Codec.ByteArrayStop;
        public byte StopValue { get; }
        public int ExternalBlockContentId { get; }


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