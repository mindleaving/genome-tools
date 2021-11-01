using GenomeTools.ChemistryLibrary.Genomics;

namespace GenomeTools.ChemistryLibrary.IO
{
    public interface IGenomeSequenceAccessor
    {
        IGenomeSequence GetSequenceByName(string sequenceName, int startIndex, int endIndex);
        IGenomeSequence GetSequenceById(int referenceId, int startIndex, int endIndex);
    }
}