using Commons;

namespace ChemistryLibrary
{
    public class ApproximatedAminoAcid
    {
        public ApproximatedAminoAcid(AminoAcidName aminoAcidName)
        {
            Name = aminoAcidName;
        }

        public Point3D NitrogenPosition { get; set; }
        public Point3D CarbonAlphaPosition { get; set; }
        public Point3D CarbonPosition { get; set; }
        public UnitValue PhiAngle { get; set; }
        public UnitValue PsiAngle { get; set; }
        public AminoAcidName Name { get; }
    }
}
