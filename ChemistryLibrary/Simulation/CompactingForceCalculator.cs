using System.Collections.Generic;
using ChemistryLibrary.Measurements;
using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.Simulation
{
    public class CompactingForceCalculator
    {
        private const double ForceDistanceScaling = 0.0;

        public Dictionary<ApproximatedAminoAcid, ApproximateAminoAcidForces> Calculate(
            CompactnessMeasurerResult compactnessMeasurerResult)
        {
            var forceDictionary = new Dictionary<ApproximatedAminoAcid, ApproximateAminoAcidForces>();

            var convexHull = compactnessMeasurerResult.ConvexHull;
            foreach (var vertex in convexHull.Vertices.Values)
            {
                var aminoAcid = (ApproximatedAminoAcid) vertex.Object;
                var vertexPosition = aminoAcid.CarbonAlphaPosition;
                if (!forceDictionary.ContainsKey(aminoAcid))
                    forceDictionary.Add(aminoAcid, new ApproximateAminoAcidForces());

                var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(convexHull, vertex);
                foreach (var adjacentVertex in adjacentVertices)
                {
                    var adjacentAminoAcid = (ApproximatedAminoAcid) adjacentVertex.Object;
                    var adjacentPosition = adjacentAminoAcid.CarbonAlphaPosition;
                    var connectingVector = vertexPosition.VectorTo(adjacentPosition);
                    var distance = connectingVector.Magnitude();
                    var forceMagnitude = 1e-9*(1.0 + ForceDistanceScaling * distance.In(SIPrefix.Pico, Unit.Meter));
                    var forceDirection = connectingVector.In(SIPrefix.Pico, Unit.Meter).Normalize();
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
