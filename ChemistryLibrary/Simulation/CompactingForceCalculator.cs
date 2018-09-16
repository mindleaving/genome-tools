using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Measurements;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;

namespace ChemistryLibrary.Simulation
{
    public class CompactingForceCalculator
    {
        private const double ForceDistanceScaling = 0.0;
        private const double ForceScaling = 1e-12;

        public Dictionary<ApproximatedAminoAcid, ApproximateAminoAcidForces> Calculate(
            CompactnessMeasurerResult compactnessMeasurerResult)
        {
            // VALIDATED: Forces point in the right direction, chain is compacted.
            // If force is reversed, chain is stretched out to a line.

            var forceDictionary = new Dictionary<ApproximatedAminoAcid, ApproximateAminoAcidForces>();

            var convexHull = compactnessMeasurerResult.ConvexHull;
            foreach (var vertex in convexHull.Vertices)
            {
                var aminoAcid = vertex.Object;
                var vertexPosition = aminoAcid.CarbonAlphaPosition;
                if (!forceDictionary.ContainsKey(aminoAcid))
                    forceDictionary.Add(aminoAcid, new ApproximateAminoAcidForces());

                var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(convexHull, vertex).Cast<IVertex<ApproximatedAminoAcid>>();
                foreach (var adjacentVertex in adjacentVertices)
                {
                    var adjacentAminoAcid = adjacentVertex.Object;
                    var adjacentPosition = adjacentAminoAcid.CarbonAlphaPosition;
                    var connectingVector = vertexPosition.VectorTo(adjacentPosition);
                    var distance = connectingVector.Magnitude();
                    var forceMagnitude = ForceScaling * (1.0 + ForceDistanceScaling * distance.In(SIPrefix.Pico, Unit.Meter));
                    var forceDirection = connectingVector.In(SIPrefix.Pico, Unit.Meter).Normalize().ToVector3D();
                    if (!forceDictionary.ContainsKey(adjacentAminoAcid))
                        forceDictionary.Add(adjacentAminoAcid, new ApproximateAminoAcidForces());
                    AddForce(forceDictionary[aminoAcid], forceDirection, forceMagnitude);
                    AddForce(forceDictionary[adjacentAminoAcid], -forceDirection, forceMagnitude);
                }
            }
            return forceDictionary;
        }

        private void AddForce(ApproximateAminoAcidForces aminoAcidForces, Vector3D forceDirection, double forceMagnitude)
        {
            var force = forceMagnitude * forceDirection.To(Unit.Newton);
            aminoAcidForces.NitrogenForce += force;
            aminoAcidForces.CarbonAlphaForce += force;
            aminoAcidForces.CarbonForce += force;
        }
    }
}
