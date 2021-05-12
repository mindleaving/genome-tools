using Commons.Physics;

namespace GenomeTools.ChemistryLibrary.Simulation
{
    public class ApproximateAminoAcidForces
    {
        public UnitVector3D NitrogenForce { get; set; } = new UnitVector3D(Unit.Newton, 0, 0, 0);
        public UnitVector3D CarbonAlphaForce { get; set; } = new UnitVector3D(Unit.Newton, 0, 0, 0);
        public UnitVector3D CarbonForce { get; set; } = new UnitVector3D(Unit.Newton, 0, 0, 0);

        public static ApproximateAminoAcidForces operator +(ApproximateAminoAcidForces forces,
            ApproximateAminoAcidForces otherForces)
        {
            return new ApproximateAminoAcidForces
            {
                NitrogenForce = forces.NitrogenForce + otherForces.NitrogenForce,
                CarbonAlphaForce = forces.CarbonAlphaForce + otherForces.CarbonAlphaForce,
                CarbonForce = forces.CarbonForce + otherForces.CarbonForce
            };
        }
    }
}