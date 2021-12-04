using GenomeTools.ChemistryLibrary.Genomics;

namespace GenomeTools.ChemistryLibrary.IO
{
    public interface IGenomeSequenceAccessor
    {
        IGenomeSequence GetSequenceByName(string sequenceName, int startIndex = 0, int? endIndex = null);
        IGenomeSequence GetSequenceById(int referenceId, int startIndex = 0, int? endIndex = null);
    }
}