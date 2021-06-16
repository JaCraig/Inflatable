/*
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
using DragonHoard.Core;
using DragonHoard.Core.Interfaces;
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.QueryProvider.Interfaces;
using Inflatable.Registration;
using Inflatable.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflatable.QueryProvider
{
    /// <summary>
    /// Query results
    /// </summary>
    public class QueryResults
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResults"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="values">The values.</param>
        /// <param name="session">The session.</param>
        /// <exception cref="ArgumentNullException">query</exception>
        public QueryResults(IQuery query, IEnumerable<Dynamo> values, ISession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Values = values?.ToList() ?? new List<Dynamo>();
            Query = query ?? throw new ArgumentNullException(nameof(query));
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <value>The query.</value>
        public IQuery Query { get; }

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>The session.</value>
        public ISession Session { get; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public List<Dynamo> Values { get; }

        /// <summary>
        /// Gets the cached items as a list.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="results">The results.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="inflatableOptions">The inflatable options.</param>
        public static void CacheValues(string keyName, List<QueryResults> results, ICache? cache, InflatableOptions inflatableOptions)
        {
            var CacheOptions = new CacheEntryOptions { Size = 1, Tags = results.Select(x => x.Query.ReturnType.GetName()).ToArray() };
            if (inflatableOptions.AbsoluteExpirationQueriesKeptInCache != default)
                CacheOptions.AbsoluteExpirationRelativeToNow = inflatableOptions.AbsoluteExpirationQueriesKeptInCache;
            if (inflatableOptions.SlidingExpirationQueriesKeptInCache != default)
                CacheOptions.SlidingExpiration = inflatableOptions.SlidingExpirationQueriesKeptInCache;
            cache?.Set(keyName, results, CacheOptions);
        }

        /// <summary>
        /// Removes the cache tag.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="cache">The cache.</param>
        public static void RemoveCacheTag(string name, ICache? cache) => cache?.RemoveByTag(name);

        /// <summary>
        /// Gets the cached value.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="results">The results.</param>
        /// <returns>The cached value</returns>
        public static bool TryGetCached(string keyName, ICache? cache, out List<QueryResults> results)
        {
            if (cache is null || !cache.TryGetValue(keyName, out List<QueryResults> value))
            {
                results = default;
                return false;
            }
            results = value;
            return true;
        }

        /// <summary>
        /// Determines whether this instance can copy the specified results.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="idProperties">The identifier properties.</param>
        /// <returns>
        /// <c>true</c> if this instance can copy the specified results; otherwise, <c>false</c>.
        /// </returns>
        public bool CanCopy(QueryResults results, IEnumerable<IIDProperty> idProperties) => results != null && idProperties.Any() && results.Query.ReturnType == Query.ReturnType;

        /// <summary>
        /// Converts the values.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns>The resulting list of objects.</returns>
        public IList<TObject> ConvertValues<TObject>()
            where TObject : class => new ObservableList<TObject>(Values.ForEach(x => (TObject)ConvertValue(x)!));

        /// <summary>
        /// Copies the specified return value.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="idProperties">The identifier properties.</param>
        public void Copy(QueryResults results, IEnumerable<IIDProperty> idProperties)
        {
            if (!CanCopy(results, idProperties))
            {
                return;
            }

            for (int i = 0, resultsValuesCount = results.Values.Count; i < resultsValuesCount; i++)
            {
                var Value = results.Values[i];
                var MyValue = Values.Find(x => idProperties.All(y => y.GetColumnInfo()[0].GetValue(x)?.Equals(y.GetColumnInfo()[0].GetValue(Value)) == true));
                if (MyValue != null)
                {
                    Value.CopyTo(MyValue);
                }
            }
        }

        /// <summary>
        /// Copies the specified return value.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="idProperties">The identifier properties.</param>
        public void CopyOrAdd(QueryResults results, IEnumerable<IIDProperty> idProperties)
        {
            if (results is null || results.Query.ReturnType != Query.ReturnType)
            {
                return;
            }

            if (!idProperties.Any())
            {
                Values.Add(results.Values);
                return;
            }
            for (int i = 0, resultsValuesCount = results.Values.Count; i < resultsValuesCount; i++)
            {
                var Value = results.Values[i];
                var MyValue = Values.Find(x => idProperties.All(y => y.GetColumnInfo()[0].GetValue(x)?.Equals(y.GetColumnInfo()[0].GetValue(Value)) == true));
                if (MyValue is null)
                {
                    Values.Add(Value);
                }
                else
                {
                    Value.CopyTo(MyValue);
                }
            }
        }

        /// <summary>
        /// Converts the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The resulting value</returns>
        private object? ConvertValue(Dynamo value)
        {
            if (value is null)
            {
                return null;
            }

            var Value = value.To(Query.ReturnType);
            ((IORMObject)Value).InitializeORMObject0(Session);
            return Value;
        }
    }
}