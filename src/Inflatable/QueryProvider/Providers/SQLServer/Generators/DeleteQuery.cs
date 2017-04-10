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
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using System.Data;
using System.Linq;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.Generators
{
    /// <summary>
    /// Delete query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="BaseClasses.QueryGeneratorBaseClass{TMappedClass}"/>
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
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.Delete;

        /// <summary>
        /// Generates a delete query.
        /// </summary>
        /// <returns>The resulting query</returns>
        public override IQuery GenerateQuery()
        {
            var ParentTypes = MappingInformation.ParentTypes[AssociatedType];
            var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
            StringBuilder Builder = new StringBuilder();
            foreach (var ParentMapping in ParentTypes.ForEach(ParentType => MappingInformation.Mappings[ParentType]).OrderBy(x => x.Order))
            {
                if (ParentMapping.IDProperties.Count > 0)
                {
                    Builder.AppendLineFormat("DELETE FROM {0} WHERE {1};",
                        GetTableName(ParentMapping),
                        ParentMapping.IDProperties.ToString(x => GetColumnName(x) + "=" + GetParameterName(x), " AND "));
                }
            }
            var Mapping = MappingInformation.Mappings[AssociatedType];
            if (Mapping.IDProperties.Count > 0)
            {
                Builder.AppendLineFormat("DELETE FROM {0} WHERE {1};",
                        GetTableName(Mapping),
                        Mapping.IDProperties.ToString(x => GetColumnName(x) + "=" + GetParameterName(x), " AND "));
            }
            return new Query(CommandType.Text, Builder.ToString(), QueryType.Delete);
        }
    }
}