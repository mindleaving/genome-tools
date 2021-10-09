namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class GammaCramEncoding : CramEncoding
    {
        public GammaCramEncoding(int offset)
        {
            Offset = offset;
        }

        public override Codec CodecId => Codec.Gamma;
        public int Offset { get; }
    }
}