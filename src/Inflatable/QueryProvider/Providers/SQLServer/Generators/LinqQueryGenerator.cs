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
using Inflatable.LinqExpression;
using Inflatable.LinqExpression.OrderBy.Enums;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using System;
using System.Data;
using System.Linq;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.Generators
{
    /// <summary>
    /// SQL Server Linq query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="Interfaces.ILinqQueryGenerator{TMappedClass}"/>
    public class LinqQueryGenerator<TMappedClass> : ILinqQueryGenerator<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqQueryGenerator{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">The mapping information.</param>
        /// <exception cref="System.ArgumentNullException">mappingInformation</exception>
        public LinqQueryGenerator(MappingSource mappingInformation)
        {
            MappingInformation = mappingInformation ?? throw new System.ArgumentNullException(nameof(mappingInformation));
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
        /// Generates the query.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The resulting query</returns>
        public IQuery GenerateQuery(QueryData<TMappedClass> data)
        {
            var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
            return new Query(CommandType.Text, GenerateSelectQuery(TypeGraph.Root, data), QueryType.All, data.Parameters.ToArray());
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="idProperty">The identifier property.</param>
        /// <returns>The column name</returns>
        protected string GetColumnName(IIDProperty idProperty)
        {
            return GetTableName(idProperty.ParentMapping) + ".[" + idProperty.ColumnName + "]";
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="idProperty">The identifier property.</param>
        /// <returns>The column name</returns>
        protected string GetColumnName(IAutoIDProperty idProperty)
        {
            return GetTableName(idProperty.ParentMapping) + ".[" + idProperty.ColumnName + "]";
        }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="referenceProperty">The reference property.</param>
        /// <returns>The column name</returns>
        protected string GetColumnName(IProperty referenceProperty)
        {
            return GetTableName(referenceProperty.ParentMapping) + ".[" + referenceProperty.ColumnName + "]";
        }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <param name="idProperty">The identifier property.</param>
        /// <returns>The parameter name</returns>
        protected string GetParameterName(IIDProperty idProperty)
        {
            return "@" + idProperty.Name;
        }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <param name="referenceProperty">The reference property.</param>
        /// <returns>The parameter name</returns>
        protected string GetParameterName(IProperty referenceProperty)
        {
            return "@" + referenceProperty.Name;
        }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <param name="autoIDProperty">The automatic identifier property.</param>
        /// <returns>The parameter type name</returns>
        protected string GetParameterType(IAutoIDProperty autoIDProperty)
        {
            return "BIGINT";
        }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <param name="iDProperty">The i d property.</param>
        /// <returns>The parameter type name</returns>
        protected string GetParameterType(IIDProperty iDProperty)
        {
            return iDProperty.PropertyType.To(SqlDbType.Int).ToString().ToUpper();
        }

        /// <summary>
        /// Gets the name of the parent column.
        /// </summary>
        /// <param name="childMapping">The child mapping.</param>
        /// <param name="autoIDProperty">The automatic identifier property.</param>
        /// <returns>The parent column name</returns>
        protected string GetParentColumnName(IMapping childMapping, IAutoIDProperty autoIDProperty)
        {
            return GetTableName(childMapping) + ".[" + autoIDProperty.ParentMapping.TableName + autoIDProperty.ColumnName + "]";
        }

        /// <summary>
        /// Gets the name of the parent column.
        /// </summary>
        /// <param name="childMapping">The child mapping.</param>
        /// <param name="iDProperty">The i d property.</param>
        /// <returns>The parent column name</returns>
        protected string GetParentColumnName(IMapping childMapping, IIDProperty iDProperty)
        {
            return GetTableName(childMapping) + ".[" + iDProperty.ParentMapping.TableName + iDProperty.ColumnName + "]";
        }

        /// <summary>
        /// Gets the name of the parent parameter.
        /// </summary>
        /// <param name="autoIDProperty">The automatic identifier property.</param>
        /// <returns>The parent parameter name</returns>
        protected string GetParentParameterName(IAutoIDProperty autoIDProperty)
        {
            return "@" + autoIDProperty.ParentMapping.TableName + autoIDProperty.ColumnName + "Temp";
        }

        /// <summary>
        /// Gets the name of the parent parameter.
        /// </summary>
        /// <param name="iDProperty">The i d property.</param>
        /// <returns>The parent parameter name</returns>
        protected string GetParentParameterName(IIDProperty iDProperty)
        {
            return "@" + iDProperty.ParentMapping.TableName + iDProperty.Name + "_Temp";
        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <param name="parentMapping">The parent mapping.</param>
        /// <returns>The name of the table</returns>
        protected string GetTableName(IMapping parentMapping)
        {
            return "[dbo].[" + parentMapping.TableName + "]";
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Generates the order by clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private string GenerateOrderByClause(Utils.TreeNode<Type> node, QueryData<TMappedClass> data)
        {
            StringBuilder Builder = new StringBuilder();
            string Splitter = "";
            if (data.OrderByValues.Count == 0)
                return "";
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
            StringBuilder Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            string Separator = "";
            foreach (var ParentNode in node.Nodes)
            {
                var ParentResult = GenerateParameterList(ParentNode, data);
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
            StringBuilder Builder = new StringBuilder();
            StringBuilder ParameterList = new StringBuilder();
            StringBuilder FromClause = new StringBuilder();
            StringBuilder WhereClause = new StringBuilder();
            StringBuilder OrderByClause = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];

            //Get From Clause
            FromClause.Append(GetTableName(Mapping));
            FromClause.Append(GenerateFromClause(node));

            //Get parameter listing
            ParameterList.Append(GenerateParameterList(node, data));

            //Get Where Clause
            WhereClause.Append(GenerateWhereClause(node, data));

            OrderByClause.Append(GenerateOrderByClause(node, data));

            //Generate final query
            Builder.Append($"SELECT{(data.Distinct ? " DISTINCT" : "")}{(data.Top > 0 ? $" TOP {data.Top}" : "")} {ParameterList}" +
                $"FROM {FromClause}" +
                $"{WhereClause}" +
                $"{OrderByClause}".Trim() + ";");
            return Builder.ToString();
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="data">The data.</param>
        /// <returns>The WHERE clause</returns>
        private string GenerateWhereClause(Utils.TreeNode<Type> node, QueryData<TMappedClass> data)
        {
            return data.WhereClause.ToString();
        }
    }
}