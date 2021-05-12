using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Builders
{
    public class MoleculeBuilder
    {
        private MoleculeReference start;
        public Molecule Molecule { get; } = new Molecule();

        public MoleculeReference Start => start ?? (start = new MoleculeReference(Molecule));
    }
}