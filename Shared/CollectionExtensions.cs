using System.Collections.Generic;
using System.Linq;

namespace LFO.Shared
{
    public static class CollectionExtensions
    {
        /// <summary>A helper to add items to an array</summary>
        /// <typeparam name="T">The inner type of the collection</typeparam>
        /// <param name="sequence">The array</param>
        /// <param name="items">The items to add</param>
        /// <returns>The array containing the items</returns>
        public static T[] AddRange<T>(this IEnumerable<T> sequence, IEnumerable<T> items) =>
            (sequence ?? Enumerable.Empty<T>()).Concat(items).ToArray();

        /// <summary>A helper to add an item to a collection</summary>
        /// <typeparam name="T">The inner type of the collection</typeparam>
        /// <param name="sequence">The collection</param>
        /// <param name="item">The item to add</param>
        /// <returns>The collection containing the item</returns>
        public static IEnumerable<T> AddCollectionItem<T>(this IEnumerable<T> sequence, T item) =>
            (sequence ?? Enumerable.Empty<T>()).Concat(new[]
            {
                item
            });
    }
}