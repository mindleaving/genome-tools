using System;
using System.Collections.Generic;

namespace Commons
{
    public class ShortestPathLookup
    {
        public readonly Vertex Source;
        private readonly Dictionary<Vertex, GraphPath> paths;

        public ShortestPathLookup(Vertex source, Dictionary<Vertex, GraphPath> paths)
        {
            Source = source;
            this.paths = paths;
        }

        public GraphPath PathTo(Vertex target)
        {
            if (!paths.ContainsKey(target))
                throw new ArgumentException("Target is not in graph");

            return paths[target];
        }

        public double PathLengthTo(Vertex target)
        {
            return !paths.ContainsKey(target) ? Double.PositiveInfinity : paths[target].PathLength;
        }

        public void RecalculateAllPathLengths()
        {
            paths.Values.ForEach(path => path.RecalculatePathLength());
        }
    }
}