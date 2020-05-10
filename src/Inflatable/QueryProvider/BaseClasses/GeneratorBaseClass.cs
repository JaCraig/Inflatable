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
using Inflatable.ClassMapper.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.LinqExpression.Interfaces;
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
    /// <seealso cref="IGenerator{TMappedClass}"/>
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
        protected GeneratorBaseClass(IMappingSource mappingInformation,
            IEnumerable<IQueryGenerator<TMappedClass>> queryGenerators)
        {
            MappingInformation = mappingInformation ?? throw new ArgumentNullException(nameof(mappingInformation));
            if (!MappingInformation.GetChildMappings(AssociatedType).Any())
            {
                throw new ArgumentException("Mapping not found for type: " + AssociatedType);
            }

            QueryGenerators = queryGenerators?.ToDictionary(x => x.QueryType) ?? throw new ArgumentNullException(nameof(queryGenerators));
            LinqQueryGenerator = (ILinqQueryGenerator<TMappedClass>)queryGenerators.FirstOrDefault(x => x.QueryType == QueryType.LinqQuery);
            DataQueryGenerator = (IDataQueryGenerator<TMappedClass>)queryGenerators.FirstOrDefault(x => x.QueryType == QueryType.LoadData);
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
        public IMappingSource MappingInformation { get; }

        /// <summary>
        /// Gets the query generators.
        /// </summary>
        /// <value>The query generators.</value>
        public IDictionary<QueryType, IQueryGenerator<TMappedClass>> QueryGenerators { get; }

        /// <summary>
        /// Gets the data query generator.
        /// </summary>
        /// <value>The data query generator.</value>
        private IDataQueryGenerator<TMappedClass> DataQueryGenerator { get; }

        /// <summary>
        /// Gets the linq query generator.
        /// </summary>
        /// <value>The linq query generator.</value>
        private ILinqQueryGenerator<TMappedClass> LinqQueryGenerator { get; }

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The resulting declarations.</returns>
        public IQuery[] GenerateDeclarations(QueryType type) => QueryGenerators[type].GenerateDeclarations();

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The resulting query</returns>
        public IQuery[] GenerateQueries(IQueryData data) => LinqQueryGenerator.GenerateQueries((QueryData<TMappedClass>)data);

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The resulting queries.</returns>
        public IQuery[] GenerateQueries(QueryType type, object queryObject) => QueryGenerators[type].GenerateQueries((TMappedClass)queryObject);

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="queryObject">The query object.</param>
        /// <param name="property">The property.</param>
        /// <returns>The resulting query</returns>
        public IQuery[] GenerateQueries(QueryType type, object queryObject, IClassProperty property) => ((IPropertyQueryGenerator<TMappedClass>)QueryGenerators[type]).GenerateQueries((TMappedClass)queryObject, property);

        /// <summary>
        /// Generates the queries.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="ids">The ids.</param>
        /// <returns>The resulting query</returns>
        public IQuery[] GenerateQueries(QueryType type, Dynamo[] ids) => DataQueryGenerator.GenerateQueries(ids);
    }
}