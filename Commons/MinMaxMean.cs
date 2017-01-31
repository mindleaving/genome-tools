using System.Collections.Generic;

namespace Commons
{
    public class MinMaxMean
    {
        public double Minimum { get; }
        public double Maximum { get; }
        public double Mean { get; }
        public double Span => Maximum - Minimum;

        public MinMaxMean(IEnumerable<double> values)
        {
            var min = double.PositiveInfinity;
            var max = double.NegativeInfinity;
            var mean = 0.0;
            var count = 0;
            foreach (var value in values)
            {
                if (double.IsNaN(value))
                    continue;
                if (value < min)
                    min = value;
                if (value > max)
                    max = value;
                mean += value;
                count++;
            }
            if (count > 0)
                mean /= count;
            Minimum = count > 0 ? min : double.NaN;
            Maximum = count > 0 ? max : double.NaN;
            Mean = count > 0 ? mean : double.NaN;
        }
    }
}