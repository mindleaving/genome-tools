using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;
using MIConvexHull;
using IVertex = MIConvexHull.IVertex;

namespace ChemistryLibrary.Measurements
{
    public class CompactnessMeasurerResult
    {
        public CompactnessMeasurerResult(UnitValue volume, Graph<ApproximatedAminoAcid,OrbitalBond> convexHull)
        {
            Volume = volume;
            ConvexHull = convexHull;
        }

        public UnitValue Volume { get; }
        public Graph<ApproximatedAminoAcid,OrbitalBond> ConvexHull { get; }
    }

    /// <summary>
    /// Measures compactness of peptide chain using convex hull
    /// </summary>
    public static class CompactnessMeasurer
    {
        public static CompactnessMeasurerResult Measure(ApproximatePeptide peptide)
        {
            var vertices = ToVertices(peptide.AminoAcids);
            var convexHull = ConvexHull.Create(vertices);
            var volume = CalculateVolume(convexHull.Result);
            var convexHullGraph = ToGraph(convexHull.Result);
            return new CompactnessMeasurerResult(volume, convexHullGraph);
        }

        private static UnitValue CalculateVolume(ConvexHull<ApproximateAminoAcidVertex3D, DefaultConvexFace<ApproximateAminoAcidVertex3D>> convexHull)
        {
            const double OneThird = 1.0/3.0;
            var volume = 0.0;
            var origin = convexHull.Points.First().Point;
            foreach (var face in convexHull.Faces)
            {
                var faceOrigin = face.Vertices[1].Point;
                var faceVector1 = (face.Vertices[1].Point - face.Vertices[0].Point).ToVector3D();
                var faceVector2 = (face.Vertices[2].Point - face.Vertices[1].Point).ToVector3D();
                var faceArea = 0.5*faceVector1.CrossProduct(faceVector2).Magnitude();
                var blockHeight = origin.DistanceFromPlane(faceOrigin, faceVector1, faceVector2);
                var blockVolume = OneThird*faceArea*blockHeight;
                volume += blockVolume;
            }
            var picoMultiplier = SIPrefix.Pico.GetMultiplier();
            var cubicPicoMeterMultiplier = picoMultiplier * picoMultiplier * picoMultiplier;
            return new UnitValue(
                new CompoundUnit(new []{ SIBaseUnit.Meter, SIBaseUnit.Meter, SIBaseUnit.Meter }), 
                volume*cubicPicoMeterMultiplier );
        }

        private static Graph<ApproximatedAminoAcid,OrbitalBond> ToGraph(ConvexHull<ApproximateAminoAcidVertex3D, DefaultConvexFace<ApproximateAminoAcidVertex3D>> convexHull)
        {
            var graph = new Graph<ApproximatedAminoAcid,OrbitalBond>();

            // Add vertices
            var convexHullToGraphVertexIdMap = new Dictionary<long, uint>();
            foreach (var hullVertex in convexHull.Points)
            {
                var graphVertex = new Vertex<ApproximatedAminoAcid>(graph.GetUnusedVertexId())
                {
                    Object = hullVertex.AminoAcid
                };
                graph.AddVertex(graphVertex);
                convexHullToGraphVertexIdMap.Add(hullVertex.Id, graphVertex.Id);
            }

            // Connect vertices
            foreach (var face in convexHull.Faces)
            {
                for (int v1Idx = 0; v1Idx < face.Vertices.Length - 1; v1Idx++)
                {
                    var vertex1 = face.Vertices[v1Idx];
                    var vertex1GraphId = convexHullToGraphVertexIdMap[vertex1.Id];
                    for (int v2Idx = v1Idx+1; v2Idx < face.Vertices.Length; v2Idx++)
                    {
                        var vertex2 = face.Vertices[v2Idx];
                        var vertex2GraphId = convexHullToGraphVertexIdMap[vertex2.Id];
                        var graphVertex1 = graph.GetVertexFromId(vertex1GraphId);
                        var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(graph, graphVertex1);
                        if(!adjacentVertices.Select(v => v.Id).Contains(vertex2GraphId))
                            graph.AddEdge(new Edge<OrbitalBond>(graph.GetUnusedEdgeId(), vertex1GraphId, vertex2GraphId));
                    }
                }
            }
            return graph;
        }

        private class ApproximateAminoAcidVertex3D : IVertex
        {
            private static long lastId;

            public ApproximateAminoAcidVertex3D(ApproximatedAminoAcid aminoAcid)
            {
                AminoAcid = aminoAcid;
                Id = Interlocked.Increment(ref lastId);
                var carbonAlphaPosition = aminoAcid.CarbonAlphaPosition.In(SIPrefix.Pico, Unit.Meter);
                var x = carbonAlphaPosition.X;
                var y = carbonAlphaPosition.Y;
                var z = carbonAlphaPosition.Z;
                Position = new[] {x, y, z};
                Point = new Point3D(x, y, z);
            }

            public long Id { get; }
            public ApproximatedAminoAcid AminoAcid { get; }
            public double[] Position { get; }
            public Point3D Point { get; }
        }

        private static IList<ApproximateAminoAcidVertex3D> ToVertices(IEnumerable<ApproximatedAminoAcid> aminoAcids)
        {
            return aminoAcids
                .Select(aa => new ApproximateAminoAcidVertex3D(aa))
                .ToList();
        }
    }
}
