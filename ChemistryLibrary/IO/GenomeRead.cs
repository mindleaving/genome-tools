using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeRead : GenomeSequence
    {
        public GenomeRead(string sequence, int referenceStartIndex, int referenceEndIndex)
            : base(sequence, referenceStartIndex, referenceEndIndex)
        {
        }

        public GenomeRead(IEnumerable<GenomeSequencePart> parts)
            : base(parts)
        {
        }
    }
}