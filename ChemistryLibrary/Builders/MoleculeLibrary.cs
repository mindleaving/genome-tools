using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Builders
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
