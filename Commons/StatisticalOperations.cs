using System;

namespace Commons
{
    public static class StatisticalOperations
    {
        public static double MSE(double[] y, double[] yhat)
        {
            if (y == null || yhat == null)
            {
                return 0;
            }
            double MSE = 0;
            int i = 0;
            while (y.Length > i && yhat.Length > i)
            {
                if (double.IsNaN(y[i]) || double.IsNaN(yhat[i]))
                {
                    i++;
                    continue;
                }
                MSE += Math.Pow(y[i] - yhat[i], 2);
                i++;
            }
            return MSE;
        }
        public static double Correlation(double[] X, double[] Y)
        {
            if (X == null || Y == null
                || X.Length != Y.Length)
            {
                return double.NaN;
            }

            var varianceCalcX = StdDevAggregator.Calculate(X);
            var varianceCalcY = StdDevAggregator.Calculate(Y);

            double corr = 0;
            double meanX = varianceCalcX.Mean;
            double meanY = varianceCalcY.Mean;
            double varX = varianceCalcX.SampleVariance;
            double varY = varianceCalcY.SampleVariance;
            for (int i = 0; i < X.Length; i++)
            {
                corr += (X[i] - meanX) * (Y[i] - meanY);
            }
            corr /= Math.Sqrt(varX * varY) * X.Length;
            return corr;
        }
        public static double Mean(double[] X)
        {
            double mean = 0;
            for (int i = 0; i < X.Length; i++)
            {
                mean += X[i];
            }
            mean /= X.Length;
            return mean;
        }

        public static double Covariance(double[] X, double[] Y)
        {
            if (X.Length != Y.Length)
            {
                return double.NaN;
            }
            double meanX = Mean(X);
            double meanY = Mean(Y);
            return Covariance(X, Y, meanX, meanY);
        }
        public static double Covariance(double[] X, double[] Y, double meanX, double meanY)
        {
            if (X.Length != Y.Length)
            {
                return double.NaN;
            }
            double covariance = 0;
            for (int i = 0; i < X.Length; i++)
            {
                covariance += (X[i] - meanX) * (Y[i] - meanY);
            }
            covariance /= X.Length;
            return covariance;
        }
    }
}