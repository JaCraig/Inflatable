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
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators.HelperClasses;
using Microsoft.Extensions.ObjectPool;
using SQLHelperDB.HelperClasses.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators
{
    /// <summary>
    /// Save properties query generator
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="PropertyQueryGeneratorBaseClass{TMappedClass}"/>
    public class SavePropertiesQuery<TMappedClass> : PropertyQueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SavePropertiesQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">Mapping information</param>
        /// <param name="objectPool">The object pool.</param>
        public SavePropertiesQuery(IMappingSource mappingInformation, ObjectPool<StringBuilder> objectPool)
            : base(mappingInformation, objectPool)
        {
            IDProperties = MappingInformation.GetChildMappings(typeof(TMappedClass))
                                             .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                             .Distinct()
                                             .SelectMany(x => x.IDProperties);
            Queries = [];
            SetupQueries();
        }

        /// <summary>
        /// The lock object
        /// </summary>
        private static readonly object _LockObject = new();

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.JoinsSave;

        /// <summary>
        /// Gets the identifier properties.
        /// </summary>
        /// <value>The identifier properties.</value>
        private IEnumerable<IIDProperty> IDProperties { get; }

        /// <summary>
        /// Gets or sets the queries.
        /// </summary>
        /// <value>The queries.</value>
        private ListMapping<string, QueryGeneratorData> Queries { get; }

        /// <summary>
        /// Generates the declarations needed for the query.
        /// </summary>
        /// <returns>The resulting declarations.</returns>
        public override IQuery[] GenerateDeclarations() => [new Query(AssociatedType, CommandType.Text, "", QueryType)];

        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="queryObject">The object to generate the queries from.</param>
        /// <param name="property">The property.</param>
        /// <returns>The resulting query</returns>
        public override IQuery[] GenerateQueries(TMappedClass queryObject, IClassProperty? property)
        {
            return property switch
            {
                IMapProperty TempMapProperty => MapProperty(TempMapProperty, queryObject),

                IManyToManyProperty Property => ManyToManyProperty(Property, queryObject),

                IManyToOneListProperty ManyToOne => ManyToOneProperty(ManyToOne, queryObject),

                IManyToOneProperty ManyToOne => ManyToOneProperty(ManyToOne, queryObject),

                _ => [],
            };
        }

        /// <summary>
        /// Generates the parameters.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <param name="property">The property.</param>
        /// <returns>The parameters</returns>
        private static IParameter?[] GenerateParameters(TMappedClass queryObject, IPropertyColumns property)
        {
            var ColumnInfos = property.GetColumnInfo();
            var ReturnValues = new IParameter?[ColumnInfos.Length];
            for (var X = 0; X < ColumnInfos.Length; ++X)
            {
                ReturnValues[X] = ColumnInfos[X].GetAsParameter(queryObject);
            }
            return ReturnValues;
        }

        /// <summary>
        /// Generates the parameters.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <param name="property">The property.</param>
        /// <param name="propertyItem">The property item.</param>
        /// <returns>The parameters</returns>
        private static IParameter?[] GenerateParameters(TMappedClass queryObject, IPropertyColumns property, object? propertyItem)
        {
            var ColumnInfos = property.GetColumnInfo();
            var ReturnValues = new IParameter?[ColumnInfos.Length];
            for (var X = 0; X < ColumnInfos.Length; ++X)
            {
                ReturnValues[X] = ColumnInfos[X].GetAsParameter(queryObject, propertyItem);
            }
            return ReturnValues;
        }

        /// <summary>
        /// Generates from clause.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private string GenerateFromClause(Utils.TreeNode<Type>? node)
        {
            if (node is null)
                return "";
            var Result = ObjectPool.Get();
            var Mapping = MappingInformation.Mappings[node.Data];
            for (int X = 0, NodeNodesCount = node.Nodes.Count; X < NodeNodesCount; X++)
            {
                var ParentNode = node.Nodes[X];
                var ParentMapping = MappingInformation.Mappings[ParentNode.Data];
                var TempIDProperties = ObjectPool.Get();
                var Separator = "";
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
                Result.AppendFormat(" INNER JOIN {0} ON {1}", GetTableName(ParentMapping), TempIDProperties);
                ObjectPool.Return(TempIDProperties);
                Result.Append(GenerateFromClause(ParentNode));
            }
            var ReturnValue = Result.ToString();

            ObjectPool.Return(Result);

            return ReturnValue;
        }

        /// <summary>
        /// Generates the join save query.
        /// </summary>
        /// <param name="foreignIDProperties">The foreign identifier properties.</param>
        /// <param name="mapProperty">The map property.</param>
        /// <returns></returns>
        private string GenerateJoinSaveQuery(IEnumerable<IIDProperty> foreignIDProperties, IMapProperty mapProperty)
        {
            var Builder = ObjectPool.Get();
            var WhereList = ObjectPool.Get();
            var ParametersList = ObjectPool.Get();
            var FromList = ObjectPool.Get();
            var Splitter2 = "";
            foreach (var ForeignID in foreignIDProperties.Distinct())
            {
                ParametersList
                    .Append(Splitter2)
                    .Append(GetTableName(mapProperty.ParentMapping))
                    .Append(".[")
                    .Append(ForeignID.ParentMapping?.TableName)
                    .Append(mapProperty.ParentMapping.Prefix)
                    .Append(mapProperty.Name)
                    .Append(mapProperty.ParentMapping.Suffix)
                    .Append(ForeignID.ColumnName)
                    .Append("] = @")
                    .Append(ForeignID.ParentMapping?.TableName)
                    .Append(mapProperty.ParentMapping.Prefix)
                    .Append(mapProperty.Name)
                    .Append(mapProperty.ParentMapping.Suffix)
                    .Append(ForeignID.ColumnName);
                Splitter2 = ", ";
            }
            Splitter2 = "";
            foreach (var IDProperty in IDProperties)
            {
                WhereList.Append(Splitter2).Append(GetColumnName(IDProperty)).Append(" = ").Append(GetParameterName(IDProperty));
                Splitter2 = " AND ";
            }
            FromList.Append(GetTableName(mapProperty.ParentMapping))
                .Append(GenerateFromClause(MappingInformation.TypeGraphs[mapProperty.ParentMapping.ObjectType]?.Root));

            Builder.AppendFormat("UPDATE {0} SET {1} FROM {2} WHERE {3};", GetTableName(mapProperty.ParentMapping), ParametersList, FromList, WhereList);
            var ReturnValue = Builder.ToString();
            ObjectPool.Return(Builder);
            ObjectPool.Return(WhereList);
            ObjectPool.Return(ParametersList);
            ObjectPool.Return(FromList);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the join save query.
        /// </summary>
        /// <param name="foreignIDProperties">The foreign identifier properties.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private string GenerateJoinSaveQuery(IEnumerable<IIDProperty> foreignIDProperties, IManyToManyProperty property)
        {
            var Builder = ObjectPool.Get();
            var PropertyNames = ObjectPool.Get();
            var PropertyValues = ObjectPool.Get();
            var ParametersList = ObjectPool.Get();
            var Splitter = "";
            foreach (var ForeignID in foreignIDProperties.Distinct())
            {
                PropertyNames.Append(Splitter).Append('[').Append(property.ParentMapping.SchemaName).Append("].[").Append(property.TableName).Append("].[").Append(ForeignID.ParentMapping.TableName).Append(ForeignID.ColumnName).Append(']');
                PropertyValues.Append(Splitter).Append('@').Append(ForeignID.ParentMapping.TableName).Append(ForeignID.ColumnName);
                Splitter = ", ";
            }
            var Prefix = "";
            if (IDProperties.Any(x => property.ForeignMapping.Any(tempMapping => x.ParentMapping == tempMapping)))
            {
                Prefix = "Parent_";
            }

            foreach (var IDProperty in IDProperties)
            {
                PropertyNames.Append(Splitter).Append('[').Append(property.ParentMapping.SchemaName).Append("].[").Append(property.TableName).Append("].[").Append(Prefix).Append(IDProperty.ParentMapping.TableName).Append(IDProperty.ColumnName).Append(']');
                PropertyValues.Append(Splitter).Append('@').Append(Prefix).Append(IDProperty.ParentMapping.TableName).Append(IDProperty.ColumnName);
                Splitter = ", ";
            }
            Builder.AppendFormat("INSERT INTO {0}({1}) VALUES ({2});", GetTableName(property), PropertyNames, PropertyValues);
            var ReturnValue = Builder.ToString();
            ObjectPool.Return(Builder);
            ObjectPool.Return(PropertyNames);
            ObjectPool.Return(PropertyValues);
            ObjectPool.Return(ParametersList);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the join save query.
        /// </summary>
        /// <param name="foreignIDProperties">The foreign identifier properties.</param>
        /// <param name="manyToOne">The many to one.</param>
        /// <returns>The queries</returns>
        private string GenerateJoinSaveQuery(IEnumerable<IIDProperty> foreignIDProperties, IManyToOneProperty manyToOne)
        {
            var Builder = ObjectPool.Get();
            var WhereList = ObjectPool.Get();
            var ParametersList = ObjectPool.Get();
            var FromList = ObjectPool.Get();
            string TableName;
            var ParentMapping = MappingInformation
                .GetChildMappings(manyToOne.ParentMapping.ObjectType)
                .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                .Distinct()
                .FirstOrDefault(x => x.IDProperties.Count > 0);
            if (manyToOne is IManyToOneListProperty)
            {
                foreach (var TempMapping in manyToOne.ForeignMapping)
                {
                    GenerateJoinSaveQueryMultiple(manyToOne, TempMapping, WhereList, ParametersList, FromList);
                    TableName = GetTableName(TempMapping);
                    Builder.AppendFormat("UPDATE {0} SET {1} FROM {2} WHERE {3};", TableName, ParametersList, FromList, WhereList);
                }
            }
            else
            {
                GenerateJoinSaveQuerySingle(foreignIDProperties, manyToOne, ParentMapping, WhereList, ParametersList, FromList);
                TableName = GetTableName(ParentMapping);
                Builder.AppendFormat("UPDATE {0} SET {1} FROM {2} WHERE {3};", TableName, ParametersList, FromList, WhereList);
            }
            var ReturnValue = Builder.ToString();
            ObjectPool.Return(Builder);
            ObjectPool.Return(WhereList);
            ObjectPool.Return(ParametersList);
            ObjectPool.Return(FromList);
            return ReturnValue;
        }

        /// <summary>
        /// Generates the join save query multiple.
        /// </summary>
        /// <param name="manyToOne">The many to one.</param>
        /// <param name="foreignMapping">The temporary mapping.</param>
        /// <param name="whereList">The where list.</param>
        /// <param name="parametersList">The parameters list.</param>
        /// <param name="fromList">From list.</param>
        private void GenerateJoinSaveQueryMultiple(IManyToOneProperty manyToOne,
                    IMapping foreignMapping,
                    StringBuilder whereList,
                    StringBuilder parametersList,
                    StringBuilder fromList)
        {
            var Splitter = "";
            foreach (var ForeignIDs in manyToOne.GetColumnInfo().Where(x => x.IsForeign).Distinct())
            {
                parametersList.Append(Splitter).Append('[').Append(ForeignIDs.SchemaName).Append("].[").Append(ForeignIDs.TableName).Append("].[").Append(ForeignIDs.ColumnName).Append("] = @").Append(ForeignIDs.ColumnName);
                Splitter = ", ";
            }
            Splitter = "";
            foreach (var ForeignIDs in manyToOne.GetColumnInfo().Where(x => !x.IsForeign))
            {
                whereList.Append(Splitter).Append('[').Append(ForeignIDs.SchemaName).Append("].[").Append(ForeignIDs.TableName).Append("].[").Append(ForeignIDs.ColumnName).Append("] = @").Append(ForeignIDs.ColumnName);
                Splitter = " AND ";
            }
            fromList.Append(GetTableName(foreignMapping));
            if (manyToOne.ForeignMapping != null)
                fromList.Append(GenerateFromClause(MappingInformation.TypeGraphs[foreignMapping.ObjectType]?.Root));
        }

        /// <summary>
        /// Generates the join save query single.
        /// </summary>
        /// <param name="foreignIDProperties">The foreign identifier properties.</param>
        /// <param name="manyToOne">The many to one.</param>
        /// <param name="parentMapping">The parent mapping.</param>
        /// <param name="whereList">The where list.</param>
        /// <param name="parametersList">The parameters list.</param>
        /// <param name="fromList">From list.</param>
        private void GenerateJoinSaveQuerySingle(IEnumerable<IIDProperty> foreignIDProperties,
                    IManyToOneProperty manyToOne,
                    IMapping? parentMapping,
                    StringBuilder whereList,
                    StringBuilder parametersList,
                    StringBuilder fromList)
        {
            var Splitter = "";
            foreach (var ForeignID in foreignIDProperties.Distinct())
            {
                parametersList.Append(Splitter).Append(GetTableName(parentMapping)
).Append(".["
).Append(manyToOne.ColumnName
).Append(ForeignID.ParentMapping.TableName
).Append(ForeignID.ColumnName
).Append("] = @"
).Append(manyToOne.ColumnName
).Append(ForeignID.ParentMapping.TableName
).Append(ForeignID.ColumnName);
                Splitter = ", ";
            }
            Splitter = "";
            foreach (var IDProperty in IDProperties)
            {
                whereList.Append(Splitter).Append(GetColumnName(IDProperty)).Append(" = ").Append(GetParameterName(IDProperty));
                Splitter = " AND ";
            }
            fromList.Append(GetTableName(parentMapping))
                .Append(GenerateFromClause(MappingInformation.TypeGraphs[parentMapping?.ObjectType!]?.Root));
        }

        /// <summary>
        /// Manies to many property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns></returns>
        private IQuery[] ManyToManyProperty(IManyToManyProperty property, TMappedClass queryObject)
        {
            if (!Queries.ContainsKey(property.Name))
            {
                lock (_LockObject)
                {
                    if (!Queries.ContainsKey(property.Name))
                    {
                        var ForeignMappings = MappingInformation.GetChildMappings(property.PropertyType)
                                            .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                            .Distinct();
                        var ForeignIDProperties = ForeignMappings.SelectMany(x => x.IDProperties);

                        foreach (var ForeignMapping in ForeignMappings)
                        {
                            Queries.Add(property.Name, new QueryGeneratorData(
                                MappingInformation.Mappings[ForeignMapping.ObjectType],
                                IDProperties,
                                GenerateJoinSaveQuery(ForeignIDProperties, property)
                            ));
                        }
                    }
                }
            }
            if (property.GetValue(queryObject) is not IEnumerable ItemList)
            {
                return [];
            }

            var ReturnValue = new List<IQuery>();
            foreach (var TempQuery in Queries[property.Name])
            {
                foreach (var Item in ItemList)
                {
                    ReturnValue.Add(new Query(TempQuery.AssociatedMapping.ObjectType,
                        CommandType.Text,
                        TempQuery.QueryText,
                        QueryType,
                        GenerateParameters(queryObject, property, Item)));
                }
            }
            return [.. ReturnValue];
        }

        /// <summary>
        /// Manies to one property.
        /// </summary>
        /// <param name="manyToOne">The many to one.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns></returns>
        private IQuery[] ManyToOneProperty(IManyToOneListProperty manyToOne, TMappedClass queryObject)
        {
            if (!Queries.ContainsKey(manyToOne.Name))
            {
                lock (_LockObject)
                {
                    if (!Queries.ContainsKey(manyToOne.Name))
                    {
                        var ForeignMappings = MappingInformation.GetChildMappings(manyToOne.PropertyType)
                                            .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                            .Distinct();
                        var ForeignIDProperties = ForeignMappings.SelectMany(x => x.IDProperties);

                        foreach (var ForeignMapping in ForeignMappings)
                        {
                            Queries.Add(manyToOne.Name, new QueryGeneratorData(
                                MappingInformation.Mappings[ForeignMapping.ObjectType],
                                IDProperties,
                                GenerateJoinSaveQuery(ForeignIDProperties, manyToOne)
                            ));
                        }
                    }
                }
            }
            if (manyToOne.GetValue(queryObject) is not IEnumerable ItemList)
            {
                return [];
            }

            var ReturnValue = new List<IQuery>();
            foreach (var TempQuery in Queries[manyToOne.Name])
            {
                foreach (var Item in ItemList)
                {
                    ReturnValue.Add(new Query(TempQuery.AssociatedMapping.ObjectType,
                        CommandType.Text,
                        TempQuery.QueryText,
                        QueryType,
                        GenerateParameters(queryObject, manyToOne, Item)));
                }
            }
            return [.. ReturnValue];
        }

        /// <summary>
        /// Manies to one property.
        /// </summary>
        /// <param name="manyToOne">The many to one.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns></returns>
        private IQuery[] ManyToOneProperty(IManyToOneProperty manyToOne, TMappedClass queryObject)
        {
            var ItemValue = manyToOne.GetValue(queryObject);

            if (!Queries.ContainsKey(manyToOne.Name))
            {
                lock (_LockObject)
                {
                    if (!Queries.ContainsKey(manyToOne.Name))
                    {
                        var ForeignMappings = MappingInformation.GetChildMappings(manyToOne.PropertyType)
                                            .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                            .Distinct();
                        var ForeignIDProperties = ForeignMappings
                                                    .SelectMany(x => x.IDProperties);

                        foreach (var ForeignMapping in ForeignMappings)
                        {
                            Queries.Add(manyToOne.Name, new QueryGeneratorData(
                                MappingInformation.Mappings[ForeignMapping.ObjectType],
                                IDProperties,
                                GenerateJoinSaveQuery(ForeignIDProperties, manyToOne)
                            ));
                        }
                    }
                }
            }
            var ReturnValue = new List<IQuery>();
            foreach (var TempQuery in Queries[manyToOne.Name])
            {
                ReturnValue.Add(new Query(TempQuery.AssociatedMapping.ObjectType,
                    CommandType.Text,
                    TempQuery.QueryText,
                    QueryType,
                    GenerateParameters(queryObject, manyToOne, ItemValue)));
            }
            return [.. ReturnValue];
        }

        /// <summary>
        /// Maps the property.
        /// </summary>
        /// <param name="mapProperty">The map property.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns></returns>
        private IQuery[] MapProperty(IMapProperty mapProperty, TMappedClass queryObject)
        {
            var ItemValue = mapProperty.GetValue(queryObject);

            if (!Queries.ContainsKey(mapProperty.Name))
            {
                lock (_LockObject)
                {
                    if (!Queries.ContainsKey(mapProperty.Name))
                    {
                        var ForeignMappings = MappingInformation.GetChildMappings(mapProperty.PropertyType)
                                            .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                            .Distinct();
                        var ForeignIDProperties = ForeignMappings
                                                    .SelectMany(x => x.IDProperties);

                        foreach (var ForeignMapping in ForeignMappings)
                        {
                            Queries.Add(mapProperty.Name, new QueryGeneratorData(
                                MappingInformation.Mappings[ForeignMapping.ObjectType],
                                IDProperties,
                                GenerateJoinSaveQuery(ForeignIDProperties, mapProperty)
                            ));
                        }
                    }
                }
            }
            var ReturnValue = new List<IQuery>();
            foreach (var TempQuery in Queries[mapProperty.Name])
            {
                ReturnValue.Add(new Query(TempQuery.AssociatedMapping.ObjectType,
                    CommandType.Text,
                    TempQuery.QueryText,
                    QueryType,
                    GenerateParameters(queryObject, mapProperty)));
            }
            return [.. ReturnValue];
        }

        /// <summary>
        /// Sets up the queries.
        /// </summary>
        private void SetupQueries()
        {
            foreach (var ParentMapping in MappingInformation.GetParentMapping(typeof(TMappedClass)))
            {
                foreach (var Property in ParentMapping.ManyToManyProperties)
                {
                    ManyToManyProperty(Property, default!);
                }
                foreach (var Property in ParentMapping.ManyToOneProperties)
                {
                    switch (Property)
                    {
                        case IManyToOneListProperty ManyToOne:
                            ManyToOneProperty(ManyToOne, default!);
                            break;

                        case IManyToOneProperty ManyToOne:
                            ManyToOneProperty(ManyToOne, default!);
                            break;
                    }
                }
                foreach (var Property in ParentMapping.MapProperties)
                {
                    MapProperty(Property, default!);
                }
            }
        }
    }
}