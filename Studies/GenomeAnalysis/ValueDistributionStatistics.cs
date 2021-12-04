using System;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.Studies.GenomeAnalysis
{
    public class ValueDistributionStatistics
    {
        public ValueDistributionStatistics(
            double minimum, 
            double percentile10, 
            double percentile25,
            double median, 
            double mean, 
            double percentile75,
            double percentile90, 
            double maximum)
        {
            Minimum = minimum;
            Percentile10 = percentile10;
            Percentile25 = percentile25;
            Median = median;
            Mean = mean;
            Percentile75 = percentile75;
            Percentile90 = percentile90;
            Maximum = maximum;
        }

        public double Minimum { get; private set; }
        public double Percentile10 { get; private set; }
        public double Percentile25 { get; private set; }
        public double Median { get; private set; }
        public double Mean { get; private set; }
        public double Percentile75 { get; private set; }
        public double Percentile90 { get; private set; }
        public double Maximum { get; private set; }

        public static ValueDistributionStatistics Calculate(IEnumerable<double> values)
        {
            var orderedValues = values.OrderBy(x => x).ToList();
            if (orderedValues.Count == 0)
            {
                return new ValueDistributionStatistics(
                    double.NaN,
                    double.NaN,
                    double.NaN,
                    double.NaN,
                    double.NaN,
                    double.NaN,
                    double.NaN,
                    double.NaN);
            }
            var minimum = orderedValues.First();

            var percentile10Index = (int)Math.Round(0.1 * orderedValues.Count);
            var percentile10 = orderedValues[percentile10Index];

            var percentile25Index = (int)Math.Round(0.25 * orderedValues.Count);
            var percentile25 = orderedValues[percentile25Index];

            var medianIndex = (int)Math.Round(0.5 * orderedValues.Count);
            var median = orderedValues[medianIndex];

            var mean = orderedValues.Average();

            var percentile75Index = (int)Math.Round(0.75 * orderedValues.Count);
            var percentile75 = orderedValues[percentile75Index];

            var percentile90Index = (int)Math.Round(0.9 * orderedValues.Count);
            var percentile90 = orderedValues[percentile90Index];

            var maximum = orderedValues.Last();

            return new ValueDistributionStatistics(
                minimum,
                percentile10,
                percentile25,
                median,
                mean,
                percentile75,
                percentile90,
                maximum);
        }
    }
}