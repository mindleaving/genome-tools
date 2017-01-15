using System;
using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary
{
    public static class MoleculeExtensions
    {
        public static MoleculeReference Add(this MoleculeReference moleculeReference, params ElementName[] elements)
        {
            var lastAtomId = moleculeReference.LastAtomId;
            foreach (var element in elements)
            {
                var atom = Atom.FromStableIsotope(element);
                lastAtomId = moleculeReference.Molecule.AddAtom(atom, lastAtomId);
                if (!moleculeReference.IsInitialized)
                    moleculeReference.LastAtomId = lastAtomId;
            }
            return new MoleculeReference(moleculeReference.Molecule, lastAtomId);
        }
        public static MoleculeReference Add(this MoleculeReference moleculeReference, ElementName element, BondMultiplicity bondMultiplicity)
        {
            var lastAtomId = moleculeReference.LastAtomId;
            var atom = Atom.FromStableIsotope(element);
            lastAtomId = moleculeReference.Molecule.AddAtom(atom, lastAtomId);
            if (!moleculeReference.IsInitialized)
                moleculeReference.LastAtomId = lastAtomId;
            return new MoleculeReference(moleculeReference.Molecule, lastAtomId);
        }
        public static MoleculeReference Add(this MoleculeReference moleculeReference, MoleculeReference otherMoleculeReference)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            moleculeReference.Molecule.AddMolecule(otherMoleculeReference, moleculeReference.LastAtomId);
            return otherMoleculeReference;
        }

        public static MoleculeReference AddToCurrentAtom(this MoleculeReference moleculeReference, params ElementName[] elements)
        {
            if(!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            foreach (var element in elements)
            {
                var atom = Atom.FromStableIsotope(element);
                moleculeReference.Molecule.AddAtom(atom, moleculeReference.LastAtomId);
            }
            return moleculeReference;
        }
        public static MoleculeReference AddToCurrentAtom(this MoleculeReference moleculeReference, ElementName element, BondMultiplicity bondMultiplicity)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            var atom = Atom.FromStableIsotope(element);
            moleculeReference.Molecule.AddAtom(atom, moleculeReference.LastAtomId, bondMultiplicity);
            return moleculeReference;
        }

        public static MoleculeReference AddSideChain(this MoleculeReference moleculeReference, params ElementName[] sideChainElements)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            var sideChainBuilder = new MoleculeBuilder();
            var sideChainReference = sideChainBuilder.Start.Add(sideChainElements);
            moleculeReference.Molecule.AddMolecule(sideChainReference, moleculeReference.LastAtomId, BondMultiplicity.Single);
            return moleculeReference;
        }

        public static MoleculeReference AddSideChain(this MoleculeReference moleculeReference, MoleculeReference sideChainReference, 
            BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            moleculeReference.Molecule.AddMolecule(sideChainReference, moleculeReference.LastAtomId, BondMultiplicity.Single);
            return moleculeReference;
        }

        public static MoleculeReference ConnectTo(this MoleculeReference moleculeReference, MoleculeReference otherReference, 
            BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            if (otherReference.Molecule != moleculeReference.Molecule)
            {
                moleculeReference.Molecule.AddMolecule(otherReference, moleculeReference.LastAtomId, bondMultiplicity);
            }
            else
            {
                moleculeReference.Molecule.ConnectAtoms(moleculeReference.LastAtomId, otherReference.LastAtomId, bondMultiplicity);
            }
            return moleculeReference;
        }

        public static List<MoleculeReference> AddBenzolRing(this MoleculeReference moleculeReference, params int[] referencePositions)
        {
            var sideChainBuilder = new MoleculeBuilder();
            var sideChainReference = sideChainBuilder.Start;
            var references = new List<MoleculeReference>();
            for (int carbonIdx = 0; carbonIdx < 6; carbonIdx++)
            {
                var bondMultiplicity = carbonIdx.IsEven() ? BondMultiplicity.Double : BondMultiplicity.Single;
                sideChainReference = sideChainReference.Add(ElementName.Carbon, bondMultiplicity);
                if(referencePositions.Contains(carbonIdx))
                    references.Add(sideChainReference);
            }
            sideChainBuilder.Start.ConnectTo(sideChainReference);
            moleculeReference.AddSideChain(sideChainBuilder.Start);
            return references;
        }
    }
}