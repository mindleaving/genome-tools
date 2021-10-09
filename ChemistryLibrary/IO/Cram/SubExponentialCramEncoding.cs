namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class SubExponentialCramEncoding : CramEncoding
    {
        public SubExponentialCramEncoding(int offset, int k)
        {
            Offset = offset;
            K = k;
        }

        public override Codec CodecId => Codec.SubExponential;
        public int Offset { get; }
        public int K { get; }
    }
}