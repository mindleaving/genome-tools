namespace GenomeTools.ChemistryLibrary.IO
{
    public class VerbatimGenomeReadSequence : IGenomeReadSequence
    {
        public string Sequence { get; }

        public VerbatimGenomeReadSequence(string sequence)
        {
            Sequence = sequence;
        }

        public int Length => Sequence.Length;

        public char GetBase(int readIndex)
        {
            return Sequence[readIndex];
        }

        public string GetBases(int readStartIndex, int readEndIndex)
        {
            return Sequence.Substring(readStartIndex, readEndIndex - readStartIndex + 1);
        }

        public string GetSequence()
        {
            throw new System.NotImplementedException();
        }
    }
}