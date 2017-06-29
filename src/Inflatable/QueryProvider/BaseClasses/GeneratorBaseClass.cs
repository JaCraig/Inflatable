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
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflatable.QueryProvider.BaseClasses
{
    /// <summary>
    /// Generator base class
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="Interfaces.IGenerator{TMappedClass}"/>
    public abstract class GeneratorBaseClass<TMappedClass> : IGenerator<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorBaseClass{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        /// <param name="queryGenerators">The query generators.</param>
        protected GeneratorBaseClass(MappingSource mappingInformation, IEnumerable<IQueryGenerator> queryGenerators)
        {
            MappingInformation = mappingInformation ?? throw new ArgumentNullException(nameof(mappingInformation));
            if (!MappingInformation.Mappings.ContainsKey(AssociatedType))
                throw new ArgumentException("Mapping not found for type: " + AssociatedType);
            queryGenerators = queryGenerators ?? throw new ArgumentNullException(nameof(queryGenerators));
            QueryGenerators = queryGenerators.ToDictionary(x => x.QueryType);
        }

        /// <summary>
        /// Gets the type of the associated.
        /// </summary>
        /// <value>The type of the associated.</value>
        public Type AssociatedType => typeof(TMappedClass);

        /// <summary>
        /// Gets the mapping information.
        /// </summary>
        /// <value>The mapping information.</value>
        public MappingSource MappingInformation { get; }

        /// <summary>
        /// Gets the query generators.
        /// </summary>
        /// <value>The query generators.</value>
        public IDictionary<QueryType, IQueryGenerator> QueryGenerators { get; }

        /// <summary>
        /// Generates the default queries associated with the mapped type.
        /// </summary>
        /// <returns>The default queries for the specified type.</returns>
        public Queries GenerateDefaultQueries()
        {
            if (!MappingInformation.ParentTypes.Keys.Contains(AssociatedType))
                return new Queries();
            var Result = new Queries();
            foreach (var QueryGenerator in QueryGenerators)
            {
                Result.Add(QueryGenerator.Key, QueryGenerator.Value.GenerateQuery());
            }
            return Result;
        }
    }
}