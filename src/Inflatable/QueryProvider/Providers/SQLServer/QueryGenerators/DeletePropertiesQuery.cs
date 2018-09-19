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
using SQLHelperDB.HelperClasses;
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
    /// Delete properties query
    /// </summary>
    /// <typeparam name="TMappedClass">The type of the mapped class.</typeparam>
    /// <seealso cref="BaseClasses.PropertyQueryGeneratorBaseClass{TMappedClass}"/>
    public class DeletePropertiesQuery<TMappedClass> : PropertyQueryGeneratorBaseClass<TMappedClass>
        where TMappedClass : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SavePropertiesQuery{TMappedClass}"/> class.
        /// </summary>
        /// <param name="mappingInformation">Mapping information</param>
        public DeletePropertiesQuery(MappingSource mappingInformation)
            : base(mappingInformation)
        {
            IDProperties = MappingInformation.GetChildMappings(typeof(TMappedClass))
                                             .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                             .Distinct()
                                             .SelectMany(x => x.IDProperties);
            Queries = new ListMapping<string, QueryGeneratorData>();
        }

        /// <summary>
        /// Gets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public override QueryType QueryType => QueryType.JoinsDelete;

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
            if (property is IManyToManyProperty Property)
            {
                return ManyToManyProperty(Property, queryObject);
            }

            return new IQuery[0];
        }

        /// <summary>
        /// Generates the join delete query.
        /// </summary>
        /// <param name="foreignIDProperties">The foreign identifier properties.</param>
        /// <param name="property">The property.</param>
        /// <param name="itemCount">The item count.</param>
        /// <returns></returns>
        private string GenerateJoinDeleteQuery(IEnumerable<IIDProperty> foreignIDProperties, IManyToManyProperty property, int itemCount)
        {
            var Builder = new StringBuilder();
            var PropertyNames = new StringBuilder();
            var PropertyValues = new StringBuilder();
            var ParametersList = new StringBuilder();
            var ParentMappings = MappingInformation.GetChildMappings(property.ParentMapping.ObjectType).SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType)).Distinct();
            var ParentWithID = ParentMappings.FirstOrDefault(x => x.IDProperties.Count > 0);
            string Prefix = "";
            if (ParentWithID == property.ForeignMapping)
            {
                Prefix = "Parent_";
            }

            string Splitter2 = "";
            foreach (var IDProperty in IDProperties)
            {
                ParametersList.Append(Splitter2).Append("[").Append(property.ParentMapping.SchemaName).Append("].[").Append(property.TableName).Append("].[").Append(Prefix).Append(IDProperty.ParentMapping.TableName).Append(IDProperty.ColumnName).Append("] = @").Append(Prefix).Append(IDProperty.ParentMapping.TableName).Append(IDProperty.ColumnName);
                Splitter2 = " AND ";
            }
            Builder.AppendFormat("DELETE FROM {0} WHERE {1};", GetTableName(property), ParametersList);
            return Builder.ToString();
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
            var ItemList = propertyItem as IEnumerable;
            var ReturnValues = new List<IParameter>();
            var ParentMappings = MappingInformation.GetChildMappings(property.ParentMapping.ObjectType).SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType)).Distinct();
            var ParentWithID = ParentMappings.FirstOrDefault(x => x.IDProperties.Count > 0);
            string Prefix = "";
            if (ParentWithID == property.ForeignMapping)
            {
                Prefix = "Parent_";
            }

            var ParentIDs = ParentMappings.SelectMany(x => x.IDProperties);
            var ForeignIDs = MappingInformation.GetParentMapping(property.PropertyType).SelectMany(x => x.IDProperties);
            ReturnValues.AddRange(ParentIDs.ForEach<IIDProperty, IParameter>(x =>
            {
                var Value = x.GetColumnInfo()[0].GetValue(queryObject);
                if (x.PropertyType == typeof(string))
                {
                    var TempParameter = Value as string;
                    return new StringParameter(Prefix + x.ParentMapping.TableName + x.ColumnName,
                        TempParameter);
                }
                return new Parameter<object>(Prefix + x.ParentMapping.TableName + x.ColumnName,
                    x.PropertyType.To<Type, SqlDbType>(),
                    Value);
            }));
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

            var ForeignIDProperties = MappingInformation.GetChildMappings(property.PropertyType)
                                            .SelectMany(x => MappingInformation.GetParentMapping(x.ObjectType))
                                            .Distinct()
                                            .SelectMany(x => x.IDProperties);

            var ReturnValue = new List<IQuery>
            {
                new Query(property.PropertyType,
                CommandType.Text,
                GenerateJoinDeleteQuery(ForeignIDProperties, property, ItemList.Count()),
                QueryType,
                GenerateParameters(queryObject, property, ItemList))
            };
            return ReturnValue.ToArray();
        }
    }

    /// <summary>
    /// Extension helpers
    /// </summary>
    /// <seealso cref="PropertyQueryGeneratorBaseClass{TMappedClass}"/>
    internal static class ExtensionHelpers
    {
        /// <summary>
        /// Counts the specified list.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static int Count(this IEnumerable list)
        {
            if (list == null)
            {
                return 0;
            }

            int FinalCount = 0;
            foreach (var Item in list)
            {
                ++FinalCount;
            }
            return FinalCount;
        }
    }
}