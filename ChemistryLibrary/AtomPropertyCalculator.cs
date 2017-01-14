using Commons;

namespace ChemistryLibrary
{
    public static class AtomPropertyCalculator
    {
        public static UnitValue CalculateMass(int protons, int neutrons)
        {
            return protons*PhysicalConstants.ProtonMass + neutrons*PhysicalConstants.NeutronMass;
        }

        public static UnitValue CalculateOrbitalEnergy(Atom atom, Orbital orbital)
        {
            throw new System.NotImplementedException();
        }
    }
}