namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class GolombRiceCramEncoding : CramEncoding
    {
        public GolombRiceCramEncoding(int offset, int log2OfM)
        {
            Offset = offset;
            Log2OfM = log2OfM;
        }

        public override Codec CodecId => Codec.GolombRice;
        public int Offset { get; }
        public int Log2OfM { get; }
    }
}