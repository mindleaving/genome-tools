namespace GenomeTools.ChemistryLibrary.IO
{
    public interface IGenomeSequence
    {
        string GetSequence();
        char GetBaseAtPosition(int position);
    }
}