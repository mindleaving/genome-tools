using System;

namespace ChemistryLibrary
{
    public class MoleculeReference
    {
        private uint firstAtomId = uint.MaxValue;

        public MoleculeReference(Molecule molecule)
        {
            Molecule = molecule;
        }

        public MoleculeReference(Molecule molecule, uint firstAtomId, uint lastAtomId)
            : this(molecule)
        {
            FirstAtomId = firstAtomId;
            LastAtomId = lastAtomId;
        }

        public Molecule Molecule { get; }
        public bool IsInitialized { get; private set; }
        public uint FirstAtomId
        {
            get { return firstAtomId; }
            set
            {
                if (IsInitialized)
                    throw new InvalidOperationException(nameof(LastAtomId) + " can only be set once");
                firstAtomId = value;
                LastAtomId = value;
                IsInitialized = true;
            }
        }
        public uint LastAtomId { get; private set; }
    }
}