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
using Inflatable.Aspect.Interfaces;
using Inflatable.ClassMapper;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Microsoft.Extensions.ObjectPool;
using SQLHelperDB.HelperClasses.Interfaces;
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
    /// <seealso cref="QueryGeneratorBaseClass{TMappedClass}"/>
    public class UpdateQuery<TMappedClass> : QueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        /// <param name="objectPool">The object pool.</param>
        public UpdateQuery(IMappingSource mappingInformation, ObjectPool<StringBuilder> objectPool)
            : base(mappingInformation, objectPool)
        {
            var ParentMappings = MappingInformation.GetChildMappings(typeof(TMappedClass))
                                                   .SelectMany(x => mappingInformation.GetParentMapping(x.ObjectType))
                                                   .Distinct();
            IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
            ReferenceProperties = ParentMappings.SelectMany(x => x.ReferenceProperties);
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
        private IEnumerable<IIDProperty> IDProperties { get; }

        /// <summary>
        /// Gets or sets the reference properties.
        /// </summary>
        /// <value>The reference properties.</value>
        private IEnumerable<IProperty> ReferenceProperties { get; }

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <returns>The resulting declarations.</returns>
        public override IQuery[] GenerateDeclarations() => new IQuery[] { new Query(AssociatedType, CommandType.Text, string.Empty, QueryType) };

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="queryObject">The object to generate the queries from.</param>
        /// <returns>The resulting query</returns>
        public override IQuery[] GenerateQueries(TMappedClass queryObject)
        {
            var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
            return new[] { new Query(AssociatedType, CommandType.Text, GenerateUpdateQuery(TypeGraph?.Root, queryObject), QueryType, GenerateParameters(queryObject)) };
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The from clause</returns>
        private string GenerateFromClause(Utils.TreeNode<Type> node, TMappedClass queryObject)
        {
            var Result = ObjectPool.Get();
            var Mapping = MappingInformation.Mappings[node.Data];
            for (int x = 0, nodeNodesCount = node.Nodes.Count; x < nodeNodesCount; x++)
            {
                var ParentNode = node.Nodes[x];
                var ParentMapping = MappingInformation.Mappings[ParentNode.Data];
                var TempIDProperties = ObjectPool.Get();
                var Separator = string.Empty;
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
                Result.AppendLineFormat("INNER JOIN {0} ON {1}", GetTableName(ParentMapping), TempIDProperties)
                    .Append(GenerateFromClause(ParentNode, queryObject));
                ObjectPool.Return(TempIDProperties);
            }

            var ReturnValue = Result.ToString();
            ObjectPool.Return(Result);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the parameters.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The parameters.</returns>
        private IParameter?[] GenerateParameters(TMappedClass queryObject)
        {
            var ORMObject = queryObject as IORMObject;
            var Parameters = IDProperties.ForEach(y => y.GetColumnInfo()[0].GetAsParameter(queryObject)).ToList();
            Parameters.AddRange(ReferenceProperties.ForEach(y => y.GetColumnInfo()[0].GetAsParameter(queryObject)));
            return Parameters.ToArray();
        }

        /// <summary>
        /// Generates the update query.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns></returns>
        private string GenerateUpdateQuery(Utils.TreeNode<Type>? node, TMappedClass queryObject)
        {
            if (node is null)
                return string.Empty;
            var Builder = ObjectPool.Get();
            var Splitter = string.Empty;

            //Generate parent queries
            for (int x = 0, nodeNodesCount = node.Nodes.Count; x < nodeNodesCount; x++)
            {
                var Parent = node.Nodes[x];
                var Result = GenerateUpdateQuery(Parent, queryObject);
                if (!string.IsNullOrEmpty(Result))
                {
                    Builder.AppendLine(Result);
                }
            }

            var Mapping = MappingInformation.Mappings[node.Data];
            string ReturnValue;
            if (Mapping.ReferenceProperties.Count == 0)
            {
                ReturnValue = Builder.ToString();
                ObjectPool.Return(Builder);
                return ReturnValue;
            }

            var ParameterList = ObjectPool.Get();
            var WhereClause = ObjectPool.Get();
            var FromClause = ObjectPool.Get();

            //Adding reference properties
            foreach (var ReferenceProperty in Mapping.ReferenceProperties)
            {
                ParameterList.Append(Splitter).Append(GetColumnName(ReferenceProperty)).Append("=").Append(GetParameterName(ReferenceProperty));
                Splitter = ",";
            }

            //From clause generation
            FromClause.AppendLine(GetTableName(Mapping))
                .Append(GenerateFromClause(node, queryObject));

            //Where clause generation
            WhereClause.Append(GenerateWhereClause(node, queryObject));

            //Generating final query
            Builder.AppendLineFormat(@"UPDATE {0}
SET {1}
FROM {2}WHERE {3};", GetTableName(Mapping), ParameterList, FromClause, WhereClause);

            ReturnValue = Builder.ToString();
            ObjectPool.Return(Builder);
            ObjectPool.Return(ParameterList);
            ObjectPool.Return(WhereClause);
            ObjectPool.Return(FromClause);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The where clause</returns>
        private string GenerateWhereClause(Utils.TreeNode<Type> node, TMappedClass queryObject)
        {
            var Result = ObjectPool.Get();
            var Mapping = MappingInformation.Mappings[node.Data];
            var Separator = string.Empty;
            for (int x = 0, nodeNodesCount = node.Nodes.Count; x < nodeNodesCount; x++)
            {
                var ParentNode = node.Nodes[x];
                var ParentResult = GenerateWhereClause(ParentNode, queryObject);
                if (!string.IsNullOrEmpty(ParentResult))
                {
                    Result.Append(Separator).Append(ParentResult);
                    Separator = "\r\nAND ";
                }
            }

            foreach (var IDProperty in Mapping.IDProperties)
            {
                Result.AppendFormat("{0}{1}={2}", Separator, GetColumnName(IDProperty), GetParameterName(IDProperty));
                Separator = "\r\nAND ";
            }
            var ReturnValue = Result.ToString();
            ObjectPool.Return(Result);
            return ReturnValue;
        }
    }
}