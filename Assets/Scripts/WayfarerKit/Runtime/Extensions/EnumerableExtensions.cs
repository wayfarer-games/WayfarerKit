using System;
using System.Collections.Generic;

namespace WayfarerKit.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Perform an action on each item in a sequence.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (var item in sequence) action(item);
        }
    }
}