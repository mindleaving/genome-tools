using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram;

public class CramGenomeSequenceAlignmentAccessorFactory
{
    private readonly string alignmentFilePath;
    private readonly string referenceSequenceFilePath;

    public CramGenomeSequenceAlignmentAccessorFactory(string alignmentFilePath, string referenceSequenceFilePath)
    {
        this.alignmentFilePath = alignmentFilePath;
        this.referenceSequenceFilePath = referenceSequenceFilePath;
    }

    public CramGenomeSequenceAlignmentAccessor Create()
    {
        var cramHeaderReader = new CramHeaderReader(CramHeaderReader.Md5CheckFailureMode.WriteToConsole);
        var cramHeader = cramHeaderReader.Read(alignmentFilePath, referenceSequenceFilePath);
        var referenceSequenceMap = ReferenceSequenceMap.FromSamHeaderEntries(cramHeader.SamHeader);
        return new CramGenomeSequenceAlignmentAccessor(alignmentFilePath, referenceSequenceFilePath, referenceSequenceMap);
    }
}