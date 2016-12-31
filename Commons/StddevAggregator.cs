using System;
using System.Collections.Generic;

namespace Commons
{
    /// <summary>
    /// Class for aggregating sum, mean and standard deviation without storing values.
    /// </summary>
    public class StdDevAggregator
    {
        // Accumulated difference between sum of squares and square of sum
        private double squaresDiff;

        /// <summary>
        /// Returns standard deviation aggregator with all values in a collection aggregated.
        /// </summary>
        public static StdDevAggregator Calculate(IEnumerable<double> values)
        {
            var aggregator = new StdDevAggregator();
            foreach (var value in values)
            {
                aggregator.Add(value);
            }
            return aggregator;
        }

        /// <summary>
        /// Number of values added.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Sum of values added.
        /// </summary>
        public double Sum { get; private set; }

        /// <summary>
        /// Average of values added.
        /// </summary>
        public double Mean { get; private set; }

        /// <summary>
        /// Statistical sample variance of numbers added. Returns NaN if count &lt; 2.
        /// </summary>
        public double SampleVariance => Count < 2 ? double.NaN : squaresDiff / (Count - 1);

        /// <summary>
        /// Statistical sample standard deviation of numbers added. Returns NaN if count &lt; 2.
        /// </summary>
        public double SampleStddev => Count < 2 ? double.NaN : Math.Sqrt(SampleVariance);

        /// <summary>
        /// Statistical full population variance of numbers added. Returns NaN if count &lt; 1.
        /// </summary>
        public double PopulationVariance => Count < 1 ? double.NaN : squaresDiff / Count;

        /// <summary>
        /// Statistical full population standard deviation of numbers added. Returns NaN if count &lt; 1.
        /// </summary>
        public double PopulationStddev => Count < 1 ? double.NaN : Math.Sqrt(PopulationVariance);

        /// <summary>
        /// Adds a value to the accumulator. All calculation properties will be updated automatically.
        /// </summary>
        /// <param name="value"></param>
        public void Add(double value)
        {
            // Donald Knuth variance algorithm. Has very stable floating point rounding errors.
            Count++;
            var delta = value - Mean;
            Mean = Mean + delta / Count;
            Sum += value;
            squaresDiff += delta * (value - Mean);
        }
    }
}