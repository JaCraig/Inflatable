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
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators
{
    /// <summary>
    /// Update query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="BaseClasses.QueryGeneratorBaseClass{TMappedClass}"/>
    public class UpdateQuery<TMappedClass> : QueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        public UpdateQuery(MappingSource mappingInformation)
            : base(mappingInformation)
        {
            var ParentMappings = MappingInformation.GetParentMapping(typeof(TMappedClass));
            IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
            ReferenceProperties = ParentMappings.SelectMany(x => x.ReferenceProperties);
            MapProperties = ParentMappings.SelectMany(x => x.MapProperties);
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.Update;

        /// <summary>
        /// Gets or sets the identifier properties.
        /// </summary>
        /// <value>The identifier properties.</value>
        private IEnumerable<IIDProperty> IDProperties { get; set; }

        /// <summary>
        /// Gets or sets the map properties.
        /// </summary>
        /// <value>The map properties.</value>
        private IEnumerable<IMapProperty> MapProperties { get; set; }

        /// <summary>
        /// Gets or sets the reference properties.
        /// </summary>
        /// <value>The reference properties.</value>
        private IEnumerable<IProperty> ReferenceProperties { get; set; }

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <returns>The resulting declarations.</returns>
        public override IQuery GenerateDeclarations()
        {
            return new Query(CommandType.Text, "", QueryType);
        }

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="queryObject">The object to generate the queries from.</param>
        /// <returns>The resulting query</returns>
        public override IQuery GenerateQuery(TMappedClass queryObject)
        {
            var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
            return new Query(CommandType.Text, GenerateUpdateQuery(TypeGraph.Root, queryObject), QueryType, GenerateParameters(queryObject));
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The from clause</returns>
        private string GenerateFromClause(Utils.TreeNode<Type> node, TMappedClass queryObject)
        {
            StringBuilder Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            foreach (var ParentNode in node.Nodes)
            {
                var ParentMapping = MappingInformation.Mappings[ParentNode.Data];
                StringBuilder TempIDProperties = new StringBuilder();
                string Separator = "";
                foreach (var IDProperty in ParentMapping.IDProperties)
                {
                    TempIDProperties.AppendFormat("{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty), GetColumnName(IDProperty));
                    Separator = " AND ";
                }
                foreach (var IDProperty in ParentMapping.AutoIDProperties)
                {
                    TempIDProperties.AppendFormat("{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty), GetColumnName(IDProperty));
                    Separator = " AND ";
                }
                Result.AppendLineFormat("INNER JOIN {0} ON {1}", GetTableName(ParentMapping), TempIDProperties);
                Result.Append(GenerateFromClause(ParentNode, queryObject));
            }
            return Result.ToString();
        }

        /// <summary>
        /// Generates the parameters.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The parameters.</returns>
        private IParameter[] GenerateParameters(TMappedClass queryObject)
        {
            var Parameters = IDProperties.ForEach(y => y.GetAsParameter(queryObject)).ToList();
            Parameters.AddRange(ReferenceProperties.ForEach(y => y.GetAsParameter(queryObject)));
            Parameters.AddRange(MapProperties.ForEach(y => y.GetAsParameter(queryObject)).SelectMany(x => x));
            return Parameters.ToArray();
        }

        /// <summary>
        /// Generates the update query.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns></returns>
        private string GenerateUpdateQuery(Utils.TreeNode<Type> node, TMappedClass queryObject)
        {
            var Mapping = MappingInformation.Mappings[node.Data];
            if (Mapping.ReferenceProperties.Count == 0)
                return "";
            StringBuilder Builder = new StringBuilder();
            StringBuilder ParameterList = new StringBuilder();
            StringBuilder WhereClause = new StringBuilder();
            StringBuilder FromClause = new StringBuilder();
            string Splitter = "";

            //Generate parent queries
            foreach (var Parent in node.Nodes)
            {
                var Result = GenerateUpdateQuery(Parent, queryObject);
                if (!string.IsNullOrEmpty(Result))
                {
                    Builder.AppendLine(Result);
                }
            }

            //Adding reference properties
            foreach (var ReferenceProperty in Mapping.ReferenceProperties)
            {
                ParameterList.Append(Splitter + GetColumnName(ReferenceProperty) + "=" + GetParameterName(ReferenceProperty));
                Splitter = ",";
            }

            //Map properties
            foreach (var MapProperty in Mapping.MapProperties)
            {
                ParameterList.Append(Splitter + GetColumnName(MapProperty) + "=" + GetParameterName(MapProperty));
                Splitter = ",";
            }

            //From clause generation
            FromClause.AppendLine(GetTableName(Mapping));
            FromClause.Append(GenerateFromClause(node, queryObject));

            //Where clause generation
            WhereClause.Append(GenerateWhereClause(node, queryObject));

            //Generating final query
            Builder.AppendLineFormat(@"UPDATE {0}
SET {1}
FROM {2}WHERE {3};", GetTableName(Mapping), ParameterList, FromClause, WhereClause);

            return Builder.ToString();
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The where clause</returns>
        private string GenerateWhereClause(Utils.TreeNode<Type> node, TMappedClass queryObject)
        {
            StringBuilder Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            string Separator = "";
            foreach (var ParentNode in node.Nodes)
            {
                var ParentResult = GenerateWhereClause(ParentNode, queryObject);
                if (!string.IsNullOrEmpty(ParentResult))
                {
                    Result.Append(Separator + ParentResult);
                    Separator = "\r\nAND ";
                }
            }
            foreach (var IDProperty in Mapping.IDProperties)
            {
                Result.AppendFormat("{0}{1}={2}", Separator, GetColumnName(IDProperty), GetParameterName(IDProperty));
                Separator = "\r\nAND ";
            }
            return Result.ToString();
        }
    }
}