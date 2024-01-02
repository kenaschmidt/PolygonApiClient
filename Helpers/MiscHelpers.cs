using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    internal static class MiscHelpers
    {

        public static T AddAndReturn<T>(this ICollection<T> collection, T item)
        {
            collection.Add(item);
            return item;
        }
        public static TValue AddAndReturn<TKey, TValue>(this IDictionary<TKey, TValue> collection, TKey key, TValue value)
        {
            collection.Add(key, value);
            return value;
        }

    }
}
