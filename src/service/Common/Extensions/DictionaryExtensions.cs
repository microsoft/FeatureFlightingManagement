using System.Linq;

namespace System.Collections.Generic
{
    /// <summary>
    /// Useful extensions to Dictionary class
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Merges two dictionaries
        /// </summary>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <param name="otherDictionary" cref="IDictionary{TKey, TValue}">Dictionary to be merged</param>
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> otherDictionary)
        {
            if (otherDictionary == null || !otherDictionary.Any())
                return;

            foreach(KeyValuePair<TKey, TValue> pair in otherDictionary)
            {
                dictionary.AddOrUpdate(pair);
            }
        }
    }
}
