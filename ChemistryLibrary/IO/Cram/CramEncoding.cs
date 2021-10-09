namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public abstract class CramEncoding
    {
        public enum Codec
        {
            Null = 0,
            External = 1,
            Golomb = 2,
            Huffman = 3,
            ByteArrayLength = 4,
            ByteArrayStop = 5,
            Beta = 6,
            SubExponential = 7,
            GolombRice = 8,
            Gamma = 9
        }
        public abstract Codec CodecId { get; }
    }
}