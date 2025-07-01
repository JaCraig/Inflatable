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
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider.Interfaces;
using Microsoft.Extensions.Logging;
using SQLHelperDB;
using System;
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
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">providers</exception>
        public QueryProviderManager(IEnumerable<Interfaces.IQueryProvider> providers, ILogger<QueryProviderManager>? logger = null)
        {
            Logger = logger;
            IsDebug = Logger?.IsEnabled(LogLevel.Debug) ?? false;
            providers ??= [];
            foreach (var QueryProvider in providers.Where(x => x.GetType().Assembly != typeof(QueryProviderManager).Assembly))
            {
                foreach (var Provider in QueryProvider.Providers)
                {
                    Providers.TryAdd(Provider, QueryProvider);
                }
            }
            foreach (var QueryProvider in providers.Where(x => x.GetType().Assembly == typeof(QueryProviderManager).Assembly))
            {
                foreach (var Provider in QueryProvider.Providers)
                {
                    Providers.TryAdd(Provider, QueryProvider);
                }
            }
            CreateGeneratorMethod = typeof(QueryProviderManager).GetMethod("CreateGenerator", [typeof(IMappingSource)]);
        }

        /// <summary>
        /// The lock object
        /// </summary>
        private readonly object _LockObject = new();

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger? Logger { get; }

        /// <summary>
        /// Gets the providers.
        /// </summary>
        /// <value>The providers.</value>
        public Dictionary<DbProviderFactory, Interfaces.IQueryProvider> Providers { get; } = [];

        /// <summary>
        /// Gets the create generator method.
        /// </summary>
        /// <value>The create generator method.</value>
        private MethodInfo? CreateGeneratorMethod { get; }

        /// <summary>
        /// Gets the generic create generator methods.
        /// </summary>
        /// <value>The generic create generator methods.</value>
        private Dictionary<int, MethodInfo> GenericCreateGeneratorMethods { get; } = [];

        /// <summary>
        /// Gets a value indicating whether debug level logging is turned on.
        /// </summary>
        /// <value><c>true</c> if debug level logging is turned on; otherwise, <c>false</c>.</value>
        private bool IsDebug { get; }

        /// <summary>
        /// Creates a batch.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>Creates a batch</returns>
        /// <exception cref="ArgumentNullException">source</exception>
        /// <exception cref="ArgumentException">Provider not found</exception>
        public SQLHelper CreateBatch(IDatabase source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!Providers.TryGetValue(source.Provider, out var QueryProvider))
            {
                throw new ArgumentException("Provider not found: " + source.Provider);
            }

            if (IsDebug)
            {
                Logger?.LogDebug("Creating batch for data source {sourceName}", source.Name);
            }

            return QueryProvider.Batch(source);
        }

        /// <summary>
        /// Creates a query generator.
        /// </summary>
        /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
        /// <param name="mappingInfo">The mapping information.</param>
        /// <returns>The requested query generator</returns>
        /// <exception cref="ArgumentNullException">mappingInfo</exception>
        /// <exception cref="ArgumentException">Provider not found</exception>
        public IGenerator<TMappedClass>? CreateGenerator<TMappedClass>(IMappingSource mappingInfo)
            where TMappedClass : class
        {
            if (mappingInfo?.GetChildMappings<TMappedClass>().Any() != true)
                return null;

            var Provider = mappingInfo.Source.Provider;
            if (!Providers.TryGetValue(Provider, out var QueryProvider))
            {
                throw new ArgumentException("Provider not found: " + Provider);
            }

            if (IsDebug)
            {
                Logger?.LogDebug("Creating generator for type {typeName} in {sourceName}", typeof(TMappedClass).GetName(), mappingInfo.Source.Name);
            }

            return QueryProvider.CreateGenerator<TMappedClass>(mappingInfo);
        }

        /// <summary>
        /// Creates a query generator.
        /// </summary>
        /// <param name="type">The type of the mapped class..</param>
        /// <param name="mappingInfo">The mapping information.</param>
        /// <returns>The requested query generator.</returns>
        public IGenerator? CreateGenerator(Type? type, IMappingSource mappingInfo)
        {
            if (type?.Namespace?.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal) ?? false)
            {
                type = type.BaseType;
            }
            if (type is null)
                return null;
            var HashCode = type.GetHashCode();
            if (!GenericCreateGeneratorMethods.TryGetValue(HashCode, out var Method))
            {
                lock (_LockObject)
                {
                    if (!GenericCreateGeneratorMethods.TryGetValue(HashCode, out Method))
                    {
                        Method = CreateGeneratorMethod?.MakeGenericMethod(type);
                        if (Method is not null)
                            GenericCreateGeneratorMethods.Add(HashCode, Method);
                    }
                }
            }

            return (IGenerator?)Method?.Invoke(this, [mappingInfo]);
        }
    }
}