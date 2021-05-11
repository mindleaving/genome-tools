using System.Collections.Generic;
using ChemistryLibrary.Objects;

namespace ChemistryLibrary.IO.Fastq
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