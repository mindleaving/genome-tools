using System.Collections.Generic;

namespace ChemistryLibrary.Objects
{
    public class AminoAcidSequence : List<AminoAcidSequenceItem>
    {
        public AminoAcidSequence(IEnumerable<AminoAcidSequenceItem> items)
            : base(items)
        {}
    }
}