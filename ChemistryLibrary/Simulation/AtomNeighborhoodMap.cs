﻿using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation
{
    public class AtomNeighborhoodMap
    {
        private readonly UnitValue distanceThreshold = 1.To(SIPrefix.Nano, Unit.Meter);
        private readonly Molecule molecule;
        private readonly Dictionary<uint, List<uint>> neighborhoodMap = new Dictionary<uint, List<uint>>();

        public AtomNeighborhoodMap(Molecule molecule)
        {
            this.molecule = molecule;
            Update();
        }

        public List<uint> GetNeighborhood(uint atom)
        {
            return neighborhoodMap[atom];
        }

        public void Update()
        {
            var vertices = molecule.MoleculeStructure.Vertices.Select(v => v.Id).ToList();
            neighborhoodMap.Clear();
            vertices.ForEach(vertex => neighborhoodMap.Add(vertex, new List<uint>()));

            var threshold = distanceThreshold;
            for (var idx1 = 0; idx1 < vertices.Count-1; idx1++)
            {
                var vertex1 = vertices[idx1];
                var atom1 = molecule.GetAtom(vertex1);
                for (var idx2 = idx1+1; idx2 < vertices.Count; idx2++)
                {
                    var vertex2 = vertices[idx2];
                    var atom2 = molecule.GetAtom(vertex2);
                    var distance = atom1.Position.DistanceTo(atom2.Position);
                    if (distance < threshold)
                    {
                        neighborhoodMap[vertex1].Add(vertex2);
                        neighborhoodMap[vertex2].Add(vertex1);
                    }
                }
            }
        }
    }
}
