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
using Inflatable.LinqExpression;
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
        /// <exception cref="ArgumentNullException">
        /// linqQueryGenerator or mappingInformation or queryGenerators
        /// </exception>
        /// <exception cref="ArgumentException">Mapping not found for type: AssociatedType</exception>
        protected GeneratorBaseClass(MappingSource mappingInformation,
            IEnumerable<IQueryGenerator<TMappedClass>> queryGenerators)
        {
            MappingInformation = mappingInformation ?? throw new ArgumentNullException(nameof(mappingInformation));
            if (!MappingInformation.Mappings.ContainsKey(AssociatedType))
                throw new ArgumentException("Mapping not found for type: " + AssociatedType);
            queryGenerators = queryGenerators ?? throw new ArgumentNullException(nameof(queryGenerators));
            QueryGenerators = queryGenerators.ToDictionary(x => x.QueryType);
            LinqQueryGenerator = (ILinqQueryGenerator<TMappedClass>)queryGenerators.FirstOrDefault(x => x.QueryType == QueryType.LinqQuery);
        }

        /// <summary>
        /// Gets the type of the associated.
        /// </summary>
        /// <value>The type of the associated.</value>
        public Type AssociatedType => typeof(TMappedClass);

        /// <summary>
        /// Gets the linq query generator.
        /// </summary>
        /// <value>The linq query generator.</value>
        public ILinqQueryGenerator<TMappedClass> LinqQueryGenerator { get; }

        /// <summary>
        /// Gets the mapping information.
        /// </summary>
        /// <value>The mapping information.</value>
        public MappingSource MappingInformation { get; }

        /// <summary>
        /// Gets the query generators.
        /// </summary>
        /// <value>The query generators.</value>
        public IDictionary<QueryType, IQueryGenerator<TMappedClass>> QueryGenerators { get; }

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The resulting declarations.</returns>
        public IQuery GenerateDeclarations(QueryType type)
        {
            return QueryGenerators[type].GenerateDeclarations();
        }

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The resulting query</returns>
        public IQuery GenerateQuery(QueryData<TMappedClass> data)
        {
            return LinqQueryGenerator.GenerateQuery(data);
        }

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The resulting queries.</returns>
        public IQuery GenerateQuery(QueryType type, TMappedClass queryObject)
        {
            return QueryGenerators[type].GenerateQuery(queryObject);
        }

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="queryObject">The query object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The resulting query</returns>
        public IQuery GenerateQuery(QueryType type, TMappedClass queryObject, string propertyName)
        {
            return ((IPropertyQueryGenerator<TMappedClass>)QueryGenerators[type]).GenerateQuery(queryObject, propertyName);
        }
    }
}