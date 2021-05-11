using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PerformanceTest
{
    /// <summary>
    /// This performance test is the C# implementation of
    /// a performance comparison between C# and C++. The C++ code is not part of this repository.
    /// Result: C++ code was 3x as fast as C#-implementation.
    /// Relevance: Determine if a molecular dynamics simulation implemented in C# was a good or bad idea.
    /// From the result it's not the obvious choice but wasn't a factor 10 bad and I hence went ahead implementing in C#
    /// because I'm much more productive in C# and ability to test ideas was/is of higher value than good performance.
    /// </summary>
    public static class Program
    {
        public static void Main()
        {
            Console.Write("Bubble sorting...");
            var stopwatch = Stopwatch.StartNew();
            const int dataSize = 1 * 1000;
            var intData = new List<int>(dataSize);
            for (var i = 0; i < dataSize; i++)
            {
                intData.Add(dataSize - i);
            }
            BubbleSorter.Sort(intData);
            stopwatch.Stop();
            Console.WriteLine("DONE");
            Console.WriteLine("Time elapsed: " + stopwatch.Elapsed);

            Console.WriteLine();

            Console.Write("N-body problem...");
            stopwatch.Restart();
            var nBodySolver = new NBodySolver(1000);
            var operations = nBodySolver.Simulate(100, 0.1);
            stopwatch.Stop();
            Console.WriteLine("DONE");
            Console.WriteLine("Operations: " + operations);
            Console.WriteLine("Time elapsed: " + stopwatch.Elapsed);
        }
    }

    public class NBodySolver
    {
        private int bodyCount;
        private readonly List<Body> bodies;
        private readonly Random rng = new Random();

        public NBodySolver(int bodyCount)
        {
            this.bodyCount = bodyCount;
            bodies = new List<Body>(bodyCount);
            for (var bodyIdx = 0; bodyIdx < bodyCount; bodyIdx++)
            {
                var x = RandomDouble(-1.0, 1.0);
                var y = RandomDouble(-1.0, 1.0);
                var z = RandomDouble(-1.0, 1.0);
                var position = new Point3D(x, y, z);
                var body = new Body(position, 1.0);
                bodies.Add(body);
            }
        }

        private double RandomDouble(double min, double max)
        {
            return min + rng.NextDouble()*(max - min);
        }

        public long Simulate(double timeSpan, double timeStep)
        {
            const double G = 0.1;

            var t = 0.0;
            long operations = 0;
            while (t < timeSpan)
            {
                for (var body1Idx = 0; body1Idx < bodyCount - 1; body1Idx++)
                {
                    var body1 = bodies[body1Idx];
                    for (var body2Idx = bodies.Count - 1; body2Idx > body1Idx; body2Idx--)
                    {
                        var body2 = bodies[body2Idx];
                        var distance = body1.Position.DistanceTo(body2.Position);
                        if (distance < body1.Radius + body2.Radius)
                        {
                            double totalMass = body1.Mass + body2.Mass;
                            double body1Weight = body1.Mass / totalMass;
                            double body2Weight = body2.Mass / totalMass;
                            double vX = body1Weight * body1.Velocity.X + body2Weight * body2.Velocity.X;
                            double vY = body1Weight * body1.Velocity.Y + body2Weight * body2.Velocity.Y;
                            double vZ = body1Weight * body1.Velocity.Z + body2Weight * body2.Velocity.Z;
                            body1.Mass = totalMass;
                            body1.UpdateRadius();
                            body1.Velocity.X = vX;
                            body1.Velocity.Y = vY;
                            body1.Velocity.Z = vZ;
                            bodies.RemoveAt(body2Idx);
                            bodyCount--;
                        }
                        operations++;
                    }
                }

                for (var body1Idx = 0; body1Idx < bodyCount; body1Idx++)
                {
                    var body1 = bodies[body1Idx];
                    double Fx = 0, Fy = 0, Fz = 0;
                    for (var body2Idx = 0; body2Idx < bodyCount; body2Idx++)
                    {
                        if (body1Idx == body2Idx)
                            continue;
                        var body2 = bodies[body2Idx];
                        var distance = body1.Position.DistanceTo(body2.Position);
                        var massProduct = body1.Mass * body2.Mass;
                        Fx += (G * massProduct / distance) * (body2.Position.X - body1.Position.X);
                        Fy += (G * massProduct / distance) * (body2.Position.Y - body1.Position.Y);
                        Fz += (G * massProduct / distance) * (body2.Position.Z - body1.Position.Z);
                        operations++;
                    }
                    var dVx = (Fx / body1.Mass) * timeStep;
                    var dVy = (Fy / body1.Mass) * timeStep;
                    var dVz = (Fz / body1.Mass) * timeStep;
                    body1.Velocity.X += dVx;
                    body1.Velocity.Y += dVy;
                    body1.Velocity.Z += dVz;

                    body1.Position.X += body1.Velocity.X * timeStep;
                    body1.Position.Y += body1.Velocity.Y * timeStep;
                    body1.Position.Z += body1.Velocity.Z * timeStep;
                }

                t += timeStep;
            }
            return operations;
        }
    }

    public class Point3D
    {
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public double DistanceTo(Point3D otherPoint)
        {
            var diffX = X - otherPoint.X;
            var diffY = Y - otherPoint.Y;
            var diffZ = Z - otherPoint.Z;
            var distance = Math.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);
            return distance;
        }
    }

    public class Body
    {
        public Point3D Position { get; }
        public Vector3D Velocity { get; }
        public double Mass { get; set; }
        public double Radius { get; private set; }

        public Body(Point3D position, double mass)
        {
            Position = position;
            Velocity = new Vector3D(0,0,0);
            Mass = mass;
            UpdateRadius();
        }

        public void UpdateRadius()
        {
            Radius = Math.Pow(0.75*1e-9*Mass/Math.PI, 1/3.0);
        }
    }

    public class Vector3D
    {
        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public static class BubbleSorter
    {
        public static List<int> Sort(List<int> data)
        {
            var requiredIterations = data.Count;
            for (var i1 = 0; i1 <= requiredIterations; i1++)
            {
                for (var i2 = 0; i2 < requiredIterations-1; i2++)
                {
                    if (data[i2] > data[i2 + 1])
                        SwitchPosition(data, i2, i2 + 1);
                }
            }
            return data;
        }

        private static void SwitchPosition(IList<int> v, int position1, int position2)
        {
            var temp = v[position1];
            v[position1] = v[position2];
            v[position2] = temp;
        }
    }
}
