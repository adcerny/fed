using System.Collections.Generic;

namespace Fed.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static IDictionary<TKey, TValue> Clone<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            var clone = new Dictionary<TKey, TValue>();
            foreach (var x in source) clone.Add(x.Key, x.Value);
            return clone;
        }
    }
}