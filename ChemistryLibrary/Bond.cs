using Commons;

namespace ChemistryLibrary
{
    public enum BondMultiplicity
    {
        Single,
        Double,
        Triple
    }
    public class Bond
    {
        public Bond(Atom atom1, 
            Orbital orbital1, 
            Atom atom2, 
            Orbital orbital2)
        {
            Atom1 = atom1;
            Atom2 = atom2;
            Orbital1 = orbital1;
            Orbital2 = orbital2;
        }

        public Atom Atom1 { get; }
        public Atom Atom2 { get; }

        public Orbital Orbital1 { get; }
        public Orbital Orbital2 { get; }

        public UnitValue BondEnergy { get; set; }
    }
}
