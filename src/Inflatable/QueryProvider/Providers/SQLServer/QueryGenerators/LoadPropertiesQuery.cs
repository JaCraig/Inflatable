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
    /// Load properties query
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="BaseClasses.PropertyQueryGeneratorBaseClass{TMappedClass}"/>
    public class LoadPropertiesQuery<TMappedClass> : PropertyQueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadPropertiesQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">Mapping information</param>
        public LoadPropertiesQuery(MappingSource mappingInformation)
            : base(mappingInformation)
        {
            var ParentMappings = MappingInformation.GetParentMapping(AssociatedType);
            IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
            Queries = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.LoadProperty;

        /// <summary>
        /// Gets or sets the identifier properties.
        /// </summary>
        /// <value>The identifier properties.</value>
        private IEnumerable<IIDProperty> IDProperties { get; set; }

        /// <summary>
        /// Gets or sets the queries.
        /// </summary>
        /// <value>The queries.</value>
        private IDictionary<string, string> Queries { get; set; }

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <returns>The resulting declarations.</returns>
        public override IQuery GenerateDeclarations()
        {
            return new Query(CommandType.Text, "", QueryType.LoadProperty);
        }

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="queryObject">The object to generate the queries from.</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>The resulting query</returns>
        public override IQuery GenerateQuery(TMappedClass queryObject, string propertyName)
        {
            var TempQueryText = "";
            if (!Queries.ContainsKey(propertyName))
            {
                var ParentMappings = MappingInformation.GetParentMapping(AssociatedType);
                var Property = ParentMappings.SelectMany(x => x.MapProperties).FirstOrDefault(x => x.Name == propertyName);
                var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
                var ForeignTypeGraph = MappingInformation.TypeGraphs[Property.PropertyType];
                TempQueryText = GenerateSelectQuery(ForeignTypeGraph, TypeGraph, Property);
            }
            else
            {
                TempQueryText = Queries[propertyName];
            }
            return new Query(CommandType.Text, TempQueryText, QueryType.LoadProperty, GenerateParameters(queryObject));
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
                Result.AppendLine();
                Result.AppendFormat("INNER JOIN {0} ON {1}", GetTableName(ParentMapping), TempIDProperties);
                Result.Append(GenerateFromClause(ParentNode));
            }
            return Result.ToString();
        }

        /// <summary>
        /// Generates the parameter list.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
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
            foreach (var MapProperty in Mapping.MapProperties)
            {
                Result.AppendFormat("{0}{1} AS {2}", Separator, GetColumnName(MapProperty), "[" + MapProperty.Name + "]");
                Separator = ",";
            }
            return Result.ToString();
        }

        /// <summary>
        /// Generates the parameters.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The parameters</returns>
        private IParameter[] GenerateParameters(TMappedClass queryObject)
        {
            return IDProperties.ForEach(x => x.GetAsParameter(queryObject)).ToArray();
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="property">The property.</param>
        /// <returns>The result</returns>
        private string GenerateParentFromClause(Utils.TreeNode<Type> node, IMapProperty property)
        {
            StringBuilder Result = new StringBuilder();
            StringBuilder TempIDProperties = new StringBuilder();
            string Separator = "";
            foreach (var IDProperty in property.ForeignMapping.IDProperties)
            {
                TempIDProperties.AppendFormat("{0}{1}={2}", Separator, GetColumnName(property), GetForeignColumnName(property));
                Separator = " AND ";
            }
            Result.AppendLine();
            Result.AppendFormat("INNER JOIN {0} ON {1}", GetTableName(MappingInformation.GetParentMapping(AssociatedType)
                                                                                        .First(x => x.IDProperties.Count > 0)),
                                                         TempIDProperties);
            return Result.ToString();
        }

        /// <summary>
        /// Generates the select query.
        /// </summary>
        /// <param name="foreignNode">The foreign node.</param>
        /// <param name="node">The node.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private string GenerateSelectQuery(Utils.Tree<Type> foreignNode, Utils.Tree<Type> node, IMapProperty property)
        {
            StringBuilder Builder = new StringBuilder();
            StringBuilder ParameterList = new StringBuilder();
            StringBuilder FromClause = new StringBuilder();
            StringBuilder WhereClause = new StringBuilder();

            var Mapping = MappingInformation.Mappings[node.Root.Data];
            var ForeignMapping = MappingInformation.Mappings[property.PropertyType];

            //Get From Clause
            FromClause.Append(GetTableName(ForeignMapping));
            FromClause.Append(GenerateFromClause(foreignNode.Root));
            FromClause.Append(GenerateParentFromClause(node.Root, property));

            //Get parameter listing
            ParameterList.Append(GenerateParameterList(foreignNode.Root));

            //Get Where Clause
            WhereClause.Append(GenerateWhereClause(node.Root));

            //Generate final query
            Builder.Append($"SELECT {ParameterList}" +
                $"\r\nFROM {FromClause}" +
                $"\r\nWHERE {WhereClause};");
            return Builder.ToString();
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The where clause</returns>
        private string GenerateWhereClause(Utils.TreeNode<Type> node)
        {
            StringBuilder Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            string Separator = "";
            foreach (var ParentNode in node.Nodes)
            {
                var ParentResult = GenerateWhereClause(ParentNode);
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