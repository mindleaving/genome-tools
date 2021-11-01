using System.IO;

namespace GenomeTools.ChemistryLibrary.IO.Fasta
{
    public class FastaMd5HashFileWriter
    {
        public void Append(string filePath, FastaSequenceMd5Hash item)
        {
            File.AppendAllLines(filePath, new []{ $"{item.SequenceName}\t{item.Md5Hash}"});
        }
    }
}