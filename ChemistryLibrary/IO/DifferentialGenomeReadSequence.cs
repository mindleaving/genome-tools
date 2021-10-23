using System;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class DifferentialGenomeReadSequence : IGenomeReadSequence
    {
        private GenomeSequenceAccessor ReferenceSequence { get; }
        private int ReferenceStartIndex { get; }
        private int ReferenceEndIndex { get; }

        public int Length => throw new NotImplementedException();

        public char GetBase(int readIndex)
        {
            throw new System.NotImplementedException();
        }

        public string GetBases(int readStartIndex, int readEndIndex)
        {
            throw new System.NotImplementedException();
        }

        public string GetSequence()
        {
            throw new System.NotImplementedException();
        }
    }
}