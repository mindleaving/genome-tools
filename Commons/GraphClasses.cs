using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace Commons
{
    [DataContract]
    public class Graph
    {
        [IgnoreDataMember]
        private ulong nextEdgeId;
        public ulong GetUnusedEdgeId()
        {
            return nextEdgeId++;
        }

        public uint GetUnusedVertexId()
        {
            if (!Vertices.Any())
                return 0;
            return Vertices.Max(v => v.Key) + 1;
        }

        [DataMember]
        public Dictionary<ulong, Edge> Edges { get; private set; }
        [DataMember]
        public Dictionary<uint, Vertex> Vertices { get; private set; }

        public Graph()
        {
            Edges = new Dictionary<ulong, Edge>();
            Vertices = new Dictionary<uint, Vertex>();
        }
        public Graph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
            : this()
        {
            AddVertices(vertices);
            AddEdges(edges);
        }

        public void AddVertex(Vertex newVertex)
        {
            if (Vertices.ContainsKey(newVertex.Id))
                throw new ArgumentException($"Vertex with ID '{newVertex.Id}' already exists.");

            Vertices.Add(newVertex.Id, newVertex);
        }

        public void AddVertices(IEnumerable<Vertex> newVertices)
        {
            newVertices.ForEach(AddVertex);
        }

        public bool RemoveVertex(Vertex vertex)
        {
            vertex.EdgeIds.ForEach(e => Edges.Remove(e));
            return Vertices.Remove(vertex.Id);
        }

        public bool RemoveVertex(uint vertexId)
        {
            if (Vertices.ContainsKey(vertexId))
                return false;
            var vertex = Vertices[vertexId];
            return RemoveVertex(vertex);
        }

        public void AddEdge(Edge newEdge)
        {
            if (!Vertices.ContainsKey(newEdge.Vertex1Id) || !Vertices.ContainsKey(newEdge.Vertex2Id))
                throw new Exception("Cannot add edge because one or both of its vertices are not in the graph");

            // Add edge to vertices
            Vertices[newEdge.Vertex1Id].EdgeIds.Add(newEdge.Id);
            Vertices[newEdge.Vertex2Id].EdgeIds.Add(newEdge.Id);

            Edges.Add(newEdge.Id, newEdge);
            if (newEdge.Id >= nextEdgeId)
                nextEdgeId = newEdge.Id + 1;
        }

        public void AddEdges(IEnumerable<Edge> edges)
        {
            edges.ForEach(AddEdge);
        }

        public bool RemoveEdge(Edge edge)
        {
            if (!Edges.ContainsKey(edge.Id))
                return false;

            Vertices[edge.Vertex1Id].RemoveEdge(edge);
            Vertices[edge.Vertex2Id].RemoveEdge(edge);

            return Edges.Remove(edge.Id);
        }

        public Edge ConnectVertices(Vertex vertex1, Vertex vertex2)
        {
            if (!Vertices.ContainsKey(vertex1.Id) || !Vertices.ContainsKey(vertex2.Id))
                throw new Exception("Cannot add edge because one or both of its vertices are not in the graph");

            var newEdge = new Edge(GetUnusedEdgeId(), vertex1.Id, vertex2.Id);
            AddEdge(newEdge);
            return newEdge;
        }

        public Edge ConnectVertices(uint vertex1Id, uint vertex2Id)
        {
            var vertex1 = Vertices[vertex1Id];
            var vertex2 = Vertices[vertex2Id];

            return ConnectVertices(vertex1, vertex2);
        }

        public GraphMergeInfo AddGraph(Graph otherGraph)
        {
            var vertexIdMap = new Dictionary<uint, uint>();
            foreach (var vertex in otherGraph.Vertices.Values)
            {
                var newVertexId = GetUnusedVertexId();
                AddVertex(new Vertex(newVertexId, vertex.Weight)
                {
                    Object = vertex.Object
                });
                vertexIdMap.Add(vertex.Id, newVertexId);
            }
            var edgeIdMap = new Dictionary<ulong, ulong>();
            foreach (var edge in otherGraph.Edges.Values)
            {
                var newEdgeId = GetUnusedEdgeId();
                AddEdge(new Edge(newEdgeId, 
                    vertexIdMap[edge.Vertex1Id], 
                    vertexIdMap[edge.Vertex2Id],
                    edge.Weight,
                    edge.IsDirected)
                {
                    Object = edge.Object
                });
                edgeIdMap.Add(edge.Id, newEdgeId);
            }
            return new GraphMergeInfo(vertexIdMap, edgeIdMap);
        }
    }

    public class GraphMergeInfo
    {
        public GraphMergeInfo(Dictionary<uint, uint> vertexIdMap, Dictionary<ulong, ulong> edgeIdMap)
        {
            VertexIdMap = vertexIdMap;
            EdgeIdMap = edgeIdMap;
        }

        public Dictionary<uint, uint> VertexIdMap { get; }
        public Dictionary<ulong, ulong> EdgeIdMap { get; }

    }

    [DataContract]
    //[KnownType(typeof(TaxiEdge))]
    public class Edge
    {
        [DataMember]
        public ulong Id { get; private set; }
        /// <summary>
        /// Property for holding an object which is represented by this edge.
        /// Intended to make it possible to model problems as graphs.
        /// </summary>
        [DataMember]
        public object Object { get; set; }

        [DataMember]
        public uint Vertex1Id { get; private set; }
        [DataMember]
        public uint Vertex2Id { get; private set; }
        [DataMember]
        public bool IsDirected { get; set; }
        [DataMember]
        public double Weight { get; set; }

        /// <summary>
        /// Property used for efficient implementations of graph algorithms.
        /// E.g. could hold a flag if this vertex has already been visited
        /// </summary>
        [IgnoreDataMember]
        public object AlgorithmData { get; set; }

        public Edge(ulong id, uint vertex1Id, uint vertex2Id, double weight = 1.0, bool isDirected = false)
        {
            Id = id;
            Vertex1Id = vertex1Id;
            Vertex2Id = vertex2Id;
            Weight = weight;
            IsDirected = isDirected;
            Object = null;
        }

        public bool HasVertex(Vertex vertex)
        {
            return HasVertex(vertex.Id);
        }

        public bool HasVertex(uint vertexId)
        {
            return Vertex1Id.Equals(vertexId) || Vertex2Id.Equals(vertexId);
        }

        public override string ToString()
        {
            var directednesSymbol = IsDirected ? "->" : "<->";
            return $"V{Vertex1Id} {directednesSymbol} V{Vertex2Id}, {Object}";
        }
    }

    [DataContract]
    //[KnownType(typeof(GeoCoordinate))]
    public class Vertex
    {
        /// <summary>
        /// Property for holding an object which is represented by this vertex.
        /// Intended to make it possible to model problems as graphs.
        /// </summary>
        [DataMember]
        public object Object { get; set; }

        [DataMember]
        public uint Id { get; private set; }
        [DataMember]
        public List<ulong> EdgeIds { get; private set; } = new List<ulong>();
        [DataMember]
        public double Weight { get; set; }

        /// <summary>
        /// Property used for efficient implementations of graph algorithms.
        /// E.g. could hold a flag if this vertex has already been visited
        /// </summary>
        [IgnoreDataMember]
        public object AlgorithmData { get; set; }

        public Vertex(uint id, double weight = 1.0)
        {
            Id = id;
            Weight = weight;
        }

        internal void AddEdge(Edge edge)
        {
            if (edge.Vertex1Id != Id || edge.Vertex2Id != Id)
                throw new ArgumentException("Cannot add edge to vertex if vertex is not an endpoint");

            EdgeIds.Add(edge.Id);
        }

        internal bool RemoveEdge(Edge edge)
        {
            return EdgeIds.Remove(edge.Id);
        }

        public override string ToString()
        {
            return $"V{Id}, #Edges: {EdgeIds.Count}";
        }
    }

    [DataContract]
    public class GraphPath
    {
        [DataMember]
        public uint StartVertexId { get; set; }
        [DataMember]
        public LinkedList<Edge> Path { get; private set; }
        [DataMember]
        public double PathLength { get; private set; }

        public GraphPath(uint startVertexId)
        {
            StartVertexId = startVertexId;
            PathLength = 0.0;
            Path = new LinkedList<Edge>();
        }

        public GraphPath(uint startVertexId, IList<Edge> edges)
            : this(startVertexId)
        {
            ValidateEdgeList(edges);
            Path = new LinkedList<Edge>(edges);
            RecalculatePathLength();
        }

        public GraphPath(uint startVertexId, GraphPath path)
            : this(startVertexId)
        {
            // No validation necessary
            Path = new LinkedList<Edge>(path.Path);
            PathLength = path.PathLength;
        }

        public void Append(Edge edge)
        {
            if(Path.Any())
                ValidateEdgePair(Path.Last.Value, edge);
            Path.AddLast(edge);
            PathLength += edge.Weight;
        }

        public void RecalculatePathLength()
        {
            PathLength = Path.Sum(e => e.Weight);
        }

        private void ValidateEdgeList(IList<Edge> edges)
        {
            for (int edgeIdx = 0; edgeIdx < edges.Count - 1; edgeIdx++)
            {
                var currentEdge = edges[edgeIdx];
                var nextEdge = edges[edgeIdx + 1];
                ValidateEdgePair(currentEdge, nextEdge);
            }
        }

        private static void ValidateEdgePair(Edge currentEdge, Edge nextEdge)
        {
            if (currentEdge.IsDirected)
            {
                if (nextEdge.IsDirected)
                {
                    if (currentEdge.Vertex2Id != nextEdge.Vertex1Id)
                        throw new ArgumentException(nameof(GraphPath) + ": At least one pair of edges in path are not connected");
                }
                else
                {
                    if(currentEdge.Vertex2Id != nextEdge.Vertex1Id && currentEdge.Vertex2Id != nextEdge.Vertex2Id)
                        throw new ArgumentException(nameof(GraphPath) + ": At least one pair of edges in path are not connected");
                }
            }
            else
            {
                if (nextEdge.IsDirected)
                {
                    if(currentEdge.Vertex1Id != nextEdge.Vertex1Id && currentEdge.Vertex2Id != nextEdge.Vertex1Id)
                        throw new ArgumentException(nameof(GraphPath) + ": At least one pair of edges in path are not connected");
                }
                else
                {
                    if(currentEdge.Vertex1Id != nextEdge.Vertex1Id && currentEdge.Vertex2Id != nextEdge.Vertex1Id
                        && currentEdge.Vertex1Id != nextEdge.Vertex2Id && currentEdge.Vertex2Id != nextEdge.Vertex2Id)
                        throw new ArgumentException(nameof(GraphPath) + ": At least one pair of edges in path are not connected");
                }
            }
        }

        public override string ToString()
        {
            return PathLength.ToString(CultureInfo.InvariantCulture);
        }
    }
}