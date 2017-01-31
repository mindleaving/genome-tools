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

        public static IEnumerable<Tout> PairwiseOperation<T1, T2, Tout>(this IList<T1> values1, IList<T2> values2, Func<T1, T2, Tout> operation)
        {
            if(values1.Count != values2.Count)
                throw new InvalidOperationException("Cannot apply pairwise operation to lists of different length");
            var result = new List<Tout>(values1.Count);
            for (int idx = 0; idx < values1.Count; idx++)
            {
                result.Add(operation(values1[idx],values2[idx]));
            }
            return result;
        }

        public static T MaximumItem<T>(this IEnumerable<T> collection, Func<T, double> valueSelector)
        {
            Func<T, double> inverseValueSelector = item => -valueSelector(item);
            return collection.MinimumItem(inverseValueSelector);
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