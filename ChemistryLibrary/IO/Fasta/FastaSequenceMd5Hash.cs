namespace GenomeTools.ChemistryLibrary.IO.Fasta
{
    public class FastaSequenceMd5Hash
    {
        public FastaSequenceMd5Hash(string sequenceName, string md5Hash)
        {
            SequenceName = sequenceName;
            Md5Hash = md5Hash;
        }

        public string SequenceName { get; }
        public string Md5Hash { get; }
    }
}