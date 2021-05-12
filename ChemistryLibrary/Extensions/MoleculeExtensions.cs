using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;
using GenomeTools.ChemistryLibrary.Builders;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Extensions
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
                    moleculeReference.FirstAtomId = lastAtomId;
            }
            return new MoleculeReference(moleculeReference.Molecule, moleculeReference.FirstAtomId, lastAtomId);
        }
        public static MoleculeReference Add(this MoleculeReference moleculeReference, ElementName element, BondMultiplicity bondMultiplicity)
        {
            var lastAtomId = moleculeReference.LastAtomId;
            var atom = Atom.FromStableIsotope(element);
            lastAtomId = moleculeReference.Molecule.AddAtom(atom, lastAtomId);
            if (!moleculeReference.IsInitialized)
                moleculeReference.FirstAtomId = lastAtomId;
            return new MoleculeReference(moleculeReference.Molecule, moleculeReference.FirstAtomId, lastAtomId);
        }
        public static MoleculeReference Add(this MoleculeReference moleculeReference, MoleculeReference otherMoleculeReference)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");

            return moleculeReference.Molecule.AddMolecule(otherMoleculeReference,
                moleculeReference.FirstAtomId,
                moleculeReference.LastAtomId);
        }
        public static MoleculeReference Add(this MoleculeReference moleculeReference, MoleculeReference otherMoleculeReference, 
            out MoleculeReference updatedOtherMoleculeReference)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");

            return moleculeReference.Molecule.AddMolecule(otherMoleculeReference, 
                moleculeReference.FirstAtomId,
                moleculeReference.LastAtomId,
                out updatedOtherMoleculeReference);
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
            moleculeReference.Molecule.AddMolecule(sideChainReference, 
                moleculeReference.FirstAtomId,
                moleculeReference.LastAtomId);
            return moleculeReference;
        }

        public static MoleculeReference AddSideChain(this MoleculeReference moleculeReference, MoleculeReference sideChainReference, 
            BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            moleculeReference.Molecule.AddMolecule(sideChainReference, 
                moleculeReference.FirstAtomId,
                moleculeReference.LastAtomId,
                bondMultiplicity);
            return moleculeReference;
        }

        public static MoleculeReference ConnectTo(this MoleculeReference moleculeReference, MoleculeReference otherReference, 
            BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            if (!otherReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Side chain molecule reference is not initialized");
            var lastAtomInOtherReference = otherReference.Molecule.GetAtom(otherReference.LastAtomId);
            var matchingConnectionAtom = moleculeReference.Molecule.Atoms
                .SingleOrDefault(a => a.Equals(lastAtomInOtherReference));
            if (matchingConnectionAtom == null)
            {
                moleculeReference.Molecule.AddMolecule(otherReference, 
                    moleculeReference.FirstAtomId,
                    moleculeReference.LastAtomId, 
                    bondMultiplicity);
            }
            else
            {
                var connectionAtomVertexId = moleculeReference.Molecule.MoleculeStructure.Vertices
                    .Single(v => v.Object.Equals(matchingConnectionAtom));
                moleculeReference.Molecule.ConnectAtoms(moleculeReference.LastAtomId, connectionAtomVertexId.Id, bondMultiplicity);
            }
            return moleculeReference;
        }

        public static List<MoleculeReference> AddBenzolRing(this MoleculeReference moleculeReference, params int[] referencePositions)
        {
            var sideChainReference = moleculeReference;
            var references = new List<MoleculeReference>();
            MoleculeReference firstCarbonReference = null;
            for (var carbonIdx = 0; carbonIdx < 6; carbonIdx++)
            {
                var bondMultiplicity = carbonIdx.IsEven() ? BondMultiplicity.Double : BondMultiplicity.Single;
                sideChainReference = sideChainReference.Add(ElementName.Carbon, bondMultiplicity);
                if (carbonIdx == 0)
                    firstCarbonReference = sideChainReference;
                if (referencePositions.Contains(carbonIdx))
                    references.Add(sideChainReference);
                else
                    sideChainReference.AddToCurrentAtom(ElementName.Hydrogen);
            }
            firstCarbonReference.ConnectTo(sideChainReference);
            return references;
        }

        public static void MarkBackbone(this Molecule molecule, MoleculeReference moleculeReference)
        {
            var pathsFromFirstAtom = GraphAlgorithms.ShortestPaths(molecule.MoleculeStructure, moleculeReference.FirstAtomId);
            var pathToLastAtom = pathsFromFirstAtom.PathTo(molecule.MoleculeStructure.GetVertexFromId(moleculeReference.LastAtomId));
            var pathVertices = pathToLastAtom.Path
                .SelectMany(edge => new[] { edge.Vertex1Id, edge.Vertex2Id })
                .Distinct()
                .Select(vId => molecule.MoleculeStructure.GetVertexFromId(vId))
                .Select(v => v.Object);
            pathVertices.ForEach(atom => atom.IsBackbone = true);
        }
    }
}