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
    /// <seealso cref="Inflatable.Interfaces.IMapping"/>
    /// <seealso cref="Inflatable.Interfaces.IMapping{ClassType}"/>
    public abstract class MappingBaseClass<ClassType, DatabaseType> : IMapping, IMapping<ClassType>
        where DatabaseType : IDatabase
        where ClassType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingBaseClass{ClassType, DatabaseType}"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="suffix">The suffix.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="order">The order.</param>
        protected MappingBaseClass(string tableName = "", string suffix = "_", string prefix = "", int order = 10)
        {
            AutoIDProperties = new List<IAutoIDProperty>();
            IDProperties = new List<IIDProperty>();
            Order = order;
            Prefix = prefix ?? "";
            Queries = new Queries();
            ReferenceProperties = new List<IProperty>();
            Suffix = suffix ?? "";
            TableName = string.IsNullOrEmpty(tableName) ? Prefix + ObjectType.Name + Suffix : tableName;
        }

        /// <summary>
        /// Gets the automatic identifier properties.
        /// </summary>
        /// <value>
        /// The automatic identifier properties.
        /// </value>
        public ICollection<IAutoIDProperty> AutoIDProperties { get; private set; }

        /// <summary>
        /// Gets the type of the database configuration.
        /// </summary>
        /// <value>The type of the database configuration.</value>
        public Type DatabaseConfigType => typeof(DatabaseType);

        /// <summary>
        /// ID properties
        /// </summary>
        /// <value>The identifier properties.</value>
        public ICollection<IIDProperty> IDProperties { get; private set; }

        /// <summary>
        /// The object type associated with the mapping
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType => typeof(ClassType);

        /// <summary>
        /// Order that the mappings are initialized
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; private set; }

        /// <summary>
        /// Prefix used for defining properties/table name
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix { get; private set; }

        /// <summary>
        /// Gets the queries.
        /// </summary>
        /// <value>The queries.</value>
        public IQueries Queries { get; private set; }

        /// <summary>
        /// Reference Properties list
        /// </summary>
        /// <value>The reference properties.</value>
        public ICollection<IProperty> ReferenceProperties { get; private set; }

        /// <summary>
        /// Suffix used for defining properties/table name
        /// </summary>
        /// <value>The suffix.</value>
        public string Suffix { get; private set; }

        /// <summary>
        /// Table name
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; private set; }

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
        public void AddAutoKey()
        {
            AutoIDProperties.Add(new AutoID(Prefix + "ID" + Suffix, this));
        }

        /// <summary>
        /// Copies the specified mapping.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        public void Copy(IMapping mapping)
        {
            foreach (var prop in mapping.ReferenceProperties)
            {
                ReferenceProperties.Add(prop.Convert<ClassType>(this));
            }
        }

        /// <summary>
        /// determines if the mappings are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var Object2 = obj as MappingBaseClass<ClassType, DatabaseType>;
            if ((object)Object2 == null)
                return false;
            return string.Equals(TableName, Object2.TableName, StringComparison.Ordinal)
                && DatabaseConfigType == Object2.DatabaseConfigType;
        }

        /// <summary>
        /// Gets the mapping's hash code
        /// </summary>
        /// <returns>Hash code for the mapping</returns>
        public override int GetHashCode()
        {
            return (TableName.GetHashCode() * DatabaseConfigType.GetHashCode()) % int.MaxValue;
        }

        /// <summary>
        /// Declares a property as an ID
        /// </summary>
        /// <typeparam name="DataType">Data type</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>the ID object</returns>
        /// <exception cref="ArgumentNullException">expression</exception>
        public ID<ClassType, DataType> ID<DataType>(System.Linq.Expressions.Expression<Func<ClassType, DataType>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var ReturnValue = new ID<ClassType, DataType>(expression, this);
            IDProperties.Add(ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Reduces this instance and removes duplicate properties
        /// </summary>
        /// <param name="logger">The logger.</param>
        public void Reduce(ILogger logger)
        {
            for (int x = 0; x < IDProperties.Count; ++x)
            {
                var IDProperty1 = IDProperties.ElementAt(x);
                for (int y = x + 1; y < IDProperties.Count; ++y)
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
            for (int x = 0; x < ReferenceProperties.Count; ++x)
            {
                var ReferenceProperty1 = ReferenceProperties.ElementAt(x);
                for (int y = x + 1; y < ReferenceProperties.Count; ++y)
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
        }

        /// <summary>
        /// Reduces this instance based on parent mapping properties.
        /// </summary>
        /// <param name="parentMapping">The parent mapping.</param>
        /// <param name="logger">The logger.</param>
        public void Reduce(IMapping parentMapping, ILogger logger)
        {
            for (int x = 0; x < parentMapping.ReferenceProperties.Count; ++x)
            {
                var ReferenceProperty1 = parentMapping.ReferenceProperties.ElementAt(x);
                for (int y = 0; y < ReferenceProperties.Count; ++y)
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
            if (expression == null) throw new ArgumentNullException(nameof(expression));
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
        /// <returns>This</returns>
        public IMapping SetQuery(QueryType queryType, string queryString, CommandType databaseCommandType)
        {
            if (string.IsNullOrEmpty(queryString)) throw new ArgumentNullException(nameof(queryString));
            Queries.Add(queryType, new Query(databaseCommandType, queryString, queryType));
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
        public override string ToString()
        {
            return ObjectType.Name;
        }
    }
}