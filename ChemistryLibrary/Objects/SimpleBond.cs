namespace ChemistryLibrary.Objects
{
    public class SimpleBond
    {
        public SimpleBond(Atom atom1, Atom atom2)
        {
            Atom1 = atom1;
            Atom2 = atom2;
        }

        public Atom Atom1 { get; }
        public Atom Atom2 { get; }
    }
}