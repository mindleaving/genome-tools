using ChemistryLibrary.DataLookups;
using ChemistryLibrary.Objects;
using Commons.Physics;

namespace ChemistryLibrary.Measurements
{
    public static class BondLengthCalculator
    {
        public static UnitValue Calculate(AtomWithOrbitals atom1, Orbital orbital1, AtomWithOrbitals atom2, Orbital orbital2)
        {
            return 1.0*(atom1.Radius + atom2.Radius);
        }

        public static UnitValue CalculateApproximate(ElementName element1, ElementName element2)
        {
            return PeriodicTable.GetRadius(element1) + PeriodicTable.GetRadius(element2);
        }
    }
}