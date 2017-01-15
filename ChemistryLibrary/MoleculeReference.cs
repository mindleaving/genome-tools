using System;

namespace ChemistryLibrary
{
    public class MoleculeReference
    {
        private uint lastAtomId = uint.MaxValue;

        public MoleculeReference(Molecule molecule)
        {
            Molecule = molecule;
        }

        public MoleculeReference(Molecule molecule, uint lastAtomId)
            : this(molecule)
        {
            LastAtomId = lastAtomId;
        }

        public Molecule Molecule { get; }
        public bool IsInitialized { get; private set; }
        public uint LastAtomId
        {
            get { return lastAtomId; }
            set
            {
                if(IsInitialized)
                    throw new InvalidOperationException(nameof(LastAtomId) + " can only be set once");
                lastAtomId = value;
                IsInitialized = true;
            }
        }
    }
}