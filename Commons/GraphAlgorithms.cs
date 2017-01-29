using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons
{
    public static class GraphAlgorithms
    {
        public static double[,] ComputeAdjacencyMatrix(Graph graph)
        {
            var adjacencyMatrix = new double[graph.Vertices.Count, graph.Vertices.Count];

            var sortedVertexIds = graph.Vertices.Values.Select(v => v.Id).OrderBy(x => x).ToList();
            foreach (var edge in graph.Edges.Values)
            {
                var vertex1Index = sortedVertexIds.IndexOf(edge.Vertex1Id);
                var vertex2Index = sortedVertexIds.IndexOf(edge.Vertex2Id);
                adjacencyMatrix[vertex1Index, vertex2Index]++;
                adjacencyMatrix[vertex2Index, vertex1Index]++;
            }

            return adjacencyMatrix;
        }

        public static ShortestPathLookup ShortestPaths(Graph graph, Vertex source)
        {
            if (!graph.Vertices.ContainsKey(source.Id))
                throw new ArgumentException("GraphAlgorithms.ShortestPath: Source vertex not in graph");
            if (graph.Edges.Values.Any(e => e.Weight < 0))
                throw new NotImplementedException("GraphAlgorithms.ShortestPath: No shortest path algorithm is implemented for graphs with negative edge-weights");

            // Dijkstra's algorithm
            var unVisitedVertexDictionary = graph.Vertices.ToDictionary(v => v.Key, v => true);
            var shortestPathLengths = graph.Vertices.ToDictionary(v => v.Key, v => double.PositiveInfinity);
            shortestPathLengths[source.Id] = 0;
            var shortestPaths = new Dictionary<Vertex, GraphPath>
            {
                {source,new GraphPath(source.Id)}
            };

            while (unVisitedVertexDictionary.ContainsValue(true))
            {
                var currentVertex = graph.Vertices
                    .Where(v => unVisitedVertexDictionary[v.Key])
                    .OrderBy(v => shortestPathLengths[v.Key])
                    .First().Value;
                unVisitedVertexDictionary[currentVertex.Id] = false;

                // If all un-visisted vertices have +inf path lengths, the graph is not connected
                // Either stop here and return vertices with +inf path length
                // or throw an exception.
                if (double.IsPositiveInfinity(shortestPathLengths[currentVertex.Id]))
                    break;//throw new Exception("GraphAlgorithms.ShortestPath: Graph appears to be not connected.");

                // Update adjacent vertices
                var adjacentEdgeVertexDictionary = currentVertex.EdgeIds
                    .Select(e => graph.Edges[e])
                    .Where(e => !e.IsDirected || e.Vertex1Id == currentVertex.Id)
                    .Select(e => new { VertexId = e.Vertex1Id != currentVertex.Id ? e.Vertex1Id : e.Vertex2Id, Edge =e });
                var currentVertexPathLength = shortestPathLengths[currentVertex.Id];
                var currentShortestPath = shortestPaths[currentVertex];
                foreach (var vertexEdgePair in adjacentEdgeVertexDictionary)
                {
                    var adjacentVertex = graph.Vertices[vertexEdgePair.VertexId];
                    var adjacentEdge = vertexEdgePair.Edge;
                    // Skip already visited vertices
                    if (!unVisitedVertexDictionary[adjacentVertex.Id])
                        continue;
                    var currentShortestPathLength = shortestPathLengths[adjacentVertex.Id];
                    if (currentShortestPathLength < currentVertexPathLength + adjacentEdge.Weight)
                        continue;

                    shortestPathLengths[adjacentVertex.Id] = currentVertexPathLength + adjacentEdge.Weight;
                    var newShortestPath = new GraphPath(source.Id, currentShortestPath);
                    newShortestPath.Append(adjacentEdge);
                    if (shortestPaths.ContainsKey(adjacentVertex))
                        shortestPaths[adjacentVertex] = newShortestPath;
                    else
                        shortestPaths.Add(adjacentVertex, newShortestPath);
                }
            }
            return new ShortestPathLookup(source, shortestPaths);
        }

        public static bool IsGraphConnected(Graph graph)
        {
            if (!graph.Vertices.Any())
                return true;
            var startVertex = graph.Vertices.Values.First();
            var connectedVertices = GetConnectedSubgraph(graph, startVertex);
            return graph.Vertices.Count == connectedVertices.Count();
        }

        public static void ApplyMethodToAllConnectedVertices(Graph graph, Vertex startVertex, Action<Vertex> action)
        {
            foreach (var connectedVertex in GetConnectedSubgraph(graph, startVertex))
            {
                action(connectedVertex);
            }
        }

        public static IEnumerable<Vertex> GetConnectedSubgraph(Graph graph, Vertex startVertex)
        {
            // Use depth first search for traversing connected component
            // Initialize graph algorithm data
            graph.Vertices.ForEach(v => v.Value.AlgorithmData = false);
            graph.Edges.Values.ForEach(e => e.AlgorithmData = false);

            foreach (var connectedVertex in GetConnectedSubgraphStep(graph, startVertex))
            {
                yield return connectedVertex;
            }
        }

        private static IEnumerable<Vertex> GetConnectedSubgraphStep(Graph graph, Vertex currentVertex)
        {
            // Mark vertex as visited
            currentVertex.AlgorithmData = true;

            var unvisitedAdjacentVertices = currentVertex.EdgeIds
                .Select(edgeId => graph.Edges[edgeId])
                .Select(edge => edge.Vertex1Id == currentVertex.Id ? edge.Vertex2Id : edge.Vertex1Id);

            foreach (var adjacentVertexId in unvisitedAdjacentVertices)
            {
                var adjacentVertex = graph.Vertices[adjacentVertexId];
                if (adjacentVertex.AlgorithmData.Equals(true))
                    continue;

                foreach (var vertex in GetConnectedSubgraphStep(graph, adjacentVertex))
                {
                    yield return vertex;
                }
            }
            yield return currentVertex;
        }

        public static IEnumerable<Vertex> GetAdjacentVertices(Graph graph, Vertex vertex)
        {
            return vertex.EdgeIds
                .Select(edgeId => graph.Edges[edgeId])
                .Select(edge => edge.Vertex1Id == vertex.Id ? edge.Vertex2Id : edge.Vertex1Id)
                .Distinct()
                .Select(vId => graph.Vertices[vId]);
        }
    }
}