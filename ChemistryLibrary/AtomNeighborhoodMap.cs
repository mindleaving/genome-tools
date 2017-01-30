using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary
{
    public class AtomNeighborhoodMap
    {
        private readonly UnitValue distanceThreshold = 300.To(SIPrefix.Pico, Unit.Meter);
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
            var vertices = molecule.MoleculeStructure.Vertices.Keys.ToList();
            neighborhoodMap.Clear();
            vertices.ForEach(vertex => neighborhoodMap.Add(vertex, new List<uint>()));

            var thresholdInPicoMeter = distanceThreshold.In(SIPrefix.Pico, Unit.Meter);
            for (var idx1 = 0; idx1 < vertices.Count-1; idx1++)
            {
                var vertex1 = vertices[idx1];
                var atom1 = molecule.GetAtom(vertex1);
                for (var idx2 = idx1+1; idx2 < vertices.Count; idx2++)
                {
                    var vertex2 = vertices[idx2];
                    var atom2 = molecule.GetAtom(vertex2);
                    var distanceInPicoMeter = atom1.Position
                        .In(SIPrefix.Pico, Unit.Meter)
                        .DistanceTo(atom2.Position.In(SIPrefix.Pico, Unit.Meter));
                    if (distanceInPicoMeter < thresholdInPicoMeter)
                    {
                        neighborhoodMap[vertex1].Add(vertex2);
                        neighborhoodMap[vertex2].Add(vertex1);
                    }
                }
            }
        }
    }
}
