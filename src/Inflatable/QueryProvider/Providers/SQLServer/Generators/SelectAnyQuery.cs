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
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using System;
using System.Data;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.Generators
{
    /// <summary>
    /// Select any query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="BaseClasses.QueryGeneratorBaseClass{TMappedClass}"/>
    public class SelectAnyQuery<TMappedClass> : QueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectAnyQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">Mapping information</param>
        public SelectAnyQuery(MappingSource mappingInformation)
            : base(mappingInformation)
        {
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.Any;

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <returns>The resulting query</returns>
        public override IQuery GenerateQuery()
        {
            var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
            return new Query(CommandType.Text, GenerateSelectQuery(TypeGraph.Root), QueryType);
        }

        private string GenerateFromClause(Utils.TreeNode<Type> node)
        {
            StringBuilder Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            foreach (var ParentNode in node.Nodes)
            {
                var ParentMapping = MappingInformation.Mappings[ParentNode.Data];
                StringBuilder IDProperties = new StringBuilder();
                string Separator = "";
                foreach (var IDProperty in ParentMapping.IDProperties)
                {
                    IDProperties.AppendFormat("{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty), GetColumnName(IDProperty));
                    Separator = " AND ";
                }
                foreach (var IDProperty in ParentMapping.AutoIDProperties)
                {
                    IDProperties.AppendFormat("{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty), GetColumnName(IDProperty));
                    Separator = " AND ";
                }
                Result.AppendLine();
                Result.AppendFormat("INNER JOIN {0} ON {1}", GetTableName(ParentMapping), IDProperties);
                Result.Append(GenerateFromClause(ParentNode));
            }
            return Result.ToString();
        }

        private string GenerateParameterList(Utils.TreeNode<Type> node)
        {
            StringBuilder Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            string Separator = "";
            foreach (var ParentNode in node.Nodes)
            {
                var ParentResult = GenerateParameterList(ParentNode);
                if (!string.IsNullOrEmpty(ParentResult))
                {
                    Result.Append(Separator + ParentResult);
                    Separator = ",";
                }
            }
            foreach (var IDProperty in Mapping.IDProperties)
            {
                Result.AppendFormat("{0}{1} AS {2}", Separator, GetColumnName(IDProperty), "[" + IDProperty.Name + "]");
                Separator = ",";
            }
            foreach (var ReferenceProperty in Mapping.ReferenceProperties)
            {
                Result.AppendFormat("{0}{1} AS {2}", Separator, GetColumnName(ReferenceProperty), "[" + ReferenceProperty.Name + "]");
                Separator = ",";
            }
            return Result.ToString();
        }

        private string GenerateSelectQuery(Utils.TreeNode<Type> node)
        {
            StringBuilder Builder = new StringBuilder();
            StringBuilder ParameterList = new StringBuilder();
            StringBuilder FromClause = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];

            //Get From Clause
            FromClause.Append(GetTableName(Mapping));
            FromClause.Append(GenerateFromClause(node));

            //Get parameter listing
            ParameterList.Append(GenerateParameterList(node));

            //Generate final query
            Builder.AppendFormat(@"SELECT TOP 1 {0}
FROM {1};", ParameterList, FromClause);
            return Builder.ToString();
        }
    }
}