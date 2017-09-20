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
using Inflatable.QueryProvider.BaseClasses;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators.HelperClasses;
using SQLHelper.HelperClasses.Interfaces;
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
    /// <seealso cref="BaseClasses.PropertyQueryGeneratorBaseClass{TMappedClass}"/>
    public class SavePropertiesQuery<TMappedClass> : PropertyQueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SavePropertiesQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">Mapping information</param>
        public SavePropertiesQuery(MappingSource mappingInformation)
            : base(mappingInformation)
        {
            IDProperties = MappingInformation.GetChildMappings(typeof(TMappedClass))
                                             .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                             .Distinct()
                                             .SelectMany(x => x.IDProperties);
            Queries = new Dictionary<string, List<QueryGeneratorData>>();
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.JoinsSave;

        /// <summary>
        /// Gets the identifier properties.
        /// </summary>
        /// <value>The identifier properties.</value>
        private IEnumerable<IIDProperty> IDProperties { get; set; }

        /// <summary>
        /// Gets or sets the queries.
        /// </summary>
        /// <value>The queries.</value>
        private IDictionary<string, List<QueryGeneratorData>> Queries { get; set; }

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
            switch (property)
            {
                case IMapProperty TempMapProperty:
                    return MapProperty(TempMapProperty, queryObject);

                case IManyToManyProperty Property:
                    return ManyToManyProperty(Property, queryObject);

                case IManyToOneListProperty ManyToOne:
                    return ManyToOneProperty(ManyToOne, queryObject);

                case IManyToOneProperty ManyToOne:
                    return ManyToOneProperty(ManyToOne, queryObject);
            }

            return new IQuery[0];
        }

        /// <summary>
        /// Generates the join save query.
        /// </summary>
        /// <param name="foreignIDProperties">The foreign identifier properties.</param>
        /// <param name="mapProperty">The map property.</param>
        /// <returns></returns>
        private string GenerateJoinSaveQuery(IEnumerable<IIDProperty> foreignIDProperties, IMapProperty mapProperty)
        {
            StringBuilder Builder = new StringBuilder();
            StringBuilder WhereList = new StringBuilder();
            StringBuilder ParametersList = new StringBuilder();
            string Splitter2 = "";
            foreach (var ForeignID in foreignIDProperties)
            {
                ParametersList.Append(Splitter2).Append(GetTableName(mapProperty.ParentMapping)
                                                                        + ".["
                                                                        + mapProperty.ForeignMapping.TableName
                                                                        + mapProperty.ParentMapping.Prefix
                                                                        + mapProperty.Name
                                                                        + mapProperty.ParentMapping.Suffix
                                                                        + ForeignID.ColumnName
                                                                        + "] = @"
                                                                        + mapProperty.ForeignMapping.TableName
                                                                        + mapProperty.ParentMapping.Prefix
                                                                        + mapProperty.Name
                                                                        + mapProperty.ParentMapping.Suffix
                                                                        + ForeignID.ColumnName);
                Splitter2 = " AND ";
            }
            Splitter2 = "";
            foreach (var IDProperty in IDProperties)
            {
                WhereList.Append(Splitter2).Append(GetColumnName(IDProperty) + " = " + GetParameterName(IDProperty));
                Splitter2 = " AND ";
            }
            Builder.AppendFormat("UPDATE {0} SET {1} WHERE {2};", GetTableName(mapProperty.ParentMapping), ParametersList, WhereList);
            return Builder.ToString();
        }

        /// <summary>
        /// Generates the join save query.
        /// </summary>
        /// <param name="foreignIDProperties">The foreign identifier properties.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        private string GenerateJoinSaveQuery(IEnumerable<IIDProperty> foreignIDProperties, IManyToManyProperty property)
        {
            StringBuilder Builder = new StringBuilder();
            StringBuilder PropertyNames = new StringBuilder();
            StringBuilder PropertyValues = new StringBuilder();
            StringBuilder ParametersList = new StringBuilder();
            string Splitter = "";
            string Splitter2 = "";
            foreach (var ForeignID in foreignIDProperties)
            {
                PropertyNames.Append(Splitter).Append("[" + property.ParentMapping.SchemaName + "].[" + property.TableName + "].[" + ForeignID.ParentMapping.TableName + ForeignID.ColumnName + "]");
                PropertyValues.Append(Splitter).Append("@" + ForeignID.ParentMapping.TableName + ForeignID.ColumnName);
                ParametersList.Append(Splitter2).Append("[" + property.ParentMapping.SchemaName + "].[" + property.TableName + "].[" + ForeignID.ParentMapping.TableName + ForeignID.ColumnName + "] = @" + ForeignID.ParentMapping.TableName + ForeignID.ColumnName);
                Splitter = ",";
                Splitter2 = " AND ";
            }
            foreach (var IDProperty in IDProperties)
            {
                PropertyNames.Append(Splitter).Append("[" + property.ParentMapping.SchemaName + "].[" + property.TableName + "].[" + IDProperty.ParentMapping.TableName + IDProperty.ColumnName + "]");
                PropertyValues.Append(Splitter).Append("@" + IDProperty.ParentMapping.TableName + IDProperty.ColumnName);
                ParametersList.Append(Splitter2).Append("[" + property.ParentMapping.SchemaName + "].[" + property.TableName + "].[" + IDProperty.ParentMapping.TableName + IDProperty.ColumnName + "] = @" + IDProperty.ParentMapping.TableName + IDProperty.ColumnName);
                Splitter = ",";
                Splitter2 = " AND ";
            }
            Builder.AppendFormat("IF NOT EXISTS (SELECT * FROM {0} WHERE {3}) BEGIN INSERT INTO {0}({1}) VALUES ({2}) END;", GetTableName(property), PropertyNames, PropertyValues, ParametersList);
            return Builder.ToString();
        }

        private string GenerateJoinSaveQuery(IEnumerable<IIDProperty> foreignIDProperties, IManyToOneProperty manyToOne)
        {
            StringBuilder Builder = new StringBuilder();
            StringBuilder WhereList = new StringBuilder();
            StringBuilder ParametersList = new StringBuilder();
            string TableName = "";
            if (manyToOne is IManyToOneListProperty)
            {
                GenerateJoinSaveQueryMultiple(foreignIDProperties, manyToOne, WhereList, ParametersList);
                TableName = GetTableName(manyToOne.ForeignMapping);
            }
            else
            {
                GenerateJoinSaveQuerySingle(foreignIDProperties, manyToOne, WhereList, ParametersList);
                TableName = GetTableName(manyToOne.ParentMapping);
            }
            Builder.AppendFormat("UPDATE {0} SET {1} WHERE {2};", TableName, ParametersList, WhereList);
            return Builder.ToString();
        }

        private void GenerateJoinSaveQueryMultiple(IEnumerable<IIDProperty> foreignIDProperties,
            IManyToOneProperty manyToOne,
            StringBuilder whereList,
            StringBuilder parametersList)
        {
            string Splitter = "";
            foreach (var ForeignID in IDProperties)
            {
                parametersList.Append(Splitter).Append(GetTableName(manyToOne.ForeignMapping)
                                                                        + ".["
                                                                        + manyToOne.ColumnName
                                                                        + manyToOne.ParentMapping.TableName
                                                                        + ForeignID.ColumnName
                                                                        + "] = @"
                                                                        + manyToOne.ColumnName
                                                                        + manyToOne.ParentMapping.TableName
                                                                        + ForeignID.ColumnName);
                Splitter = " AND ";
            }
            Splitter = "";
            foreach (var IDProperty in foreignIDProperties)
            {
                whereList.Append(Splitter).Append(GetColumnName(IDProperty) + " = " + GetParameterName(IDProperty));
                Splitter = " AND ";
            }
        }

        private void GenerateJoinSaveQuerySingle(IEnumerable<IIDProperty> foreignIDProperties,
            IManyToOneProperty manyToOne,
            StringBuilder whereList,
            StringBuilder parametersList)
        {
            string Splitter = "";
            foreach (var ForeignID in foreignIDProperties)
            {
                parametersList.Append(Splitter).Append(GetTableName(manyToOne.ParentMapping)
                                                                        + ".["
                                                                        + manyToOne.ColumnName
                                                                        + manyToOne.ForeignMapping.TableName
                                                                        + ForeignID.ColumnName
                                                                        + "] = @"
                                                                        + manyToOne.ColumnName
                                                                        + manyToOne.ForeignMapping.TableName
                                                                        + ForeignID.ColumnName);
                Splitter = " AND ";
            }
            Splitter = "";
            foreach (var IDProperty in IDProperties)
            {
                whereList.Append(Splitter).Append(GetColumnName(IDProperty) + " = " + GetParameterName(IDProperty));
                Splitter = " AND ";
            }
        }

        /// <summary>
        /// Generates the parameters.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <param name="mapProperty">The map property.</param>
        /// <returns></returns>
        private IParameter[] GenerateParameters(TMappedClass queryObject, IMapProperty mapProperty)
        {
            List<IParameter> ReturnValue = new List<IParameter>();
            ReturnValue.AddRange(mapProperty.GetAsParameter(queryObject));
            return ReturnValue.ToArray();
        }

        /// <summary>
        /// Generates the parameters.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <param name="property">The property.</param>
        /// <param name="propertyItem">The property item.</param>
        /// <returns></returns>
        private IParameter[] GenerateParameters(TMappedClass queryObject, IManyToManyProperty property, object propertyItem)
        {
            List<IParameter> ReturnValues = new List<IParameter>();
            ReturnValues.AddRange(property.GetAsParameter(queryObject, propertyItem));
            return ReturnValues.ToArray();
        }

        private IParameter[] GenerateParameters(TMappedClass queryObject, IManyToOneProperty manyToOne, object propertyItem)
        {
            List<IParameter> ReturnValues = new List<IParameter>();
            ReturnValues.AddRange(manyToOne.GetAsParameter(queryObject, propertyItem));
            return ReturnValues.ToArray();
        }

        /// <summary>
        /// Manies to many property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="queryObject">The query object.</param>
        /// <returns></returns>
        private IQuery[] ManyToManyProperty(IManyToManyProperty property, TMappedClass queryObject)
        {
            var ItemList = property.GetValue(queryObject) as IEnumerable;

            if (!Queries.ContainsKey(property.Name))
            {
                var ForeignIDProperties = MappingInformation.GetChildMappings(property.PropertyType)
                                            .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                            .Distinct()
                                            .SelectMany(x => x.IDProperties);

                Queries.Add(property.Name, new List<QueryGeneratorData>());
                Queries[property.Name].Add(new QueryGeneratorData
                {
                    AssociatedMapping = MappingInformation.Mappings[property.PropertyType],
                    IDProperties = IDProperties,
                    QueryText = GenerateJoinSaveQuery(ForeignIDProperties, property)
                });
            }
            if (ItemList == null)
                return new IQuery[0];

            List<IQuery> ReturnValue = new List<IQuery>();
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
            return ReturnValue.ToArray();
        }

        private IQuery[] ManyToOneProperty(IManyToOneListProperty manyToOne, TMappedClass queryObject)
        {
            var ItemList = manyToOne.GetValue(queryObject) as IEnumerable;

            if (!Queries.ContainsKey(manyToOne.Name))
            {
                var ForeignIDProperties = MappingInformation.GetChildMappings(manyToOne.PropertyType)
                                            .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                            .Distinct()
                                            .SelectMany(x => x.IDProperties);

                Queries.Add(manyToOne.Name, new List<QueryGeneratorData>());
                Queries[manyToOne.Name].Add(new QueryGeneratorData
                {
                    AssociatedMapping = MappingInformation.Mappings[manyToOne.PropertyType],
                    IDProperties = IDProperties,
                    QueryText = GenerateJoinSaveQuery(ForeignIDProperties, manyToOne)
                });
            }
            if (ItemList == null)
                return new IQuery[0];

            List<IQuery> ReturnValue = new List<IQuery>();
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
            return ReturnValue.ToArray();
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
                var ForeignIDProperties = MappingInformation.GetChildMappings(manyToOne.PropertyType)
                                            .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                            .Distinct()
                                            .SelectMany(x => x.IDProperties);

                Queries.Add(manyToOne.Name, new List<QueryGeneratorData>());
                Queries[manyToOne.Name].Add(new QueryGeneratorData
                {
                    AssociatedMapping = MappingInformation.Mappings[manyToOne.PropertyType],
                    IDProperties = IDProperties,
                    QueryText = GenerateJoinSaveQuery(ForeignIDProperties, manyToOne)
                });
            }
            List<IQuery> ReturnValue = new List<IQuery>();
            foreach (var TempQuery in Queries[manyToOne.Name])
            {
                ReturnValue.Add(new Query(TempQuery.AssociatedMapping.ObjectType,
                    CommandType.Text,
                    TempQuery.QueryText,
                    QueryType,
                    GenerateParameters(queryObject, manyToOne, ItemValue)));
            }
            return ReturnValue.ToArray();
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
                var ForeignIDProperties = MappingInformation.GetChildMappings(mapProperty.PropertyType)
                                            .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                            .Distinct()
                                            .SelectMany(x => x.IDProperties);

                Queries.Add(mapProperty.Name, new List<QueryGeneratorData>());
                Queries[mapProperty.Name].Add(new QueryGeneratorData
                {
                    AssociatedMapping = MappingInformation.Mappings[mapProperty.PropertyType],
                    IDProperties = IDProperties,
                    QueryText = GenerateJoinSaveQuery(ForeignIDProperties, mapProperty)
                });
            }
            List<IQuery> ReturnValue = new List<IQuery>();
            foreach (var TempQuery in Queries[mapProperty.Name])
            {
                ReturnValue.Add(new Query(TempQuery.AssociatedMapping.ObjectType,
                    CommandType.Text,
                    TempQuery.QueryText,
                    QueryType,
                    GenerateParameters(queryObject, mapProperty)));
            }
            return ReturnValue.ToArray();
        }
    }
}