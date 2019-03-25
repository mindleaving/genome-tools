using System.Collections.Generic;
using SequenceAssembler.Objects;

namespace SequenceAssembler.Workflow
{
    public class RestricedBoltzmannMachineSequenceAssembler
    {
        public AssembledSequence Assemble(FastqData sequences)
        {
            var assembledSequence = new List<byte>();
            foreach (var contig in sequences.Contigs)
            {
                assembledSequence.AddRange(contig.Nucleotides);
            }
            return new AssembledSequence(assembledSequence.ToArray());
        }
    }
}
