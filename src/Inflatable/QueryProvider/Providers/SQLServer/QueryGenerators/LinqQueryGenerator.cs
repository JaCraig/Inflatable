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
using Inflatable.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.LinqExpression.OrderBy.Enums;
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators
{
    /// <summary>
    /// SQL Server Linq query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="Interfaces.ILinqQueryGenerator{TMappedClass}"/>
    public class LinqQueryGenerator<TMappedClass> : LinqQueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqQueryGenerator{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        /// <exception cref="System.ArgumentNullException">mappingInformation</exception>
        public LinqQueryGenerator(MappingSource mappingInformation)
            : base(mappingInformation)
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
        public override IQuery[] GenerateDeclarations()
        {
            return new IQuery[] { new Query(AssociatedType, CommandType.Text, "", QueryType) };
        }

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The resulting query</returns>
        public override IQuery[] GenerateQueries(QueryData<TMappedClass> data)
        {
            var ReturnValue = new List<IQuery>();
            var ChildMappings = MappingInformation.GetChildMappings(typeof(TMappedClass));
            foreach (var ChildMapping in ChildMappings)
            {
                var TypeGraph = MappingInformation.TypeGraphs[ChildMapping.ObjectType];
                ReturnValue.Add(new Query(ChildMapping.ObjectType, CommandType.Text, GenerateSelectQuery(TypeGraph.Root, data), QueryType, data.Parameters.ToArray()));
            }
            return ReturnValue.ToArray();
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private string GenerateFromClause(Utils.TreeNode<Type> node)
        {
            var Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            for (int x = 0, nodeNodesCount = node.Nodes.Count; x < nodeNodesCount; x++)
            {
                var ParentNode = node.Nodes[x];
                var ParentMapping = MappingInformation.Mappings[ParentNode.Data];
                var IDProperties = new StringBuilder();
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

        /// <summary>
        /// Generates the order by clause.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private string GenerateOrderByClause(QueryData<TMappedClass> data)
        {
            var Builder = new StringBuilder();
            string Splitter = "";
            if (data.OrderByValues.Count == 0)
            {
                return "";
            }

            Builder.Append("ORDER BY ");
            foreach (var Column in data.OrderByValues.OrderBy(x => x.Order))
            {
                Builder.Append(Splitter)
                       .Append(Column.Property.Name)
                       .Append(Column.Direction == Direction.Descending ? " DESC" : "");
                Splitter = ",";
            }
            return Builder.ToString();
        }

        /// <summary>
        /// Generates the parameter list.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private string GenerateParameterList(Utils.TreeNode<Type> node, QueryData<TMappedClass> data)
        {
            var Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            string Separator = "";
            for (int x = 0, nodeNodesCount = node.Nodes.Count; x < nodeNodesCount; x++)
            {
                var ParentNode = node.Nodes[x];
                var ParentResult = GenerateParameterList(ParentNode, data);
                if (!string.IsNullOrEmpty(ParentResult))
                {
                    Result.Append(Separator).Append(ParentResult);
                    Separator = ",";
                }
            }

            foreach (var IDProperty in Mapping.IDProperties)
            {
                Result.AppendFormat("{0}{1} AS {2}", Separator, GetColumnName(IDProperty), "[" + IDProperty.Name + "]");
                Separator = ",";
            }
            foreach (var ReferenceProperty in Mapping.ReferenceProperties.Where(x => data.SelectValues.Count == 0
                                                                                  || data.SelectValues.Any(y => y.Name == x.Name)))
            {
                Result.AppendFormat("{0}{1} AS {2}", Separator, GetColumnName(ReferenceProperty), "[" + ReferenceProperty.Name + "]");
                Separator = ",";
            }
            return Result.ToString();
        }

        /// <summary>
        /// Generates the select query.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private string GenerateSelectQuery(Utils.TreeNode<Type> node, QueryData<TMappedClass> data)
        {
            var Builder = new StringBuilder();
            var ParameterList = new StringBuilder();
            var FromClause = new StringBuilder();
            var WhereClause = new StringBuilder();
            var OrderByClause = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];

            //Get From Clause
            FromClause.Append(GetTableName(Mapping));
            FromClause.Append(GenerateFromClause(node));

            //Get parameter listing
            ParameterList.Append(GenerateParameterList(node, data));

            //Get Where Clause
            WhereClause.Append(GenerateWhereClause(data, Mapping));

            OrderByClause.Append(GenerateOrderByClause(data));

            //Generate final query
            Builder.Append(($"SELECT{(data.Distinct ? " DISTINCT" : "")}{(data.Top > 0 ? $" TOP {data.Top}" : "")} {ParameterList}" +
                $"\r\nFROM {FromClause}" +
                $"\r\n{WhereClause}" +
                $"\r\n{OrderByClause}").TrimEnd('\r', '\n', ' ', '\t')).Append(";");
            return Builder.ToString();
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