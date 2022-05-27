using System.Collections.Generic;
using MongoDB.Driver;

namespace GenomeTools.Studies.GenomeAnalysis
{
    public static class AsyncEnumerableExtensions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IAsyncCursor<T> cursor)
        {
            while (await cursor.MoveNextAsync())
            {
                foreach (var item in cursor.Current)
                {
                    yield return item;
                }
            }
        }
    }
}
