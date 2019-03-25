using System.IO;
using SequenceAssembler.Workflow;

namespace SequenceAssembler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var fastqFilePath = @"G:\Projects\HumanGenome\ecoli_mda_lane1_left_partial.fastq";
            var outputFilePath = @"G:\Projects\HumanGenome\ecoli_mda_lane1_assembled.fastq";

            var sequenceReader = new FastqReader();
            var sequences = sequenceReader.Read(fastqFilePath);

            var sequenceAssembler = new RestricedBoltzmannMachineSequenceAssembler();
            var assembledSequence = sequenceAssembler.Assemble(sequences);

            var sequenceWriter = new NucleotideSequenceWriter();
            sequenceWriter.Write(assembledSequence, outputFilePath);
        }
    }
}
