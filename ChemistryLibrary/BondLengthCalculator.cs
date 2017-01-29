using Commons;

namespace ChemistryLibrary
{
    public static class BondLengthCalculator
    {
        public static UnitValue Calculate(Atom atom1, Orbital orbital1, Atom atom2, Orbital orbital2)
        {
            return 0.9*(atom1.Radius + atom2.Radius);
        }
    }
}