namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class HuffmanCodeSymbol
    {
        public HuffmanCodeSymbol(int symbol, int codeLength)
        {
            Symbol = symbol;
            CodeLength = codeLength;
        }

        public int Symbol { get; }
        public int CodeLength { get; }
    }
}