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
using Inflatable.ClassMapper.Interfaces;
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using System;
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
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.LoadProperty;

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <returns>The resulting declarations.</returns>
        public override IQuery GenerateDeclarations()
        {
            return new Query(CommandType.Text, "", QueryType.LoadProperty);
        }

        public override IQuery GenerateQuery(TMappedClass queryObject, string propertyName)
        {
            var ParentMappings = MappingInformation.GetParentMapping(AssociatedType);
            var Property = ParentMappings.SelectMany(x => x.MapProperties).FirstOrDefault(x => x.Name == propertyName);
            //GenerateSelectQuery(typeof(TMappedClass), Property);
            return new Query(CommandType.Text, "", QueryType.LoadProperty);
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
        /// Generates from clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The result</returns>
        private string GenerateParentFromClause(Utils.TreeNode<Type> node, IMapProperty property)
        {
            StringBuilder Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            foreach (var ParentNode in node.Nodes.Where(x => !MappingInformation.Mappings[x.Data].MapProperties.Contains(property)))
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
                Result.Append(GenerateParentFromClause(ParentNode, property));
            }
            return Result.ToString();
        }

        /// <summary>
        /// Generates the select query.
        /// </summary>
        /// <param name="foreignNode">The foreign node.</param>
        /// <param name="node">The node.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private string GenerateSelectQuery(Utils.TreeNode<Type> foreignNode, Utils.TreeNode<Type> node, IMapProperty property)
        {
            StringBuilder Builder = new StringBuilder();
            StringBuilder ParameterList = new StringBuilder();
            StringBuilder FromClause = new StringBuilder();
            StringBuilder WhereClause = new StringBuilder();

            var Mapping = MappingInformation.Mappings[node.Data];
            var ForeignMapping = MappingInformation.Mappings[property.PropertyType];

            //Get From Clause
            FromClause.Append(GetTableName(ForeignMapping));
            FromClause.Append(GenerateFromClause(foreignNode));
            FromClause.Append(GenerateParentFromClause(node, property));

            //Get parameter listing
            ParameterList.Append(GenerateParameterList(foreignNode));

            //Get Where Clause
            WhereClause.Append(GenerateWhereClause(node));

            //Generate final query
            Builder.Append($"SELECT {ParameterList}" +
                $"\r\nFROM {FromClause}" +
                $"\r\n{WhereClause};");
            return Builder.ToString();
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="data">The data.</param>
        /// <returns>The WHERE clause</returns>
        private string GenerateWhereClause(Utils.TreeNode<Type> node)
        {
            return "";
            //return data.WhereClause.ToString();
        }
    }
}