using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Fastq
{
    public class FastqData
    {
        public FastqData(List<Contig> contigs)
        {
            Contigs = contigs;
        }

        public List<Contig> Contigs { get; }
    }
}