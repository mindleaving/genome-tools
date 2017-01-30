namespace ChemistryLibrary
{
    public static class MoleculeLibrary
    {
        public static Molecule H2O
        {
            get
            {
                var moleculeBuilder = new MoleculeBuilder();
                moleculeBuilder.Start
                    .Add(ElementName.Oxygen)
                    .AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);
                return moleculeBuilder.Molecule;
            }
        }
    }
}
