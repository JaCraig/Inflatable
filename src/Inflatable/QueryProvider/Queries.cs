﻿/*
Copyright 2017 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using BigBook;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Inflatable.QueryProvider
{
    /// <summary>
    /// Query holder
    /// </summary>
    /// <seealso cref="IQueries"/>
    public class Queries : IQueries
    {
        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count => InternalDictionary.Count;

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<QueryType> Keys => InternalDictionary.Keys;

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<IQuery?> Values => InternalDictionary.Values;

        /// <summary>
        /// Gets or sets the internal dictionary.
        /// </summary>
        /// <value>The internal dictionary.</value>
        private Dictionary<QueryType, IQuery?> InternalDictionary { get; } = [];

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(KeyValuePair<QueryType, IQuery?> item) => InternalDictionary.SetValue(item.Key, item.Value);

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(QueryType key, IQuery? value) => InternalDictionary.SetValue(key, value);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear() => InternalDictionary.Clear();

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.</returns>
        public bool Contains(KeyValuePair<QueryType, IQuery?> item) => InternalDictionary.Contains(item);

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key contains key; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(QueryType key) => InternalDictionary.ContainsKey(key);

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(KeyValuePair<QueryType, IQuery?>[] array, int arrayIndex)
        {
            if (array is null)
                return;
            foreach (var Item in InternalDictionary)
            {
                if (array.Length <= arrayIndex)
                {
                    return;
                }

                array[arrayIndex] = Item;
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator<KeyValuePair<QueryType, IQuery?>> GetEnumerator() => InternalDictionary.GetEnumerator();

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator() => InternalDictionary.GetEnumerator();

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if it is removed, false otherwise.</returns>
        public bool Remove(KeyValuePair<QueryType, IQuery?> item) => InternalDictionary.Remove(item.Key);

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if it is removed, false otherwise.</returns>
        public bool Remove(QueryType key) => InternalDictionary.Remove(key);

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if it is found, false otherwise.</returns>
        public bool TryGetValue(QueryType key, out IQuery? value) => InternalDictionary.TryGetValue(key, out value);

        /// <summary>
        /// Gets or sets the <see cref="IQuery"/> with the specified key.
        /// </summary>
        /// <value>The <see cref="IQuery"/>.</value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public IQuery? this[QueryType key]
        {
            get => InternalDictionary.GetValue(key);

            set => InternalDictionary.SetValue(key, value);
        }
    }
}