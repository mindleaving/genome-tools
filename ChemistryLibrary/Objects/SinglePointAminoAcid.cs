using Commons.Physics;

namespace GenomeTools.ChemistryLibrary.Objects
{
    public class SinglePointAminoAcid
    {
        public AminoAcidName Name { get; set; }
        public int SequenceNumber { get; set; }
        public UnitPoint3D Position { get; set; }
    }
}
