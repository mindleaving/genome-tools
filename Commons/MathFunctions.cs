using System;

namespace Commons
{
    public static class MathFunctions
    {
        public static double Sigmoid(double x)
        {
            return 1.0/(1 + Math.Exp(-x));
        }
    }
}
