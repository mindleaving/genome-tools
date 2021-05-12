using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.DataLookups;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Measurements
{
    public static class AtomPropertyCalculator
    {
        public static UnitValue CalculateMass(int protons, int neutrons)
        {
            return protons*PhysicalConstants.ProtonMass + neutrons*PhysicalConstants.NeutronMass;
        }

        public static UnitValue CalculateOrbitalEnergy(AtomWithOrbitals atom, Orbital orbital)
        {
            return -(1.0/(OrbitalComparer.CalculateOrbitalOrder(orbital) + 1)).To(Unit.ElectronVolts);
        }
    }
}