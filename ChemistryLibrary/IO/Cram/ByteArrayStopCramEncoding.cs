namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class ByteArrayStopCramEncoding : CramEncoding
    {
        public ByteArrayStopCramEncoding(byte stopValue, int externalBlockContentId)
        {
            StopValue = stopValue;
            ExternalBlockContentId = externalBlockContentId;
        }

        public override Codec CodecId => Codec.ByteArrayStop;
        public byte StopValue { get; }
        public int ExternalBlockContentId { get; }
    }
}