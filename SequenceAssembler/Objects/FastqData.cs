using System.Collections.Generic;

namespace SequenceAssembler.Objects
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