using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.IO;

namespace GenomeTools.ChemistryLibraryTest.IO
{
    public class DummyReferenceAccessor : IGenomeSequenceAccessor
    {
        private readonly string referenceSequence;

        public DummyReferenceAccessor(string referenceSequence)
        {
            this.referenceSequence = referenceSequence;
        }

        public IGenomeSequence GetSequenceByName(string sequenceName, int startIndex, int endIndex)
        {
            return new GenomeSequence(referenceSequence.Substring(startIndex, endIndex - startIndex + 1), sequenceName, startIndex);
        }

        public IGenomeSequence GetSequenceById(int referenceId, int startIndex, int endIndex)
        {
            return new GenomeSequence(referenceSequence.Substring(startIndex, endIndex - startIndex + 1), "Reference", startIndex);
        }
    }
}