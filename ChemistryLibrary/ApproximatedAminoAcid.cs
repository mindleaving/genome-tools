using Commons;

namespace ChemistryLibrary
{
    public class ApproximatedAminoAcid
    {
        public ApproximatedAminoAcid(AminoAcidName aminoAcidName)
        {
            Name = aminoAcidName;
        }

        public UnitPoint3D NitrogenPosition { get; set; }
        public UnitPoint3D CarbonAlphaPosition { get; set; }
        public UnitPoint3D CarbonPosition { get; set; }
        public UnitValue PhiAngle { get; set; }
        public UnitValue PsiAngle { get; set; }
        public AminoAcidName Name { get; }
    }
}
