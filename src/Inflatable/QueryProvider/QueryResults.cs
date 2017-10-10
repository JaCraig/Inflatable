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
using BigBook.Caching.Interfaces;
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.QueryProvider.Interfaces;
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
        public QueryResults(IQuery query, IEnumerable<Dynamo> values, Session session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Values = (values ?? new List<Dynamo>()).ToList();
            Query = query ?? throw new ArgumentNullException(nameof(query));
        }

        /// <summary>
        /// Gets the cache manager.
        /// </summary>
        /// <value>The cache manager.</value>
        private static ICache Cache => Canister.Builder.Bootstrapper.Resolve<BigBook.Caching.Manager>().Cache();

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <value>The query.</value>
        public IQuery Query { get; }

        /// <summary>
        /// Gets the session.
        /// </summary>
        /// <value>The session.</value>
        public Session Session { get; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public IList<Dynamo> Values { get; }

        /// <summary>
        /// Gets the cached items as a list.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="results">The results.</param>
        public static void CacheValues(string keyName, List<QueryResults> results)
        {
            Cache.Add(keyName, results, results.Select(x => x.Query.ReturnType.GetName()).ToArray());
        }

        /// <summary>
        /// Gets the cached value.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <returns>The cached value</returns>
        public static List<QueryResults> GetCached(string keyName)
        {
            return (List<QueryResults>)Cache[keyName];
        }

        /// <summary>
        /// Determines whether the specified key name is cached.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <returns><c>true</c> if the specified key name is cached; otherwise, <c>false</c>.</returns>
        public static bool IsCached(string keyName)
        {
            return Cache.ContainsKey(keyName);
        }

        /// <summary>
        /// Removes the cache tag.
        /// </summary>
        /// <param name="name">The name.</param>
        public static void RemoveCacheTag(string name)
        {
            Cache.RemoveByTag(name);
        }

        /// <summary>
        /// Determines whether this instance can copy the specified results.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="idProperties">The identifier properties.</param>
        /// <returns>
        /// <c>true</c> if this instance can copy the specified results; otherwise, <c>false</c>.
        /// </returns>
        public bool CanCopy(QueryResults results, IEnumerable<IIDProperty> idProperties)
        {
            return results != null && idProperties.Any() && results.Query.ReturnType == Query.ReturnType;
        }

        /// <summary>
        /// Converts the values.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns>The resulting list of objects.</returns>
        public IList<TObject> ConvertValues<TObject>()
            where TObject : class
        {
            return new ObservableList<TObject>(Values.ForEachParallel(x => (TObject)ConvertValue(x)));
        }

        /// <summary>
        /// Copies the specified return value.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="idProperties">The identifier properties.</param>
        public void Copy(QueryResults results, IEnumerable<IIDProperty> idProperties)
        {
            if (!CanCopy(results, idProperties))
                return;
            foreach (var Value in results.Values)
            {
                var MyValue = Values.FirstOrDefault(x => idProperties.All(y => y.GetColumnInfo()[0].GetValue(x).Equals(y.GetColumnInfo()[0].GetValue(Value))));
                if (MyValue != null)
                    Value.CopyTo(MyValue);
            }
        }

        /// <summary>
        /// Copies the specified return value.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="idProperties">The identifier properties.</param>
        public void CopyOrAdd(QueryResults results, IEnumerable<IIDProperty> idProperties)
        {
            if (results == null || results.Query.ReturnType != Query.ReturnType)
                return;
            if (!idProperties.Any())
            {
                Values.Add(results.Values);
                return;
            }
            foreach (var Value in results.Values)
            {
                var MyValue = Values.FirstOrDefault(x => idProperties.All(y => y.GetColumnInfo()[0].GetValue(x).Equals(y.GetColumnInfo()[0].GetValue(Value))));
                if (MyValue == null)
                    Values.Add(Value);
                else
                    Value.CopyTo(MyValue);
            }
        }

        /// <summary>
        /// Converts the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The resulting value</returns>
        private object ConvertValue(Dynamo value)
        {
            if (value == null)
                return null;
            var Value = value.To(Query.ReturnType);
            ((IORMObject)Value).Session0 = Session;
            return Value;
        }
    }
}