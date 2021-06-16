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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using SQLHelperDB;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer
{
    /// <summary>
    /// SQL Server query provider
    /// </summary>
    /// <seealso cref="IQueryProvider"/>
    public class SQLServerQueryProvider : IQueryProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SQLServerQueryProvider"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="stringBuilderPool">The string builder pool.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">configuration</exception>
        public SQLServerQueryProvider(IConfiguration configuration, ObjectPool<StringBuilder>? stringBuilderPool, ILogger<SQLHelper> logger = null)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            StringBuilderPool = stringBuilderPool;
            Logger = logger;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Provider name associated with the query provider
        /// </summary>
        public DbProviderFactory Provider => SqlClientFactory.Instance;

        /// <summary>
        /// Gets or sets the dictionary.
        /// </summary>
        /// <value>The dictionary.</value>
        private Dictionary<Tuple<Type, IMappingSource>, IGenerator> CachedResults { get; } = new Dictionary<Tuple<Type, IMappingSource>, IGenerator>();

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger<SQLHelper> Logger { get; }

        /// <summary>
        /// Gets the string builder pool.
        /// </summary>
        /// <value>The string builder pool.</value>
        private ObjectPool<StringBuilder>? StringBuilderPool { get; }

        /// <summary>
        /// The lock object
        /// </summary>
        private readonly object LockObject = new object();

        /// <summary>
        /// Creates a batch for running commands
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>A batch object</returns>
        public SQLHelper Batch(IDatabase source) => new SQLHelper(StringBuilderPool!, Configuration, Logger).CreateBatch(Provider, source?.Name ?? string.Empty);

        /// <summary>
        /// Creates a generator object
        /// </summary>
        /// <typeparam name="TMappedClass">Class type to create the generator for</typeparam>
        /// <param name="mappingInformation">The mapping information.</param>
        /// <returns>Generator object</returns>
        public IGenerator<TMappedClass> CreateGenerator<TMappedClass>(IMappingSource mappingInformation)
            where TMappedClass : class
        {
            var Key = new Tuple<Type, IMappingSource>(typeof(TMappedClass), mappingInformation);
            if (CachedResults.TryGetValue(Key, out var ReturnValue))
                return (IGenerator<TMappedClass>)ReturnValue;
            lock (LockObject)
            {
                if (CachedResults.TryGetValue(Key, out ReturnValue))
                    return (IGenerator<TMappedClass>)ReturnValue;
                ReturnValue = new SQLServerGenerator<TMappedClass>(mappingInformation, StringBuilderPool!);
                CachedResults.Add(Key, ReturnValue);
            }
            return (IGenerator<TMappedClass>)ReturnValue;
        }
    }
}