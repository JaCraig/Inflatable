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
using Inflatable.QueryProvider;
using Inflatable.Schema;
using System;

namespace Inflatable.Sessions
{
    /// <summary>
    /// Session manager
    /// </summary>
    public class SessionManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionManager"/> class.
        /// </summary>
        /// <param name="mappingManager">The mapping manager.</param>
        /// <param name="schemaManager">The schema manager.</param>
        /// <param name="queryProviderManager">The query provider manager.</param>
        /// <exception cref="System.ArgumentNullException">
        /// queryProviderManager or schemaManager or mappingManager
        /// </exception>
        public SessionManager(MappingManager mappingManager, SchemaManager schemaManager, QueryProviderManager queryProviderManager)
        {
            QueryProviderManager = queryProviderManager ?? throw new ArgumentNullException(nameof(queryProviderManager));
            SchemaManager = schemaManager ?? throw new ArgumentNullException(nameof(schemaManager));
            MappingManager = mappingManager ?? throw new ArgumentNullException(nameof(mappingManager));
        }

        /// <summary>
        /// Gets the mapping manager.
        /// </summary>
        /// <value>The mapping manager.</value>
        public MappingManager MappingManager { get; }

        /// <summary>
        /// Gets the query provider manager.
        /// </summary>
        /// <value>The query provider manager.</value>
        public QueryProviderManager QueryProviderManager { get; }

        /// <summary>
        /// Gets the schema manager.
        /// </summary>
        /// <value>The schema manager.</value>
        public SchemaManager SchemaManager { get; }
    }
}