namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class CramRecord
    {
        public GenomeRead Read { get; }

        public CramRecord(GenomeRead read)
        {
            Read = read;
        }
    }
}
