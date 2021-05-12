namespace GenomeTools.ChemistryLibrary.Objects
{
    public class AminoAcidSequenceItem
    {
        public AminoAcidName AminoAcidName { get; set; }
        public int ResidueNumber { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj is AminoAcidSequenceItem))
                return false;
            var otherItem = (AminoAcidSequenceItem) obj;
            return Equals(otherItem);
        }

        private bool Equals(AminoAcidSequenceItem other)
        {
            return AminoAcidName == other.AminoAcidName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) AminoAcidName*397) ^ ResidueNumber;
            }
        }
    }
}