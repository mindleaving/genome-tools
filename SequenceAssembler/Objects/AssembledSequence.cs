namespace SequenceAssembler.Objects
{
    public class AssembledSequence
    {
        public AssembledSequence(byte[] sequence)
        {
            Sequence = sequence;
        }

        public byte[] Sequence { get; }
    }
}