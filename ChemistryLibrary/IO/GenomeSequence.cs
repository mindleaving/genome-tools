namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeSequence : IGenomeSequence
    {
        private readonly string sequence;

        public string SequenceName { get; }
        public int StartIndex { get; }

        public GenomeSequence(string sequence, string sequenceName, int startIndex)
        {
            this.sequence = sequence;
            SequenceName = sequenceName;
            StartIndex = startIndex;
        }

        public string GetSequence()
        {
            return sequence;
        }

        public char GetBaseAtPosition(int position)
        {
            return sequence[position];
        }
    }
}