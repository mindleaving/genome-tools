using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> ts, Action<T> @do)
        {
            foreach (var t in ts)
            {
                @do(t);
            }
        }

        public static T MinimumItem<T>(this IEnumerable<T> collection, Func<T, double> valueSelector)
        {
            var collectionList = collection.ToList();
            if (!collectionList.Any())
                throw new InvalidOperationException("Sequence does not contain any items");

            var minValue = double.PositiveInfinity;
            var minValueItem = default(T);
            foreach (var item in collectionList)
            {
                var itemValue = valueSelector(item);
                if (itemValue < minValue)
                {
                    minValue = itemValue;
                    minValueItem = item;
                }
            }
            if (double.IsPositiveInfinity(minValue))
                throw new InvalidOperationException("No minimum found");
            return minValueItem;
        }
    }
}