using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons
{
    public static class MathExtensions
    {
        public static int Modulus(this int n, int modulus)
        {
            var residual = n % modulus;
            return residual < 0 ? residual + modulus : residual;
        }
        public static double Modulus(this double n, double modulus)
        {
            var residual = n % modulus;
            return residual < 0 ? residual + modulus : residual;
        }

        public static bool IsEven(this int i)
        {
            return i%2 == 0;
        }
        public static bool IsOdd(this int i)
        {
            return !i.IsEven();
        }

        public static bool IsNaN(this double value)
        {
            return double.IsNaN(value);
        }

        /// <summary>
        /// Calculates the small angle between two headings
        /// </summary>
        public static double CircularDifferene(this double angle1, double angle2)
        {
            var modulusAngle1 = angle1.Modulus(360);
            var modulusAngle2 = angle2.Modulus(360);
            var greaterAngle = Math.Max(modulusAngle1, modulusAngle2);
            var smallerAngle = Math.Min(modulusAngle1, modulusAngle2);
            return greaterAngle - smallerAngle > 180
                ? 360 - (greaterAngle - smallerAngle)
                : greaterAngle - smallerAngle;
        }

        public static double CircularDifferene(this int angle1, double angle2)
        {
            return CircularDifferene((double) angle1, angle2);
        }

        public static double RoundToNearest(this double value, double resolution)
        {
            return Math.Round(value / resolution) * resolution;
        }
        public static double RoundDownToNearest(this double value, double resolution)
        {
            return Math.Floor(value / resolution) * resolution;
        }
        public static double RoundUpToNearest(this double value, double resolution)
        {
            return Math.Ceiling(value / resolution) * resolution;
        }

        public static int RoundToNearest(this int value, int resolution)
        {
            return (int) ((double) value).RoundToNearest(resolution);
        }
        public static int RoundDownToNearest(this int value, int resolution)
        {
            return (int)((double)value).RoundDownToNearest(resolution);
        }
        public static int RoundUpToNearest(this int value, int resolution)
        {
            return (int)((double)value).RoundUpToNearest(resolution);
        }

        public static double Median(this IEnumerable<double> items)
        {
            var itemList = items.ToList();
            var halfIndex = itemList.Count / 2;
            if (itemList.Count.IsEven())
            {
                return (itemList[halfIndex - 1] + itemList[halfIndex])/2.0;
            }
            return itemList[halfIndex];
        }
        public static double Median<T>(this IEnumerable<T> items, Func<T,double> valueSelector)
        {
            return items.Select(valueSelector).Median();
        }
    }
}
