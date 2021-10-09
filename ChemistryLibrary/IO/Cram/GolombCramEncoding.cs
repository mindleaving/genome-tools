namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class GolombCramEncoding : CramEncoding
    {
        public GolombCramEncoding(int offset, int m)
        {
            Offset = offset;
            M = m;
        }

        public override Codec CodecId => Codec.Golomb;
        public int Offset { get; }
        public int M { get; }
    }
}