using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram
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