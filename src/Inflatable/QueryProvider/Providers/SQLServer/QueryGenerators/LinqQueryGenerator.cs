﻿/*
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
using Inflatable.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.LinqExpression.OrderBy.Enums;
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators
{
    /// <summary>
    /// SQL Server Linq query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="ILinqQueryGenerator{TMappedClass}"/>
    public class LinqQueryGenerator<TMappedClass> : LinqQueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqQueryGenerator{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        /// <param name="objectPool">The object pool.</param>
        /// <exception cref="ArgumentNullException">mappingInformation</exception>
        public LinqQueryGenerator(IMappingSource mappingInformation, ObjectPool<StringBuilder> objectPool)
            : base(mappingInformation, objectPool)
        {
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.LinqQuery;

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <returns>The resulting declarations.</returns>
        public override IQuery[] GenerateDeclarations() => [new Query(AssociatedType, CommandType.Text, "", QueryType)];

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The resulting query</returns>
        public override IQuery[] GenerateQueries(QueryData<TMappedClass> data)
        {
            if (data is null)
                return [];
            var ReturnValue = new List<IQuery>();
            foreach (var ChildMapping in MappingInformation.GetChildMappings(typeof(TMappedClass)))
            {
                var TypeGraph = MappingInformation.TypeGraphs[ChildMapping.ObjectType];
                ReturnValue.Add(new Query(ChildMapping.ObjectType, CommandType.Text, GenerateSelectQuery(TypeGraph?.Root, data), QueryType, [.. data.Parameters]));
            }
            return [.. ReturnValue];
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private string GenerateFromClause(Utils.TreeNode<Type> node)
        {
            var Result = ObjectPool.Get();
            var Mapping = MappingInformation.Mappings[node.Data];
            for (int X = 0, NodeNodesCount = node.Nodes.Count; X < NodeNodesCount; X++)
            {
                var ParentNode = node.Nodes[X];
                var ParentMapping = MappingInformation.Mappings[ParentNode.Data];
                var IDProperties = ObjectPool.Get();
                var Separator = "";
                foreach (var IDProperty in ParentMapping.IDProperties)
                {
                    IDProperties.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty), GetColumnName(IDProperty));
                    Separator = " AND ";
                }
                foreach (var IDProperty in ParentMapping.AutoIDProperties)
                {
                    IDProperties.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty), GetColumnName(IDProperty));
                    Separator = " AND ";
                }
                Result.AppendLine();
                Result.AppendFormat(CultureInfo.InvariantCulture, "INNER JOIN {0} ON {1}", GetTableName(ParentMapping), IDProperties);
                ObjectPool.Return(IDProperties);
                Result.Append(GenerateFromClause(ParentNode));
            }

            var ReturnValue = Result.ToString();
            ObjectPool.Return(Result);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the order by clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="data">The data.</param>
        /// <returns>Order by clause</returns>
        private string GenerateOrderByClause(Utils.TreeNode<Type> node, QueryData<TMappedClass> data)
        {
            var Builder = ObjectPool.Get();
            var Splitter = "";
            string ReturnValue = "";
            if (data.OrderByValues.Count == 0)
            {
                var Mapping = MappingInformation.Mappings[node.Data];
                for (int X = 0, NodeNodesCount = node.Nodes.Count; X < NodeNodesCount; X++)
                {
                    var ParentNode = node.Nodes[X];
                    var ParentResult = GenerateOrderByClause(ParentNode, data);
                    if (!string.IsNullOrEmpty(ParentResult))
                        return ParentResult;
                }
                foreach (var IDProperty in Mapping.IDProperties)
                {
                    Builder.Append(Splitter)
                       .Append(GetColumnName(IDProperty));
                    Splitter = ",";
                }
                ReturnValue = Builder.ToString();
                ObjectPool.Return(Builder);
                return ReturnValue;
            }

            foreach (var Column in data.OrderByValues.OrderBy(x => x.Order))
            {
                Builder.Append(Splitter)
                       .Append(Column.Property.Name)
                       .Append(Column.Direction == Direction.Descending ? " DESC" : "");
                Splitter = ",";
            }
            ReturnValue = Builder.ToString();
            ObjectPool.Return(Builder);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the parameter list.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private string GenerateParameterList(Utils.TreeNode<Type> node, QueryData<TMappedClass> data)
        {
            var Result = ObjectPool.Get();
            var Mapping = MappingInformation.Mappings[node.Data];
            var Separator = "";
            for (int X = 0, NodeNodesCount = node.Nodes.Count; X < NodeNodesCount; X++)
            {
                var ParentNode = node.Nodes[X];
                var ParentResult = GenerateParameterList(ParentNode, data);
                if (!string.IsNullOrEmpty(ParentResult))
                {
                    Result.Append(Separator).Append(ParentResult);
                    Separator = ",";
                }
            }

            foreach (var IDProperty in Mapping.IDProperties)
            {
                Result.AppendFormat(CultureInfo.InvariantCulture, "{0}{1} AS {2}", Separator, GetColumnName(IDProperty), "[" + IDProperty.Name + "]");
                Separator = ",";
            }
            foreach (var ReferenceProperty in Mapping.ReferenceProperties.Where(x => data.SelectValues.Count == 0
                                                                                  || data.SelectValues.Any(y => y.Name == x.Name)))
            {
                Result.AppendFormat(CultureInfo.InvariantCulture, "{0}{1} AS {2}", Separator, GetColumnName(ReferenceProperty), "[" + ReferenceProperty.Name + "]");
                Separator = ",";
            }
            var ReturnValue = Result.ToString();
            ObjectPool.Return(Result);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the select query.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private string GenerateSelectQuery(Utils.TreeNode<Type>? node, QueryData<TMappedClass> data)
        {
            if (node is null)
                return "";
            var Builder = ObjectPool.Get();
            var ParameterList = ObjectPool.Get();
            var FromClause = ObjectPool.Get();
            var WhereClause = ObjectPool.Get();
            var OrderByClause = ObjectPool.Get();
            var Mapping = MappingInformation.Mappings[node.Data];

            //Get From Clause
            FromClause.Append(GetTableName(Mapping));
            FromClause.Append(GenerateFromClause(node));

            //Get parameter listing
            ParameterList.Append(GenerateParameterList(node, data));

            //Get Where Clause
            WhereClause.Append(GenerateWhereClause(data, Mapping));

            //Get Order By clause
            OrderByClause.Append(GenerateOrderByClause(node, data));

            if (data.Count)
            {
                Builder.AppendLine("SELECT COUNT(*) AS Count FROM (");
            }
            //Generate final query
            Builder
                .Append("SELECT")
                .Append(data.Distinct ? " DISTINCT " : " ")
                .Append(ParameterList)
                .AppendLine()
                .Append("FROM ")
                .Append(FromClause)
                .AppendLine();
            if (WhereClause.Length > 0)
            {
                Builder.Append(WhereClause)
                       .AppendLine();
            }
            if (!data.Count)
            {
                if (OrderByClause.Length > 0)
                {
                    Builder.Append("ORDER BY ")
                        .Append(OrderByClause)
                        .AppendLine();
                }
                if (data.Top > 0 || data.Skip > 0)
                {
                    Builder.Append("OFFSET ").Append(data.Skip).AppendLine(" ROWS")
                           .Append("FETCH NEXT ").Append(data.Top).Append(" ROWS ONLY");
                }
            }
            if (data.Count)
            {
                Builder.AppendLine().Append(") AS _InternalQuery");
            }
            var ReturnValue = Builder.ToString().TrimEnd('\r', '\n', ' ', '\t') + ";";
            ObjectPool.Return(Builder);
            ObjectPool.Return(ParameterList);
            ObjectPool.Return(FromClause);
            ObjectPool.Return(WhereClause);
            ObjectPool.Return(OrderByClause);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The WHERE clause</returns>
        private string GenerateWhereClause(QueryData<TMappedClass> data, IMapping mapping)
        {
            data.WhereClause.SetColumnNames(MappingInformation, mapping);
            return data.WhereClause.ToString();
        }
    }
}