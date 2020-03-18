﻿using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Paradox.Common
{
    internal static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var obj in enumerable)
            {
                action(obj);
            }
        }

        public static string ToJson(this object obj, bool indented = true) {
            return JsonConvert.SerializeObject(obj, (indented ? Formatting.Indented : Formatting.None), new JsonConverter[] { new StringEnumConverter() });
        }
        public static bool ExpiredSince(this DateTime dateTime, int minutes)
        {
            return (dateTime - DateTime.Now).TotalMinutes < minutes;
        }
        public static FileInfo CombineFile(this DirectoryInfo dir, params string[] paths) {
            var final = dir.FullName;
            foreach (var path in paths) {
                final = Path.Combine(final, path);
            }
            return new FileInfo(final);
        }

        public static string StringJoin(this IEnumerable<object> enumerable, string separator = ", ")
        {
            return string.Join(separator, enumerable);
        }


        public static IEnumerable<T> NullToEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? new T[] { };
        }

        /// <summary>
        /// <para>If the specified key is not already associated with a value (or is mapped to <c>null</c>), attempts to compute
        /// its value using the given mapping function and enters it into this dictionary unless <c>null</c>.</para>
        /// <para>If the function returns null no mapping is recorded. If the function itself throws an exception,
        /// the exception is rethrown, and no mapping is recorded. The most common usage is to construct a new object
        /// serving as an initial mapped value or memoized result. </para>
        /// </summary>
        /// <example>
        /// <code>
        ///    dictionary.ComputeIfAbsent(key, k => new Value());
        /// </code>
        /// </example>
        /// <example>
        ///  <code>
        ///    dictionary.ComputeIfAbsent(key, k => new List()).Add("value");
        /// </code>
        /// </example>
        /// <param name="dictionary">The Dictionary</param>
        /// <param name="key">The key with which the specified value is to be associated</param>
        /// <param name="func">The function to compute a value</param>
        /// <typeparam name="TK">The key type</typeparam>
        /// <typeparam name="TV">The value Type</typeparam>
        /// <returns>the current (existing or computed) value associated with the specified key, or null if the computed value is null</returns>
        public static TV ComputeIfAbsent<TK, TV>(this IDictionary<TK, TV> dictionary, TK key, Func<TK, TV> func)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = func(key);
            }

            return dictionary[key];
        }

        /// <summary>
        /// Puts the contents of an <see cref="IEnumerable{T}"/> into the dictionary.  Existing keys will be overriden.
        /// </summary>
        /// <param name="dictionary">The Dictionary</param>
        /// <param name="toPut">The <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/>s to add</param>
        /// <typeparam name="TK">The key type</typeparam>
        /// <typeparam name="TV">The value Type</typeparam>
        public static void PutAll<TK, TV>(this IDictionary<TK, TV> dictionary,
            IEnumerable<KeyValuePair<TK, TV>> toPut)
        {
            foreach (var keyValuePair in toPut)
            {
                dictionary[keyValuePair.Key] = keyValuePair.Value;
            }
        }

        internal static void MoveItemUp<T>(this ObservableCollection<T> baseCollection, int selectedIndex)
        {
            //# Check if move is possible
            if (selectedIndex <= 0)
                return;

            //# Move-Item
            baseCollection.Move(selectedIndex - 1, selectedIndex);
        }

        internal static void MoveItemDown<T>(this ObservableCollection<T> baseCollection, int selectedIndex)
        {
            //# Check if move is possible
            if (selectedIndex < 0 || selectedIndex + 1 >= baseCollection.Count)
                return;

            //# Move-Item
            baseCollection.Move(selectedIndex + 1, selectedIndex);
        }

        internal static void MoveItemDown<T>(this ObservableCollection<T> baseCollection, T selectedItem)
        {
            //# MoveDown based on Item
            baseCollection.MoveItemDown(baseCollection.IndexOf(selectedItem));
        }

        internal static void MoveItemUp<T>(this ObservableCollection<T> baseCollection, T selectedItem)
        {
            //# MoveUp based on Item
            baseCollection.MoveItemUp(baseCollection.IndexOf(selectedItem));
        }

        internal static bool Matches(this ModDefinitionFile modData, Json.ModsRegistryEntry modsRegistryEntry)
        {
            if (!string.IsNullOrEmpty(modsRegistryEntry.GameRegistryId))
                return modsRegistryEntry.GameRegistryId.Equals(modData.Key, StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(modsRegistryEntry.SteamId) && !string.IsNullOrEmpty(modData.RemoteFileId))
            {
                return modsRegistryEntry.SteamId.Equals(modData.RemoteFileId, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(modsRegistryEntry.DisplayName, modData.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Generates an IComparer where the objects will be compared by extracting a single property using the first supplied function, if that returns equal (0) then the next property will be extracted and compared etc.    
        /// </summary>
        /// <remarks>
        /// Based off javas Comparator.comparing(Function).  Why does this not exist in c#?
        /// </remarks>
        public static IComparer<T> Create<T>(params Func<T, IComparable>[] keyFunctions)
        {
            IComparer<T> comparer = new FunctionComparer<T>(keyFunctions[0]);
            if (keyFunctions.Length == 1)
            {
                return comparer;
            }

            for (int i = 1; i < keyFunctions.Length; i++)
            {
                comparer = comparer.ThenComparing(keyFunctions[i]);
            }

            return comparer;
        }

        /// <summary>
        /// Chains 2 comparators, such that the 2nd will be used if the first returns 0.
        /// </summary>
        /// <param name="comparator">The first comparator</param>
        /// <param name="thenComparing">A function to extract the field that will then be compared upon</param>
        /// <remarks>
        /// Based off javas Comparator.thenComparing(Function).  Why does this not exist in c#?
        /// </remarks>
        public static IComparer<T> ThenComparing<T>(this IComparer<T> comparator, Func<T, IComparable> thenComparing)
        {
            return new ChainedComparer<T>(comparator, new FunctionComparer<T>(thenComparing));
        }

        private sealed class ChainedComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> _comp1;
            private readonly IComparer<T> _comp2;

            internal ChainedComparer(IComparer<T> comp1, IComparer<T> comp2)
            {
                this._comp1 = comp1;
                this._comp2 = comp2;
            }
            public int Compare(T x, T y)
            {
                var compare = this._comp1.Compare(x, y);
                return compare == 0 ? this._comp2.Compare(x, y) : compare;
            }
        }

        private sealed class FunctionComparer<T> : IComparer<T>
        {
            private readonly Func<T, IComparable> _func;

            internal FunctionComparer(Func<T, IComparable> func)
            {
                this._func = func;
            }
            public int Compare(T x, T y)
            {
                return this._func(x).CompareTo(this._func(y));
            }
        }

        // This provides a useful extension-like method to find the index of and item from IEnumerable<T>
        // This was based off of the Enumerable.Count<T> extension method.
        /// <summary>
        /// Returns the index of an item in a sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence containing elements.</param>
        /// <param name="item">The item to locate.</param>
        /// <param name="itemComparer">The item equality comparer to use.  Pass null to use the default comparer.</param>
        /// <returns>The index of the entry if it was found in the sequence; otherwise, -1.</returns>
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, TSource item,
	        IEqualityComparer<TSource> itemComparer = null)
        {
          switch (source)
          {
              case null:
                  throw new ArgumentNullException(nameof(source));
              case IList<TSource> listOfT:
                  return listOfT.IndexOf(item);
              case IList list:
                  return list.IndexOf(item);
          }

          if (itemComparer == null)
          {
            itemComparer = EqualityComparer<TSource>.Default;
          }

          var i = 0;
          foreach (var possibleItem in source)
          {
            if (itemComparer.Equals(item, possibleItem))
            {
              return i;
            }
            i++;
          }
          return -1;
        }
    }
}
