using System;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.Objects
{
    public class MoleculeReference : IDisposable
    {
        private uint firstAtomId = uint.MaxValue;
        private readonly List<uint> vertexIds;

        public MoleculeReference(Molecule molecule)
        {
            Molecule = molecule;
        }

        public MoleculeReference(Molecule molecule, IEnumerable<uint> verrtexIds)
        {
            Molecule = molecule;
            vertexIds = new List<uint>(verrtexIds);
        }

        public MoleculeReference(Molecule molecule, uint firstAtomId, uint lastAtomId)
            : this(molecule)
        {
            FirstAtomId = firstAtomId;
            LastAtomId = lastAtomId;
        }

        public MoleculeReference(Molecule molecule, IEnumerable<uint> vertexIds,  uint firstAtomId, uint lastAtomId)
            : this(molecule, vertexIds)
        {
            FirstAtomId = firstAtomId;
            LastAtomId = lastAtomId;
        }

        public Molecule Molecule { get; }

        public IEnumerable<uint> VertexIds
        {
            get
            {
                if(vertexIds != null)
                    return vertexIds;
                return Molecule.MoleculeStructure.Vertices.Select(v => v.Id);
            }
        }

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

        public void Dispose()
        {
            Molecule.Dispose();
        }
    }
}