namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class BetaCramEncoding : CramEncoding
    {
        public BetaCramEncoding(int offset, int numberOfBits)
        {
            Offset = offset;
            NumberOfBits = numberOfBits;
        }

        public override Codec CodecId => Codec.Beta;
        public int Offset { get; }
        public int NumberOfBits { get; }
    }
}