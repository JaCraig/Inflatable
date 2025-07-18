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

using Inflatable.ClassMapper.Default;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Microsoft.Extensions.Logging;
using SQLHelperDB.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Inflatable.BaseClasses
{
    /// <summary>
    /// Mapping base class
    /// </summary>
    /// <typeparam name="TClassType">The type of the lass type.</typeparam>
    /// <typeparam name="TDatabaseType">The type of the atabase type.</typeparam>
    /// <seealso cref="IMapping"/>
    /// <seealso cref="IMapping{TClassType}"/>
    public abstract class MappingBaseClass<TClassType, TDatabaseType> : IMapping, IMapping<TClassType>
        where TClassType : class
        where TDatabaseType : IDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingBaseClass{TClassType,
        /// TDatabaseType}"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="suffix">The suffix.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="order">The order.</param>
        /// <param name="merge">if set to <c>true</c> [merge] this mapping when possible.</param>
        protected MappingBaseClass(string tableName = "", string schemaName = "dbo", string suffix = "_", string prefix = "",
            int order = 10, bool merge = false)
        {
            SchemaName = schemaName;
            AutoIDProperties = [];
            IDProperties = [];
            Order = order;
            Prefix = prefix ?? "";
            Queries = new Queries();
            ReferenceProperties = [];
            Suffix = suffix ?? "";
            TableName = string.IsNullOrEmpty(tableName) ? Prefix + ObjectType.Name + Suffix : tableName;
            Merge = merge;
            MapProperties = [];
            ManyToManyProperties = [];
            ManyToOneProperties = [];
            _HashCode = TableName.GetHashCode(StringComparison.InvariantCulture) * DatabaseConfigType.GetHashCode() % int.MaxValue;
            _ToString = ObjectType.Name;
        }

        /// <summary>
        /// The hash code
        /// </summary>
        private readonly int _HashCode;

        /// <summary>
        /// To string
        /// </summary>
        private readonly string _ToString;

        /// <summary>
        /// Gets the automatic identifier properties.
        /// </summary>
        /// <value>The automatic identifier properties.</value>
        public List<IAutoIDProperty> AutoIDProperties { get; }

        /// <summary>
        /// Gets the type of the database configuration.
        /// </summary>
        /// <value>The type of the database configuration.</value>
        public Type DatabaseConfigType => typeof(TDatabaseType);

        /// <summary>
        /// ID properties
        /// </summary>
        /// <value>The identifier properties.</value>
        public List<IIDProperty> IDProperties { get; }

        /// <summary>
        /// Gets the many to many properties.
        /// </summary>
        /// <value>The many to many properties.</value>
        public List<IManyToManyProperty> ManyToManyProperties { get; }

        /// <summary>
        /// Gets the many to one properties.
        /// </summary>
        /// <value>The many to one properties.</value>
        public List<IManyToOneProperty> ManyToOneProperties { get; }

        /// <summary>
        /// Gets the map properties.
        /// </summary>
        /// <value>The map properties.</value>
        public List<IMapProperty> MapProperties { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Inflatable.Interfaces.IMapping"/>
        /// should be merged.
        /// </summary>
        /// <value><c>true</c> if merge this instance; otherwise, <c>false</c>.</value>
        public bool Merge { get; }

        /// <summary>
        /// The object type associated with the mapping
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType => typeof(TClassType);

        /// <summary>
        /// Order that the mappings are initialized
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; }

        /// <summary>
        /// Prefix used for defining properties/table name
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix { get; }

        /// <summary>
        /// Gets the queries.
        /// </summary>
        /// <value>The queries.</value>
        public IQueries Queries { get; }

        /// <summary>
        /// Reference Properties list
        /// </summary>
        /// <value>The reference properties.</value>
        public List<IProperty> ReferenceProperties { get; }

        /// <summary>
        /// Gets the name of the schema.
        /// </summary>
        /// <value>The name of the schema.</value>
        public string SchemaName { get; }

        /// <summary>
        /// Suffix used for defining properties/table name
        /// </summary>
        /// <value>The suffix.</value>
        public string Suffix { get; }

        /// <summary>
        /// Table name
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; }

        /// <summary>
        /// Determines if the two items are not equal
        /// </summary>
        /// <param name="item1">Item 1</param>
        /// <param name="item2">Item 2</param>
        /// <returns>True if they are not equal, false otherwise</returns>
        public static bool operator !=(MappingBaseClass<TClassType, TDatabaseType>? item1, MappingBaseClass<TClassType, TDatabaseType>? item2)
        {
            return !(item1 == item2);
        }

        /// <summary>
        /// Determines if the two items are equal
        /// </summary>
        /// <param name="item1">Item 1</param>
        /// <param name="item2">Item 2</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public static bool operator ==(MappingBaseClass<TClassType, TDatabaseType>? item1, MappingBaseClass<TClassType, TDatabaseType>? item2)
        {
            return item1?.Equals(item2) ?? item2 is null;
        }

        /// <summary>
        /// Adds an automatic key.
        /// </summary>
        public void AddAutoKey() => AutoIDProperties.Add(new AutoID(Prefix + "ID" + Suffix, this));

        /// <summary>
        /// Determines whether the mapping contains a property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><c>true</c> if the mapping contains the specified property; otherwise, <c>false</c>.</returns>
        public bool ContainsProperty(string propertyName)
        {
            return IDProperties.Any(x => x.Name == propertyName)
                    || ReferenceProperties.Any(x => x.Name == propertyName)
                    || MapProperties.Any(x => x.Name == propertyName)
                    || ManyToManyProperties.Any(x => x.Name == propertyName)
                    || ManyToOneProperties.Any(x => x.Name == propertyName);
        }

        /// <summary>
        /// Copies the specified mapping.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        public void Copy(IMapping mapping)
        {
            if (mapping is null)
                return;
            foreach (var Prop in mapping.IDProperties.Where(x => !IDProperties.Any(y => y.Name == x.Name)))
            {
                CopyProperty(Prop);
            }
            foreach (var Prop in mapping.ReferenceProperties.Where(x => !ReferenceProperties.Any(y => y.Name == x.Name)))
            {
                CopyProperty(Prop);
            }
            foreach (var Prop in mapping.MapProperties.Where(x => !MapProperties.Any(y => y.Name == x.Name)))
            {
                CopyProperty(Prop);
            }
            foreach (var Prop in mapping.ManyToManyProperties.Where(x => !ManyToManyProperties.Any(y => y.Name == x.Name)))
            {
                CopyProperty(Prop);
            }
            foreach (var Prop in mapping.ManyToOneProperties.Where(x => !ManyToOneProperties.Any(y => y.Name == x.Name)))
            {
                CopyProperty(Prop);
            }
        }

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        public void CopyProperty(IIDProperty prop)
        {
            if (prop is null)
                return;
            IDProperties.Add(prop.Convert<TClassType>(this));
        }

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        public void CopyProperty(IProperty prop)
        {
            if (prop is null)
                return;
            ReferenceProperties.Add(prop.Convert<TClassType>(this));
        }

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        public void CopyProperty(IMapProperty prop)
        {
            if (prop is null)
                return;
            MapProperties.Add(prop.Convert<TClassType>(this));
        }

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        public void CopyProperty(IManyToOneProperty prop)
        {
            if (prop is null)
                return;
            ManyToOneProperties.Add(prop.Convert<TClassType>(this));
        }

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        public void CopyProperty(IManyToManyProperty prop)
        {
            if (prop is null)
                return;
            ManyToManyProperties.Add(prop.Convert<TClassType>(this));
        }

        /// <summary>
        /// determines if the mappings are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is MappingBaseClass<TClassType, TDatabaseType> Object2
                && string.Equals(TableName, Object2.TableName, StringComparison.Ordinal)
                && DatabaseConfigType == Object2.DatabaseConfigType;
        }

        /// <summary>
        /// Gets the name of the column based on property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The column name.</returns>
        public string GetColumnName(string propertyName)
        {
            var IDProperty = IDProperties.Find(x => x.Name == propertyName);
            if (IDProperty is not null)
            {
                return "[" + SchemaName + "].[" + TableName + "].[" + IDProperty.ColumnName + "]";
            }

            var ReferenceProperty = ReferenceProperties.Find(x => x.Name == propertyName);
            if (ReferenceProperty is not null)
            {
                return "[" + SchemaName + "].[" + TableName + "].[" + ReferenceProperty.ColumnName + "]";
            }

            return "";
        }

        /// <summary>
        /// Gets the mapping's hash code
        /// </summary>
        /// <returns>Hash code for the mapping</returns>
        public override int GetHashCode() => _HashCode;

        /// <summary>
        /// Declares a property as an ID
        /// </summary>
        /// <typeparam name="TDataType">Data type</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>the ID object</returns>
        /// <exception cref="ArgumentNullException">expression</exception>
        public ID<TClassType, TDataType?> ID<TDataType>(System.Linq.Expressions.Expression<Func<TClassType, TDataType?>> expression)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var ReturnValue = new ID<TClassType, TDataType?>(expression, this);
            IDProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Sets a property as a many to many type.
        /// </summary>
        /// <typeparam name="TDataType">The type of the data type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The many to many object</returns>
        public ManyToMany<TClassType, TDataType> ManyToMany<TDataType>(System.Linq.Expressions.Expression<Func<TClassType, IList<TDataType>?>> expression)
            where TDataType : class
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var ReturnValue = new ManyToMany<TClassType, TDataType>(expression, this);
            ManyToManyProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Sets a property as a many to one type.
        /// </summary>
        /// <typeparam name="TDataType">The type of the data type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The many to many object</returns>
        public ManyToOneMany<TClassType, TDataType> ManyToOne<TDataType>(System.Linq.Expressions.Expression<Func<TClassType, IList<TDataType>?>> expression)
            where TDataType : class
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var ReturnValue = new ManyToOneMany<TClassType, TDataType>(expression, this);
            ManyToOneProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Sets a property as a many to one type.
        /// </summary>
        /// <typeparam name="TDataType">The type of the data type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The many to many object</returns>
        public ManyToOneSingle<TClassType, TDataType> ManyToOne<TDataType>(System.Linq.Expressions.Expression<Func<TClassType, TDataType?>> expression)
            where TDataType : class
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var ReturnValue = new ManyToOneSingle<TClassType, TDataType>(expression, this);
            ManyToOneProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Sets a property as a map type.
        /// </summary>
        /// <typeparam name="TDataType">The type of the data type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The map object</returns>
        /// <exception cref="ArgumentNullException">expression</exception>
        public Map<TClassType, TDataType> Map<TDataType>(System.Linq.Expressions.Expression<Func<TClassType, TDataType?>> expression)
            where TDataType : class
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var ReturnValue = new Map<TClassType, TDataType>(expression, this);
            MapProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Reduces this instance and removes duplicate properties
        /// </summary>
        /// <param name="logger">The logger.</param>
        public void Reduce(ILogger? logger)
        {
            var IsDebug = logger?.IsEnabled(LogLevel.Debug) ?? false;
            for (var X = 0; X < IDProperties.Count; ++X)
            {
                var IDProperty1 = IDProperties[X];
                for (var Y = X + 1; Y < IDProperties.Count; ++Y)
                {
                    var IDProperty2 = IDProperties[Y];
                    if (IDProperty1 == IDProperty2)
                    {
                        if (IsDebug)
                            logger?.LogDebug("Found duplicate ID and removing {propertyName} from mapping {objectTypeName}", IDProperty2.Name, ObjectType.Name);
                        IDProperties.Remove(IDProperty2);
                        --Y;
                    }
                }
            }
            for (var X = 0; X < ReferenceProperties.Count; ++X)
            {
                var ReferenceProperty1 = ReferenceProperties[X];
                for (var Y = X + 1; Y < ReferenceProperties.Count; ++Y)
                {
                    var ReferenceProperty2 = ReferenceProperties[Y];
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        if (IsDebug)
                            logger?.LogDebug("Found duplicate reference and removing {propertyName} from mapping {objectTypeName}", ReferenceProperty2.Name, ObjectType.Name);
                        ReferenceProperties.Remove(ReferenceProperty2);
                        --Y;
                    }
                }
            }
            for (var X = 0; X < MapProperties.Count; ++X)
            {
                var ReferenceProperty1 = MapProperties[X];
                for (var Y = X + 1; Y < MapProperties.Count; ++Y)
                {
                    var ReferenceProperty2 = MapProperties[Y];
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        if (IsDebug)
                            logger?.LogDebug("Found duplicate map and removing {propertyName} from mapping {objectTypeName}", ReferenceProperty2.Name, ObjectType.Name);
                        MapProperties.Remove(ReferenceProperty2);
                        --Y;
                    }
                }
            }
            for (var X = 0; X < ManyToManyProperties.Count; ++X)
            {
                var ReferenceProperty1 = ManyToManyProperties[X];
                for (var Y = X + 1; Y < ManyToManyProperties.Count; ++Y)
                {
                    var ReferenceProperty2 = ManyToManyProperties[Y];
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        if (IsDebug)
                            logger?.LogDebug("Found duplicate many to many and removing {propertyName} from mapping {objectTypeName}", ReferenceProperty2.Name, ObjectType.Name);
                        ManyToManyProperties.Remove(ReferenceProperty2);
                        --Y;
                    }
                }
            }

            for (var X = 0; X < ManyToOneProperties.Count; ++X)
            {
                var ReferenceProperty1 = ManyToOneProperties[X];
                for (var Y = X + 1; Y < ManyToOneProperties.Count; ++Y)
                {
                    var ReferenceProperty2 = ManyToOneProperties[Y];
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        if (IsDebug)
                            logger?.LogDebug("Found duplicate many to one and removing {propertyName} from mapping {objectTypeName}", ReferenceProperty2.Name, ObjectType.Name);
                        ManyToOneProperties.Remove(ReferenceProperty2);
                        --Y;
                    }
                }
            }
        }

        /// <summary>
        /// Reduces this instance based on parent mapping properties.
        /// </summary>
        /// <param name="parentMapping">The parent mapping.</param>
        /// <param name="logger">The logger.</param>
        public void Reduce(IMapping parentMapping, ILogger? logger)
        {
            if (parentMapping is null)
                return;
            var IsDebug = logger?.IsEnabled(LogLevel.Debug) ?? false;
            for (var X = 0; X < parentMapping.ReferenceProperties.Count; ++X)
            {
                var ReferenceProperty1 = parentMapping.ReferenceProperties[X];
                for (var Y = 0; Y < ReferenceProperties.Count; ++Y)
                {
                    var ReferenceProperty2 = ReferenceProperties[Y];
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        if (IsDebug)
                            logger?.LogDebug("Found duplicate reference and removing {propertyName} from mapping {objectTypeName}", ReferenceProperty2.Name, ObjectType.Name);
                        ReferenceProperties.Remove(ReferenceProperty2);
                        --Y;
                    }
                }
            }
            for (var X = 0; X < parentMapping.MapProperties.Count; ++X)
            {
                var ReferenceProperty1 = parentMapping.MapProperties[X];
                for (var Y = X + 1; Y < MapProperties.Count; ++Y)
                {
                    var ReferenceProperty2 = MapProperties[Y];
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        if (IsDebug)
                            logger?.LogDebug("Found duplicate map and removing {propertyName} from mapping {objectTypeName}", ReferenceProperty2.Name, ObjectType.Name);
                        MapProperties.Remove(ReferenceProperty2);
                        --Y;
                    }
                }
            }
            for (var X = 0; X < parentMapping.ManyToManyProperties.Count; ++X)
            {
                var ReferenceProperty1 = parentMapping.ManyToManyProperties[X];
                for (var Y = X + 1; Y < ManyToManyProperties.Count; ++Y)
                {
                    var ReferenceProperty2 = ManyToManyProperties[Y];
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        if (IsDebug)
                            logger?.LogDebug("Found duplicate many to many and removing {propertyName} from mapping {objectTypeName}", ReferenceProperty2.Name, ObjectType.Name);
                        ManyToManyProperties.Remove(ReferenceProperty2);
                        --Y;
                    }
                }
            }

            for (var X = 0; X < parentMapping.ManyToOneProperties.Count; ++X)
            {
                var ReferenceProperty1 = parentMapping.ManyToOneProperties[X];
                for (var Y = X + 1; Y < ManyToOneProperties.Count; ++Y)
                {
                    var ReferenceProperty2 = ManyToOneProperties[Y];
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        if (IsDebug)
                            logger?.LogDebug("Found duplicate many to one and removing {propertyName} from mapping {objectTypeName}", ReferenceProperty2.Name, ObjectType.Name);
                        ManyToOneProperties.Remove(ReferenceProperty2);
                        --Y;
                    }
                }
            }
        }

        /// <summary>
        /// Sets a property as a reference type
        /// </summary>
        /// <typeparam name="TDataType">Data type</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>the reference object</returns>
        /// <exception cref="ArgumentNullException">expression</exception>
        public Reference<TClassType, TDataType?> Reference<TDataType>(System.Linq.Expressions.Expression<Func<TClassType, TDataType?>> expression)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var ReturnValue = new Reference<TClassType, TDataType?>(expression, this);
            ReferenceProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Sets the default query based on query type
        /// </summary>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="queryString">The query string.</param>
        /// <param name="databaseCommandType">Type of the database command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>This</returns>
        /// <exception cref="ArgumentNullException">queryString</exception>
        public IMapping SetQuery(QueryType queryType, string queryString, CommandType databaseCommandType, params IParameter[] parameters)
        {
            if (string.IsNullOrEmpty(queryString))
                throw new ArgumentNullException(nameof(queryString));

            Queries.Add(queryType, new Query(ObjectType, databaseCommandType, queryString, queryType, parameters));
            return this;
        }

        /// <summary>
        /// Sets up the mapping
        /// </summary>
        public void Setup()
        {
        }

        /// <summary>
        /// Converts the mapping to a string
        /// </summary>
        /// <returns>The table name</returns>
        public override string ToString() => _ToString;
    }
}