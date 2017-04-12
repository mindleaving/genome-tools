using System;
using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary.Pdb
{
    public static class PdbAminoAcidAtomNamer
    {
        public static void AssignNames(AminoAcidReference aminoAcidReference)
        {
            var molecule = aminoAcidReference.Molecule;
            aminoAcidReference.VertexIds
                .Select(vId => aminoAcidReference.Molecule.MoleculeStructure.Vertices[vId])
                .ForEach(v => v.AlgorithmData = false);

            var nitrogenVertex = molecule.MoleculeStructure.Vertices[aminoAcidReference.FirstAtomId];
            nitrogenVertex.AlgorithmData = true;
            var nitrogen = molecule.GetAtom(aminoAcidReference.FirstAtomId);
            nitrogen.AminoAcidAtomName = "N";
            if(nitrogen.Element != ElementName.Nitrogen)
                throw new Exception("Bug!");

            var carbonEndVertex = molecule.MoleculeStructure.Vertices[aminoAcidReference.LastAtomId];
            carbonEndVertex.AlgorithmData = true;
            var carbonEnd = molecule.GetAtom(aminoAcidReference.LastAtomId);
            carbonEnd.AminoAcidAtomName = "C";
            if(carbonEnd.Element != ElementName.Carbon)
                throw new Exception("Bug!");

            var carbonEndNeighborVertices = GetNeighbors(molecule, carbonEndVertex).ToList();

            var oxygenVertex = carbonEndNeighborVertices.Single(v => ((Atom) v.Object).Element == ElementName.Oxygen);
            oxygenVertex.AlgorithmData = true;
            var oxygen = (Atom)oxygenVertex.Object;
            oxygen.AminoAcidAtomName = "O";

            var sideChainCarbonVertex = carbonEndNeighborVertices.Single(v => ((Atom)v.Object).Element == ElementName.Carbon);

            var activeChains = new Queue<AtomChainInfo>(new []
            {
                new AtomChainInfo { Vertex = sideChainCarbonVertex, ChainIdx = 1}
            });
            var level = 1;
            while (activeChains.Any())
            {
                var atomChains = activeChains.ToList();
                activeChains.Clear();
                foreach (var atomChain in atomChains)
                {
                    if(atomChain.Vertex.AlgorithmData.Equals(true))
                        continue;
                    atomChain.Vertex.AlgorithmData = true;
                    var atom = (Atom) atomChain.Vertex.Object;
                    var elementSymbol = atom.Element.ToElementSymbol().ToString();
                    var levelLetter = MapLevelToLetter(level);
                    var chainSuffix = atomChains.Select(chain => chain.Vertex.Id).Distinct().Count() > 1
                        ? atomChain.ChainIdx.ToString()
                        : "";

                    atom.AminoAcidAtomName = elementSymbol + levelLetter + chainSuffix;

                    // Add unvisited non-hydrogen neighbors to chain
                    GetNeighbors(molecule, atomChain.Vertex)
                        .Where(v => ((Atom)v.Object).Element != ElementName.Hydrogen)
                        .Where(v => v.AlgorithmData.Equals(false))
                        .Select((v,idx) => new AtomChainInfo
                        {
                            Vertex = v,
                            ChainIdx = atomChain.ChainIdx + idx
                        })
                        .ForEach(chainInfo => activeChains.Enqueue(chainInfo));
                }
                level++;
            }
        }

        private static IEnumerable<Vertex> GetNeighbors(Molecule molecule, Vertex vertex)
        {
            return vertex.EdgeIds
                .Select(edgeId => molecule.MoleculeStructure.Edges[edgeId])
                .Select(edge => edge.Vertex1Id == vertex.Id ? edge.Vertex2Id : edge.Vertex1Id)
                .Distinct()
                .Select(vId => molecule.MoleculeStructure.Vertices[vId]);
        }

        private class AtomChainInfo
        {
            public Vertex Vertex { get; set; }
            public int ChainIdx { get; set; }
        }

        private static char MapLevelToLetter(int level)
        {
            switch (level)
            {
                case 1:
                    return 'A';
                case 2:
                    return 'B';
                case 3:
                    return 'G';
                case 4:
                    return 'D';
                case 5:
                    return 'E';
                case 6:
                    return 'Z';
                case 7:
                    return 'H';
                default:
                    throw new ArgumentOutOfRangeException(nameof(level));
            }
        }
    }
}
