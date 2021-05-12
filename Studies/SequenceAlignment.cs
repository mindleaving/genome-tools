using System.Collections.Generic;

namespace GenomeTools.Studies
{
    public class SequenceAlignment<T>
    {
        public SequenceAlignment(List<AlignedPair<T>> alignedPairs)
        {
            AlignedPairs = alignedPairs;
        }

        public List<AlignedPair<T>> AlignedPairs { get; }
    }
}