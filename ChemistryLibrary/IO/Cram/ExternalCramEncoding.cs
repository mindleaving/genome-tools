namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class ExternalCramEncoding : CramEncoding
    {
        public ExternalCramEncoding(int blockContentId)
        {
            BlockContentId = blockContentId;
        }

        public override Codec CodecId => Codec.External;
        public int BlockContentId { get; }
    }
}