namespace GenomeTools.ChemistryLibrary.Genomics
{
    public interface IGenomeSequence
    {
        int Length { get; }
        string GetSequence();
        char GetBaseAtPosition(int position);
        char GetQualityScoreAtPosition(int position);
    }
}