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
using Inflatable.Interfaces;
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using SQLHelperDB.HelperClasses.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators
{
    /// <summary>
    /// Delete query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="QueryGeneratorBaseClass{TMappedClass}"/>
    public class DeleteQuery<TMappedClass> : QueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        public DeleteQuery(MappingSource mappingInformation)
            : base(mappingInformation)
        {
            ParentMappings = MappingInformation.GetChildMappings<TMappedClass>()
                                                .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                                .Distinct();
            IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
            GenerateQuery();
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.Delete;

        /// <summary>
        /// Gets or sets the identifier properties.
        /// </summary>
        /// <value>The identifier properties.</value>
        private IEnumerable<IIDProperty> IDProperties { get; }

        /// <summary>
        /// Gets or sets the parent mappings.
        /// </summary>
        /// <value>The parent mappings.</value>
        private IEnumerable<IMapping> ParentMappings { get; }

        /// <summary>
        /// Gets or sets the query text.
        /// </summary>
        /// <value>The query text.</value>
        private string? QueryText { get; set; }

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <returns>The resulting declarations.</returns>
        public override IQuery[] GenerateDeclarations() => new IQuery[] { new Query(typeof(object), CommandType.Text, "", QueryType) };

        /// <summary>
        /// Generates a delete query.
        /// </summary>
        /// <param name="queryObject">The object to generate the queries from.</param>
        /// <returns>The resulting query</returns>
        public override IQuery[] GenerateQueries(TMappedClass queryObject) => new IQuery[] { new Query(AssociatedType, CommandType.Text, QueryText ?? "", QueryType, GenerateParameters(queryObject)) };

        /// <summary>
        /// Generates the parameters.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The parameters</returns>
        private IParameter?[] GenerateParameters(TMappedClass queryObject) => IDProperties.SelectMany(x => x.GetColumnInfo().Select(y => y.GetAsParameter(queryObject))).ToArray();

        /// <summary>
        /// Generates the query.
        /// </summary>
        private void GenerateQuery()
        {
            var Builder = new StringBuilder();
            foreach (var ParentMapping in ParentMappings.Where(x => x.IDProperties.Count > 0).OrderBy(x => x.Order))
            {
                Builder.AppendLineFormat("DELETE FROM {0} WHERE {1};",
                    GetTableName(ParentMapping),
                    ParentMapping.IDProperties.ToString(x => GetColumnName(x) + "=" + GetParameterName(x), " AND "));
            }
            QueryText = Builder.ToString();
        }
    }
}