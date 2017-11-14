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
using Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators.HelperClasses;
using Inflatable.Utils;
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
            var ChildMappings = MappingInformation.GetChildMappings(typeof(TMappedClass));
            var ParentMappings = ChildMappings.SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType)).Distinct();
            IDProperties = ParentMappings.SelectMany(x => x.IDProperties);
            Queries = new ListMapping<string, QueryGeneratorData>();
            SetupQueries();
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.LoadProperty;

        /// <summary>
        /// Gets the identifier properties.
        /// </summary>
        /// <value>The identifier properties.</value>
        private IEnumerable<IIDProperty> IDProperties { get; set; }

        /// <summary>
        /// Gets or sets the queries.
        /// </summary>
        /// <value>The queries.</value>
        private ListMapping<string, QueryGeneratorData> Queries { get; set; }

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
        /// <param name="queryObject">The object to generate the queries from.</param>
        /// <param name="property">The property.</param>
        /// <returns>The resulting query</returns>
        public override IQuery[] GenerateQueries(TMappedClass queryObject, IClassProperty property)
        {
            var ParentMappings = MappingInformation.GetChildMappings(AssociatedType).SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType)).Distinct().ToList();
            switch (property)
            {
                case IMapProperty Property:
                    return LoadMapProperty(Property, queryObject);

                case IManyToManyProperty ManyToManyProperty:
                    return LoadManyToManyProperty(ManyToManyProperty, queryObject);

                case IManyToOneListProperty ManyToOne:
                    return LoadManyToOneProperty(ManyToOne, queryObject);

                case IManyToOneProperty ManyToOne:
                    return LoadManyToOneProperty(ManyToOne, queryObject);
            }
            return new IQuery[0];
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The from clause</returns>
        private string GenerateFromClause(Utils.TreeNode<Type> node, string suffix = "")
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
                    TempIDProperties.AppendFormat("{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty, suffix), GetColumnName(IDProperty, suffix));
                    Separator = " AND ";
                }
                foreach (var IDProperty in ParentMapping.AutoIDProperties)
                {
                    TempIDProperties.AppendFormat("{0}{1}={2}", Separator, GetParentColumnName(Mapping, IDProperty, suffix), GetColumnName(IDProperty, suffix));
                    Separator = " AND ";
                }
                Result.AppendLine();
                var AsStatement = "";
                if (!string.IsNullOrEmpty(suffix))
                    AsStatement = " AS " + GetTableName(ParentMapping, suffix);
                Result.AppendFormat("INNER JOIN {0}{1} ON {2}", GetTableName(ParentMapping), AsStatement, TempIDProperties);
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
            return Result.ToString();
        }

        /// <summary>
        /// Generates the parameters.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <param name="idProperties">The identifier properties.</param>
        /// <returns>The parameters</returns>
        private IParameter[] GenerateParameters(TMappedClass queryObject, IEnumerable<IIDProperty> idProperties)
        {
            return idProperties.ForEach(x => x.GetColumnInfo()[0].GetAsParameter(queryObject)).ToArray();
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The result</returns>
        private string GenerateParentFromClause(IMapProperty property, string suffix)
        {
            StringBuilder Result = new StringBuilder();
            StringBuilder TempIDProperties = new StringBuilder();
            string Separator = "";
            foreach (var IDProperty in property.ForeignMapping.IDProperties)
            {
                TempIDProperties.AppendFormat("{0}{1}={2}", Separator, GetColumnName(property, suffix), GetForeignColumnName(property));
                Separator = " AND ";
            }
            Result.AppendLine();
            var AsStatement = string.IsNullOrEmpty(suffix) ? "" : " as [" + property.ParentMapping.TableName + suffix + "]";
            Result.AppendFormat("INNER JOIN {0} ON {1}", GetTableName(property.ParentMapping) + AsStatement, TempIDProperties);
            return Result.ToString();
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The result</returns>
        private string GenerateParentFromClause(IManyToManyProperty property)
        {
            StringBuilder Result = new StringBuilder();
            StringBuilder TempIDProperties = new StringBuilder();
            string Separator = "";
            foreach (var IDProperty in property.ForeignMapping.IDProperties)
            {
                TempIDProperties.AppendFormat("{0}{1}={2}",
                    Separator,
                    "[" + property.ParentMapping.SchemaName + "].[" + property.TableName + "].[" + property.ForeignMapping.TableName + IDProperty.ColumnName + "]",
                    GetColumnName(IDProperty));
                Separator = " AND ";
            }
            Result.AppendLine();
            Result.AppendFormat("INNER JOIN {0} ON {1}", "[" + property.ParentMapping.SchemaName + "].[" + property.TableName + "]", TempIDProperties);
            return Result.ToString();
        }

        private string GenerateParentFromClause(IManyToOneProperty manyToOne)
        {
            StringBuilder Result = new StringBuilder();
            StringBuilder TempIDProperties = new StringBuilder();
            string Separator = "";
            foreach (var IDProperty in manyToOne.ForeignMapping.IDProperties)
            {
                TempIDProperties.AppendFormat("{0}{1}={2}",
                    Separator,
                    GetTableName(manyToOne.ParentMapping) + ".[" + manyToOne.ColumnName + manyToOne.ForeignMapping.TableName + IDProperty.ColumnName + "]",
                    GetColumnName(IDProperty));
                Separator = " AND ";
            }
            Result.AppendLine();
            Result.AppendFormat("INNER JOIN {0} ON {1}", GetTableName(manyToOne.ParentMapping), TempIDProperties);
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
            var ForeignMapping = MappingInformation.Mappings[foreignNode.Root.Data];
            var SameObject = "";

            if (MappingInformation.GetChildMappings(Mapping.ObjectType).SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType)).Contains(ForeignMapping))
                SameObject = "2";

            //Get From Clause
            FromClause.Append(GetTableName(ForeignMapping));
            FromClause.Append(GenerateFromClause(foreignNode.Root));
            FromClause.Append(GenerateParentFromClause(property, SameObject));
            FromClause.Append(GenerateFromClause(MappingInformation.TypeGraphs[property.ParentMapping.ObjectType].Root, SameObject));

            //Get parameter listing
            ParameterList.Append(GenerateParameterList(foreignNode.Root));

            //Get Where Clause
            WhereClause.Append(GenerateWhereClause(node.Root, SameObject));

            //Generate final query
            Builder.Append($"SELECT {ParameterList}" +
                $"\r\nFROM {FromClause}" +
                $"\r\nWHERE {WhereClause};");
            return Builder.ToString();
        }

        /// <summary>
        /// Generates the select query.
        /// </summary>
        /// <param name="foreignNode">The foreign node.</param>
        /// <param name="node">The node.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private string GenerateSelectQuery(Utils.Tree<Type> foreignNode, Utils.Tree<Type> node, IManyToManyProperty property)
        {
            StringBuilder Builder = new StringBuilder();
            StringBuilder ParameterList = new StringBuilder();
            StringBuilder FromClause = new StringBuilder();
            StringBuilder WhereClause = new StringBuilder();

            var Mapping = MappingInformation.Mappings[node.Root.Data];
            var ForeignMapping = MappingInformation.Mappings[foreignNode.Root.Data];

            //Get From Clause
            FromClause.Append(GetTableName(ForeignMapping));
            FromClause.Append(GenerateFromClause(foreignNode.Root));
            FromClause.Append(GenerateParentFromClause(property));

            //Get parameter listing
            ParameterList.Append(GenerateParameterList(foreignNode.Root));

            //Get Where Clause
            WhereClause.Append(GenerateWhereClause(property));

            //Generate final query
            Builder.Append($"SELECT {ParameterList}" +
                $"\r\nFROM {FromClause}" +
                $"\r\nWHERE {WhereClause};");
            return Builder.ToString();
        }

        private string GenerateSelectQuery(Tree<Type> foreignNode, Tree<Type> node, IManyToOneListProperty manyToOne)
        {
            StringBuilder Builder = new StringBuilder();
            StringBuilder ParameterList = new StringBuilder();
            StringBuilder FromClause = new StringBuilder();
            StringBuilder WhereClause = new StringBuilder();

            var Mapping = MappingInformation.Mappings[node.Root.Data];
            var ForeignMapping = MappingInformation.Mappings[foreignNode.Root.Data];

            //Get From Clause
            FromClause.Append(GetTableName(ForeignMapping));
            FromClause.Append(GenerateFromClause(foreignNode.Root));

            //Get parameter listing
            ParameterList.Append(GenerateParameterList(foreignNode.Root));

            //Get Where Clause
            WhereClause.Append(GenerateWhereClause(manyToOne));

            //Generate final query
            Builder.Append($"SELECT {ParameterList}" +
                $"\r\nFROM {FromClause}" +
                $"\r\nWHERE {WhereClause};");
            return Builder.ToString();
        }

        private string GenerateSelectQuery(Tree<Type> foreignNode, Tree<Type> node, IManyToOneProperty manyToOne)
        {
            StringBuilder Builder = new StringBuilder();
            StringBuilder ParameterList = new StringBuilder();
            StringBuilder FromClause = new StringBuilder();
            StringBuilder WhereClause = new StringBuilder();

            var Mapping = MappingInformation.Mappings[node.Root.Data];
            var ForeignMapping = MappingInformation.Mappings[foreignNode.Root.Data];

            //Get From Clause
            FromClause.Append(GetTableName(ForeignMapping));
            FromClause.Append(GenerateFromClause(foreignNode.Root));
            FromClause.Append(GenerateParentFromClause(manyToOne));

            //Get parameter listing
            ParameterList.Append(GenerateParameterList(foreignNode.Root));

            //Get Where Clause
            WhereClause.Append(GenerateWhereClause(manyToOne));

            //Generate final query
            Builder.Append($"SELECT {ParameterList}" +
                $"\r\nFROM {FromClause}" +
                $"\r\nWHERE {WhereClause};");
            return Builder.ToString();
        }

        private string GenerateWhereClause(IManyToOneProperty manyToOne)
        {
            StringBuilder Result = new StringBuilder();
            string Separator = "";
            var ParentIDMappings = MappingInformation.GetParentMapping(manyToOne.ParentMapping.ObjectType).SelectMany(x => x.IDProperties);
            foreach (var ParentIDMapping in ParentIDMappings)
            {
                Result.AppendFormat("{0}{1}={2}",
                    Separator,
                    GetColumnName(ParentIDMapping),
                    GetParameterName(ParentIDMapping));
                Separator = "\r\nAND ";
            }
            return Result.ToString();
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="manyToOne">The many to one.</param>
        /// <returns></returns>
        private string GenerateWhereClause(IManyToOneListProperty manyToOne)
        {
            StringBuilder Result = new StringBuilder();
            string Separator = "";
            var ParentIDMappings = MappingInformation.GetParentMapping(manyToOne.ParentMapping.ObjectType).SelectMany(x => x.IDProperties);
            foreach (var ParentIDMapping in ParentIDMappings)
            {
                Result.AppendFormat("{0}{1}={2}",
                    Separator,
                    "[" + manyToOne.ForeignMapping.SchemaName + "].[" + manyToOne.ForeignMapping.TableName + "].[" + manyToOne.ColumnName + ParentIDMapping.ParentMapping.TableName + ParentIDMapping.ColumnName + "]",
                    GetParameterName(ParentIDMapping));
                Separator = "\r\nAND ";
            }
            return Result.ToString();
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private string GenerateWhereClause(IManyToManyProperty property)
        {
            StringBuilder Result = new StringBuilder();
            string Separator = "";
            var ParentIDMappings = MappingInformation.GetParentMapping(property.ParentMapping.ObjectType).SelectMany(x => x.IDProperties);
            var Prefix = "";
            if (ParentIDMappings.Any(x => x.ParentMapping == property.ForeignMapping))
                Prefix = "Parent_";
            foreach (var ParentIDMapping in ParentIDMappings)
            {
                Result.AppendFormat("{0}{1}={2}",
                    Separator,
                    "[" + property.ParentMapping.SchemaName + "].[" + property.TableName + "].[" + Prefix + ParentIDMapping.ParentMapping.TableName + ParentIDMapping.ColumnName + "]",
                    GetParameterName(ParentIDMapping));
                Separator = "\r\nAND ";
            }
            return Result.ToString();
        }

        /// <summary>
        /// Generates the where clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns>The where clause</returns>
        private string GenerateWhereClause(Utils.TreeNode<Type> node, string suffix = "")
        {
            StringBuilder Result = new StringBuilder();
            var Mapping = MappingInformation.Mappings[node.Data];
            string Separator = "";
            foreach (var ParentNode in node.Nodes)
            {
                var ParentResult = GenerateWhereClause(ParentNode, suffix);
                if (!string.IsNullOrEmpty(ParentResult))
                {
                    Result.Append(Separator + ParentResult);
                    Separator = "\r\nAND ";
                }
            }
            foreach (var IDProperty in Mapping.IDProperties)
            {
                Result.AppendFormat("{0}{1}={2}", Separator, GetColumnName(IDProperty, suffix), GetParameterName(IDProperty));
                Separator = "\r\nAND ";
            }
            return Result.ToString();
        }

        private IQuery[] LoadManyToManyProperty(IManyToManyProperty property, TMappedClass queryObject)
        {
            var ChildMappings = MappingInformation.GetChildMappings(property.PropertyType);
            List<IQuery> ReturnValue = new List<IQuery>();

            if (!Queries.ContainsKey(property.Name))
            {
                foreach (var ChildMapping in ChildMappings)
                {
                    var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
                    var ForeignTypeGraph = MappingInformation.TypeGraphs[ChildMapping.ObjectType];

                    Queries.Add(property.Name, new QueryGeneratorData
                    {
                        AssociatedMapping = ChildMapping,
                        IDProperties = IDProperties,
                        QueryText = GenerateSelectQuery(ForeignTypeGraph, TypeGraph, property),
                    });
                }
            }
            foreach (var TempQuery in Queries[property.Name])
            {
                ReturnValue.Add(new Query(TempQuery.AssociatedMapping.ObjectType, CommandType.Text, TempQuery.QueryText, QueryType.LoadProperty, GenerateParameters(queryObject, TempQuery.IDProperties)));
            }
            return ReturnValue.ToArray();
        }

        /// <summary>
        /// Loads the many to one property.
        /// </summary>
        /// <param name="manyToOne">The many to one.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns></returns>
        private IQuery[] LoadManyToOneProperty(IManyToOneProperty manyToOne, TMappedClass queryObject)
        {
            var ChildMappings = MappingInformation.GetChildMappings(manyToOne.PropertyType);

            List<IQuery> ReturnValue = new List<IQuery>();

            if (!Queries.ContainsKey(manyToOne.Name))
            {
                foreach (var ChildMapping in ChildMappings)
                {
                    var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
                    var ForeignTypeGraph = MappingInformation.TypeGraphs[ChildMapping.ObjectType];

                    Queries.Add(manyToOne.Name, new QueryGeneratorData
                    {
                        AssociatedMapping = ChildMapping,
                        IDProperties = IDProperties,
                        QueryText = GenerateSelectQuery(ForeignTypeGraph, TypeGraph, manyToOne)
                    });
                }
            }
            foreach (var TempQuery in Queries[manyToOne.Name])
            {
                ReturnValue.Add(new Query(TempQuery.AssociatedMapping.ObjectType, CommandType.Text, TempQuery.QueryText, QueryType.LoadProperty, GenerateParameters(queryObject, TempQuery.IDProperties)));
            }
            return ReturnValue.ToArray();
        }

        /// <summary>
        /// Loads the many to one property.
        /// </summary>
        /// <param name="manyToOne">The many to one.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns></returns>
        private IQuery[] LoadManyToOneProperty(IManyToOneListProperty manyToOne, TMappedClass queryObject)
        {
            var ChildMappings = MappingInformation.GetChildMappings(manyToOne.PropertyType);

            List<IQuery> ReturnValue = new List<IQuery>();

            if (!Queries.ContainsKey(manyToOne.Name))
            {
                foreach (var ChildMapping in ChildMappings)
                {
                    var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
                    var ForeignTypeGraph = MappingInformation.TypeGraphs[ChildMapping.ObjectType];

                    Queries.Add(manyToOne.Name, new QueryGeneratorData
                    {
                        AssociatedMapping = ChildMapping,
                        IDProperties = IDProperties,
                        QueryText = GenerateSelectQuery(ForeignTypeGraph, TypeGraph, manyToOne)
                    });
                }
            }
            foreach (var TempQuery in Queries[manyToOne.Name])
            {
                ReturnValue.Add(new Query(TempQuery.AssociatedMapping.ObjectType, CommandType.Text, TempQuery.QueryText, QueryType.LoadProperty, GenerateParameters(queryObject, TempQuery.IDProperties)));
            }
            return ReturnValue.ToArray();
        }

        /// <summary>
        /// Loads the map property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns>The queries to load the map property.</returns>
        private IQuery[] LoadMapProperty(IMapProperty property, TMappedClass queryObject)
        {
            var ChildMappings = MappingInformation.GetChildMappings(property.PropertyType);

            List<IQuery> ReturnValue = new List<IQuery>();

            if (!Queries.ContainsKey(property.Name))
            {
                foreach (var ChildMapping in ChildMappings)
                {
                    var TypeGraph = MappingInformation.TypeGraphs[AssociatedType];
                    var ForeignTypeGraph = MappingInformation.TypeGraphs[ChildMapping.ObjectType];

                    Queries.Add(property.Name, new QueryGeneratorData
                    {
                        AssociatedMapping = ChildMapping,
                        IDProperties = IDProperties,
                        QueryText = GenerateSelectQuery(ForeignTypeGraph, TypeGraph, property)
                    });
                }
            }
            foreach (var TempQuery in Queries[property.Name])
            {
                ReturnValue.Add(new Query(TempQuery.AssociatedMapping.ObjectType, CommandType.Text, TempQuery.QueryText, QueryType.LoadProperty, GenerateParameters(queryObject, TempQuery.IDProperties)));
            }
            return ReturnValue.ToArray();
        }

        /// <summary>
        /// Sets up the queries.
        /// </summary>
        private void SetupQueries()
        {
            var ParentMappings = MappingInformation.GetParentMapping(typeof(TMappedClass));
            foreach (var ParentMapping in ParentMappings)
            {
                foreach (var Property in ParentMapping.ManyToManyProperties)
                {
                    LoadManyToManyProperty(Property, default(TMappedClass));
                }
                foreach (var Property in ParentMapping.ManyToOneProperties)
                {
                    switch (Property)
                    {
                        case IManyToOneListProperty ManyToOne:
                            LoadManyToOneProperty(ManyToOne, default(TMappedClass));
                            break;

                        case IManyToOneProperty ManyToOne:
                            LoadManyToOneProperty(ManyToOne, default(TMappedClass));
                            break;
                    }
                }
                foreach (var Property in ParentMapping.MapProperties)
                {
                    LoadMapProperty(Property, default(TMappedClass));
                }
            }
        }
    }
}