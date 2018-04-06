using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Physics;

namespace ChemistryLibrary.Measurements
{
    public static class BondEnergyCalculator
    {
        public static UnitValue Calculate(AtomWithOrbitals atom1, Orbital orbital1, AtomWithOrbitals atom2, Orbital orbital2)
        {
            return -5.To(Unit.ElectronVolts);
        }
    }
}