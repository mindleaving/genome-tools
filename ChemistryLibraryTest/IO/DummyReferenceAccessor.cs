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

        public IGenomeSequence GetSequenceByName(string sequenceName, int startIndex = 0, int? endIndex = null)
        {
            if (!endIndex.HasValue)
                endIndex = referenceSequence.Length - 1;
            return new GenomeSequence(referenceSequence.Substring(startIndex, endIndex.Value - startIndex + 1), sequenceName, startIndex);
        }

        public IGenomeSequence GetSequenceById(int referenceId, int startIndex = 0, int? endIndex = null)
        {
            if (!endIndex.HasValue)
                endIndex = referenceSequence.Length - 1;
            return new GenomeSequence(referenceSequence.Substring(startIndex, endIndex.Value - startIndex + 1), "Reference", startIndex);
        }
    }
}