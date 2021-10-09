namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class NullCramEncoding : CramEncoding
    {
        public override Codec CodecId => Codec.Null;
    }
}