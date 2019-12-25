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

using Inflatable.ClassMapper.Default;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Serilog;
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
    /// <typeparam name="ClassType">The type of the lass type.</typeparam>
    /// <typeparam name="DatabaseType">The type of the atabase type.</typeparam>
    /// <seealso cref="IMapping"/>
    /// <seealso cref="IMapping{ClassType}"/>
    public abstract class MappingBaseClass<ClassType, DatabaseType> : IMapping, IMapping<ClassType>
        where ClassType : class
        where DatabaseType : IDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingBaseClass{ClassType, DatabaseType}"/> class.
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
            AutoIDProperties = new List<IAutoIDProperty>();
            IDProperties = new List<IIDProperty>();
            Order = order;
            Prefix = prefix ?? "";
            Queries = new Queries();
            ReferenceProperties = new List<IProperty>();
            Suffix = suffix ?? "";
            TableName = string.IsNullOrEmpty(tableName) ? Prefix + ObjectType.Name + Suffix : tableName;
            Merge = merge;
            MapProperties = new List<IMapProperty>();
            ManyToManyProperties = new List<IManyToManyProperty>();
            ManyToOneProperties = new List<IManyToOneProperty>();
        }

        /// <summary>
        /// Gets the automatic identifier properties.
        /// </summary>
        /// <value>The automatic identifier properties.</value>
        public ICollection<IAutoIDProperty> AutoIDProperties { get; }

        /// <summary>
        /// Gets the type of the database configuration.
        /// </summary>
        /// <value>The type of the database configuration.</value>
        public Type DatabaseConfigType => typeof(DatabaseType);

        /// <summary>
        /// ID properties
        /// </summary>
        /// <value>The identifier properties.</value>
        public ICollection<IIDProperty> IDProperties { get; }

        /// <summary>
        /// Gets the many to many properties.
        /// </summary>
        /// <value>The many to many properties.</value>
        public ICollection<IManyToManyProperty> ManyToManyProperties { get; }

        /// <summary>
        /// Gets the many to one properties.
        /// </summary>
        /// <value>The many to one properties.</value>
        public ICollection<IManyToOneProperty> ManyToOneProperties { get; }

        /// <summary>
        /// Gets the map properties.
        /// </summary>
        /// <value>The map properties.</value>
        public ICollection<IMapProperty> MapProperties { get; }

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
        public Type ObjectType => typeof(ClassType);

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
        public ICollection<IProperty> ReferenceProperties { get; }

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
        /// <param name="Item1">Item 1</param>
        /// <param name="Item2">Item 2</param>
        /// <returns>True if they are not equal, false otherwise</returns>
        public static bool operator !=(MappingBaseClass<ClassType, DatabaseType> Item1, MappingBaseClass<ClassType, DatabaseType> Item2)
        {
            return !(Item1 == Item2);
        }

        /// <summary>
        /// Determines if the two items are equal
        /// </summary>
        /// <param name="Item1">Item 1</param>
        /// <param name="Item2">Item 2</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public static bool operator ==(MappingBaseClass<ClassType, DatabaseType> Item1, MappingBaseClass<ClassType, DatabaseType> Item2)
        {
            return Item1.Equals(Item2);
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
            foreach (var prop in mapping.IDProperties.Where(x => !IDProperties.Any(y => y.Name == x.Name)))
            {
                CopyProperty(prop);
            }
            foreach (var prop in mapping.ReferenceProperties.Where(x => !ReferenceProperties.Any(y => y.Name == x.Name)))
            {
                CopyProperty(prop);
            }
            foreach (var prop in mapping.MapProperties.Where(x => !MapProperties.Any(y => y.Name == x.Name)))
            {
                CopyProperty(prop);
            }
            foreach (var prop in mapping.ManyToManyProperties.Where(x => !ManyToManyProperties.Any(y => y.Name == x.Name)))
            {
                CopyProperty(prop);
            }
            foreach (var prop in mapping.ManyToOneProperties.Where(x => !ManyToOneProperties.Any(y => y.Name == x.Name)))
            {
                CopyProperty(prop);
            }
        }

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        public void CopyProperty(IIDProperty prop) => IDProperties.Add(prop.Convert<ClassType>(this));

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        public void CopyProperty(IProperty prop) => ReferenceProperties.Add(prop.Convert<ClassType>(this));

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        public void CopyProperty(IMapProperty prop) => MapProperties.Add(prop.Convert<ClassType>(this));

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        public void CopyProperty(IManyToOneProperty prop) => ManyToOneProperties.Add(prop.Convert<ClassType>(this));

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        public void CopyProperty(IManyToManyProperty prop) => ManyToManyProperties.Add(prop.Convert<ClassType>(this));

        /// <summary>
        /// determines if the mappings are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is MappingBaseClass<ClassType, DatabaseType> Object2))
            {
                return false;
            }

            return string.Equals(TableName, Object2.TableName, StringComparison.Ordinal)
                && DatabaseConfigType == Object2.DatabaseConfigType;
        }

        /// <summary>
        /// Gets the name of the column based on property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The column name.</returns>
        public string GetColumnName(string propertyName)
        {
            var IDProperty = IDProperties.FirstOrDefault(x => x.Name == propertyName);
            if (IDProperty != null)
            {
                return "[" + SchemaName + "].[" + TableName + "].[" + IDProperty.ColumnName + "]";
            }

            var ReferenceProperty = ReferenceProperties.FirstOrDefault(x => x.Name == propertyName);
            if (ReferenceProperty != null)
            {
                return "[" + SchemaName + "].[" + TableName + "].[" + ReferenceProperty.ColumnName + "]";
            }

            return "";
        }

        /// <summary>
        /// Gets the mapping's hash code
        /// </summary>
        /// <returns>Hash code for the mapping</returns>
        public override int GetHashCode() => (TableName.GetHashCode() * DatabaseConfigType.GetHashCode()) % int.MaxValue;

        /// <summary>
        /// Declares a property as an ID
        /// </summary>
        /// <typeparam name="DataType">Data type</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>the ID object</returns>
        /// <exception cref="ArgumentNullException">expression</exception>
        public ID<ClassType, DataType> ID<DataType>(System.Linq.Expressions.Expression<Func<ClassType, DataType>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var ReturnValue = new ID<ClassType, DataType>(expression, this);
            IDProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Sets a property as a many to many type.
        /// </summary>
        /// <typeparam name="DataType">The type of the data type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The many to many object</returns>
        public ManyToMany<ClassType, DataType> ManyToMany<DataType>(System.Linq.Expressions.Expression<Func<ClassType, IList<DataType>>> expression)
            where DataType : class
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var ReturnValue = new ManyToMany<ClassType, DataType>(expression, this);
            ManyToManyProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Sets a property as a many to one type.
        /// </summary>
        /// <typeparam name="DataType">The type of the data type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The many to many object</returns>
        public ManyToOneMany<ClassType, DataType> ManyToOne<DataType>(System.Linq.Expressions.Expression<Func<ClassType, IList<DataType>>> expression)
            where DataType : class
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var ReturnValue = new ManyToOneMany<ClassType, DataType>(expression, this);
            ManyToOneProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Sets a property as a many to one type.
        /// </summary>
        /// <typeparam name="DataType">The type of the data type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The many to many object</returns>
        public ManyToOneSingle<ClassType, DataType> ManyToOne<DataType>(System.Linq.Expressions.Expression<Func<ClassType, DataType>> expression)
            where DataType : class
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var ReturnValue = new ManyToOneSingle<ClassType, DataType>(expression, this);
            ManyToOneProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Sets a property as a map type.
        /// </summary>
        /// <typeparam name="DataType">The type of the data type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The map object</returns>
        /// <exception cref="ArgumentNullException">expression</exception>
        public Map<ClassType, DataType> Map<DataType>(System.Linq.Expressions.Expression<Func<ClassType, DataType>> expression)
            where DataType : class
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var ReturnValue = new Map<ClassType, DataType>(expression, this);
            MapProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Reduces this instance and removes duplicate properties
        /// </summary>
        /// <param name="logger">The logger.</param>
        public void Reduce(ILogger logger)
        {
            for (var x = 0; x < IDProperties.Count; ++x)
            {
                var IDProperty1 = IDProperties.ElementAt(x);
                for (var y = x + 1; y < IDProperties.Count; ++y)
                {
                    var IDProperty2 = IDProperties.ElementAt(y);
                    if (IDProperty1 == IDProperty2)
                    {
                        logger.Debug("Found duplicate ID and removing {Name:l} from mapping {Mapping:l}", IDProperty2.Name, ObjectType.Name);
                        IDProperties.Remove(IDProperty2);
                        --y;
                    }
                }
            }
            for (var x = 0; x < ReferenceProperties.Count; ++x)
            {
                var ReferenceProperty1 = ReferenceProperties.ElementAt(x);
                for (var y = x + 1; y < ReferenceProperties.Count; ++y)
                {
                    var ReferenceProperty2 = ReferenceProperties.ElementAt(y);
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        logger.Debug("Found duplicate reference and removing {Name:l} from mapping {Mapping:l}", ReferenceProperty2.Name, ObjectType.Name);
                        ReferenceProperties.Remove(ReferenceProperty2);
                        --y;
                    }
                }
            }
            for (var x = 0; x < MapProperties.Count; ++x)
            {
                var ReferenceProperty1 = MapProperties.ElementAt(x);
                for (var y = x + 1; y < MapProperties.Count; ++y)
                {
                    var ReferenceProperty2 = MapProperties.ElementAt(y);
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        logger.Debug("Found duplicate map and removing {Name:l} from mapping {Mapping:l}", ReferenceProperty2.Name, ObjectType.Name);
                        MapProperties.Remove(ReferenceProperty2);
                        --y;
                    }
                }
            }
            for (var x = 0; x < ManyToManyProperties.Count; ++x)
            {
                var ReferenceProperty1 = ManyToManyProperties.ElementAt(x);
                for (var y = x + 1; y < ManyToManyProperties.Count; ++y)
                {
                    var ReferenceProperty2 = ManyToManyProperties.ElementAt(y);
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        logger.Debug("Found duplicate many to many and removing {Name:l} from mapping {Mapping:l}", ReferenceProperty2.Name, ObjectType.Name);
                        ManyToManyProperties.Remove(ReferenceProperty2);
                        --y;
                    }
                }
            }

            for (var x = 0; x < ManyToOneProperties.Count; ++x)
            {
                var ReferenceProperty1 = ManyToOneProperties.ElementAt(x);
                for (var y = x + 1; y < ManyToOneProperties.Count; ++y)
                {
                    var ReferenceProperty2 = ManyToOneProperties.ElementAt(y);
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        logger.Debug("Found duplicate many to one and removing {Name:l} from mapping {Mapping:l}", ReferenceProperty2.Name, ObjectType.Name);
                        ManyToOneProperties.Remove(ReferenceProperty2);
                        --y;
                    }
                }
            }
        }

        /// <summary>
        /// Reduces this instance based on parent mapping properties.
        /// </summary>
        /// <param name="parentMapping">The parent mapping.</param>
        /// <param name="logger">The logger.</param>
        public void Reduce(IMapping parentMapping, ILogger logger)
        {
            for (var x = 0; x < parentMapping.ReferenceProperties.Count; ++x)
            {
                var ReferenceProperty1 = parentMapping.ReferenceProperties.ElementAt(x);
                for (var y = 0; y < ReferenceProperties.Count; ++y)
                {
                    var ReferenceProperty2 = ReferenceProperties.ElementAt(y);
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        logger.Debug("Found duplicate reference and removing {Name:l} from mapping {Mapping:l}", ReferenceProperty2.Name, ObjectType.Name);
                        ReferenceProperties.Remove(ReferenceProperty2);
                        --y;
                    }
                }
            }
            for (var x = 0; x < parentMapping.MapProperties.Count; ++x)
            {
                var ReferenceProperty1 = parentMapping.MapProperties.ElementAt(x);
                for (var y = x + 1; y < MapProperties.Count; ++y)
                {
                    var ReferenceProperty2 = MapProperties.ElementAt(y);
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        logger.Debug("Found duplicate map and removing {Name:l} from mapping {Mapping:l}", ReferenceProperty2.Name, ObjectType.Name);
                        MapProperties.Remove(ReferenceProperty2);
                        --y;
                    }
                }
            }
            for (var x = 0; x < parentMapping.ManyToManyProperties.Count; ++x)
            {
                var ReferenceProperty1 = parentMapping.ManyToManyProperties.ElementAt(x);
                for (var y = x + 1; y < ManyToManyProperties.Count; ++y)
                {
                    var ReferenceProperty2 = ManyToManyProperties.ElementAt(y);
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        logger.Debug("Found duplicate many to many and removing {Name:l} from mapping {Mapping:l}", ReferenceProperty2.Name, ObjectType.Name);
                        ManyToManyProperties.Remove(ReferenceProperty2);
                        --y;
                    }
                }
            }

            for (var x = 0; x < parentMapping.ManyToOneProperties.Count; ++x)
            {
                var ReferenceProperty1 = parentMapping.ManyToOneProperties.ElementAt(x);
                for (var y = x + 1; y < ManyToOneProperties.Count; ++y)
                {
                    var ReferenceProperty2 = ManyToOneProperties.ElementAt(y);
                    if (ReferenceProperty1.Similar(ReferenceProperty2))
                    {
                        logger.Debug("Found duplicate many to one and removing {Name:l} from mapping {Mapping:l}", ReferenceProperty2.Name, ObjectType.Name);
                        ManyToOneProperties.Remove(ReferenceProperty2);
                        --y;
                    }
                }
            }
        }

        /// <summary>
        /// Sets a property as a reference type
        /// </summary>
        /// <typeparam name="DataType">Data type</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>the reference object</returns>
        /// <exception cref="ArgumentNullException">expression</exception>
        public Reference<ClassType, DataType> Reference<DataType>(System.Linq.Expressions.Expression<Func<ClassType, DataType>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var ReturnValue = new Reference<ClassType, DataType>(expression, this);
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
            {
                throw new ArgumentNullException(nameof(queryString));
            }

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
        public override string ToString() => ObjectType.Name;
    }
}