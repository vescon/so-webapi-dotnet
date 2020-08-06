using System.Collections.Generic;

namespace Sample
{
    public static class EnumerableExtensions
    {
        public static string Concatenate<T>(this IEnumerable<T> source, string delimiter)
        {
            return string.Join(delimiter, source);
        }
    }
}