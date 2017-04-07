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

using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Inflatable.QueryProvider
{
    /// <summary>
    /// Query provider manager
    /// </summary>
    public class QueryProviderManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryProviderManager"/> class.
        /// </summary>
        /// <param name="providers">The providers.</param>
        /// <param name="mappingInfo">The mapping information.</param>
        /// <exception cref="System.ArgumentNullException">mappingInfo or providers</exception>
        public QueryProviderManager(IEnumerable<Interfaces.IQueryProvider> providers, MappingSource mappingInfo)
        {
            MappingInfo = mappingInfo ?? throw new ArgumentNullException(nameof(mappingInfo));
            providers = providers ?? throw new ArgumentNullException(nameof(providers));
            Providers = new ConcurrentDictionary<DbProviderFactory, Interfaces.IQueryProvider>();
            foreach (var Provider in providers.Where(x => !x.GetType().GetTypeInfo().Assembly.FullName.ToUpper().Contains("INFLATABLE")))
            {
                Providers.Add(Provider.Provider, Provider);
            }
            foreach (var Provider in providers.Where(x => x.GetType().GetTypeInfo().Assembly.FullName.ToUpper().Contains("INFLATABLE")))
            {
                if (!Providers.Keys.Contains(Provider.Provider))
                {
                    Providers.Add(Provider.Provider, Provider);
                }
            }
        }

        /// <summary>
        /// Gets the mapping information.
        /// </summary>
        /// <value>The mapping information.</value>
        public MappingSource MappingInfo { get; }

        /// <summary>
        /// Gets the providers.
        /// </summary>
        /// <value>The providers.</value>
        public IDictionary<DbProviderFactory, Interfaces.IQueryProvider> Providers { get; }

        /// <summary>
        /// Creates a batch.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Creates a batch</returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        /// <exception cref="System.ArgumentException">Provider not found</exception>
        public SQLHelper.SQLHelper CreateBatch(IDatabase source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!Providers.TryGetValue(source.Provider, out Interfaces.IQueryProvider QueryProvider))
                throw new ArgumentException("Provider not found: " + source.Provider);
            return QueryProvider.Batch(source);
        }

        /// <summary>
        /// Creates a query generator.
        /// </summary>
        /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
        /// <param name="provider">The provider.</param>
        /// <returns>The requested query generator</returns>
        /// <exception cref="System.ArgumentNullException">provider</exception>
        /// <exception cref="System.ArgumentException">Provider not found: provider name</exception>
        public IGenerator<TMappedClass> CreateGenerator<TMappedClass>(DbProviderFactory provider)
            where TMappedClass : class
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            if (!Providers.TryGetValue(provider, out Interfaces.IQueryProvider QueryProvider))
                throw new ArgumentException("Provider not found: " + provider);
            return QueryProvider.CreateGenerator<TMappedClass>(MappingInfo);
        }
    }
}