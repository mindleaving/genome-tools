namespace GenomeTools.ChemistryLibrary.IO
{
    public interface IGenomeSequence
    {
        int Length { get; }
        string GetSequence();
        char GetBaseAtPosition(int position);
    }
}