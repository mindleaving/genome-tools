using System;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeSequencePart
    {
        private GenomeSequencePart(GenomeSequencePartType type, 
            IGenomeReadSequence sequence,
            int referenceStartIndex, 
            int referenceEndIndex)
        {
            Type = type;
            Sequence = sequence;
            ReferenceStartIndex = referenceStartIndex;
            ReferenceEndIndex = referenceEndIndex;
        }

        public GenomeSequencePartType Type { get; }
        public int ReferenceStartIndex { get; }
        public int ReferenceEndIndex { get; }
        public IGenomeReadSequence Sequence { get; }

        public static GenomeSequencePart MatchedBases(IGenomeReadSequence sequence, int referenceStartIndex, int referenceEndIndex)
        {
            if (referenceEndIndex - referenceStartIndex + 1 != sequence.Length)
                throw new ArgumentException("Length of provided sequence doesn't match the reference index range");
            return new GenomeSequencePart(
                GenomeSequencePartType.MatchedBases,
                sequence,
                referenceStartIndex,
                referenceEndIndex);
        }

        public static GenomeSequencePart Insert(string sequence, int referencePosition)
        {
            return new GenomeSequencePart(
                GenomeSequencePartType.Insert,
                new VerbatimGenomeReadSequence(sequence),
                referencePosition,
                referencePosition);
        }

        public static GenomeSequencePart Deletion(int referenceStartIndex, int referenceEndIndex)
        {
            return new GenomeSequencePart(
                GenomeSequencePartType.Deletion,
                null,
                referenceStartIndex,
                referenceEndIndex);
        }

        public static GenomeSequencePart Reversal(IGenomeReadSequence sequence, int referenceStartIndex, int referenceEndIndex)
        {
            return new GenomeSequencePart(
                GenomeSequencePartType.Reversal,
                sequence,
                referenceStartIndex,
                referenceEndIndex);
        }
    }
}