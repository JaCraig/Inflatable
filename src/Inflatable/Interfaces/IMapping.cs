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
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Interfaces;
using Serilog;
using SQLHelperDB.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Inflatable.Interfaces
{
    /// <summary>
    /// Class mapping interface
    /// </summary>
    /// <typeparam name="TClassType">Class type</typeparam>
    /// <seealso cref="IMapping"/>
    public interface IMapping<TClassType> : IMapping
        where TClassType : class
    {
        /// <summary>
        /// Declares a property as an ID
        /// </summary>
        /// <typeparam name="TDataType">Data type</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>the ID object</returns>
        ID<TClassType, TDataType> ID<TDataType>(Expression<Func<TClassType, TDataType>> expression);

        /// <summary>
        /// Sets a property as a many to many type.
        /// </summary>
        /// <typeparam name="TDataType">The type of the ata type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The many to many object</returns>
        ManyToMany<TClassType, TDataType> ManyToMany<TDataType>(Expression<Func<TClassType, IList<TDataType?>>> expression)
            where TDataType : class;

        /// <summary>
        /// Sets a property as a many to one type.
        /// </summary>
        /// <typeparam name="TDataType">The type of the data type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The many to many object</returns>
        ManyToOneMany<TClassType, TDataType> ManyToOne<TDataType>(Expression<Func<TClassType, IList<TDataType?>>> expression)
            where TDataType : class;

        /// <summary>
        /// Sets a property as a many to one type.
        /// </summary>
        /// <typeparam name="TDataType">The type of the data type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The many to many object</returns>
        ManyToOneSingle<TClassType, TDataType> ManyToOne<TDataType>(Expression<Func<TClassType, TDataType?>> expression)
            where TDataType : class;

        /// <summary>
        /// Sets a property as a map type.
        /// </summary>
        /// <typeparam name="TDataType">The type of the data type.</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>The map object</returns>
        Map<TClassType, TDataType> Map<TDataType>(Expression<Func<TClassType, TDataType?>> expression)
            where TDataType : class;

        /// <summary>
        /// Sets a property as a reference type
        /// </summary>
        /// <typeparam name="TDataType">Data type</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>the reference object</returns>
        Reference<TClassType, TDataType> Reference<TDataType>(Expression<Func<TClassType, TDataType>> expression);
    }

    /// <summary>
    /// Mapping interface
    /// </summary>
    public interface IMapping
    {
        /// <summary>
        /// Gets the automatic identifier properties.
        /// </summary>
        /// <value>The automatic identifier properties.</value>
        List<IAutoIDProperty> AutoIDProperties { get; }

        /// <summary>
        /// Gets the type of the database configuration.
        /// </summary>
        /// <value>The type of the database configuration.</value>
        Type DatabaseConfigType { get; }

        /// <summary>
        /// ID properties
        /// </summary>
        /// <value>The identifier properties.</value>
        List<IIDProperty> IDProperties { get; }

        /// <summary>
        /// Gets the many to many properties.
        /// </summary>
        /// <value>The many to many properties.</value>
        List<IManyToManyProperty> ManyToManyProperties { get; }

        /// <summary>
        /// Gets the many to many properties.
        /// </summary>
        /// <value>The many to many properties.</value>
        List<IManyToOneProperty> ManyToOneProperties { get; }

        /// <summary>
        /// Gets the map properties.
        /// </summary>
        /// <value>The map properties.</value>
        List<IMapProperty> MapProperties { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMapping"/> should be merged.
        /// </summary>
        /// <value><c>true</c> if merge this instance; otherwise, <c>false</c>.</value>
        bool Merge { get; }

        /// <summary>
        /// The object type associated with the mapping
        /// </summary>
        /// <value>The type of the object.</value>
        Type ObjectType { get; }

        /// <summary>
        /// Order that the mappings are initialized
        /// </summary>
        /// <value>The order.</value>
        int Order { get; }

        /// <summary>
        /// Prefix used for defining properties/table name
        /// </summary>
        /// <value>The prefix.</value>
        string Prefix { get; }

        /// <summary>
        /// Gets the queries.
        /// </summary>
        /// <value>The queries.</value>
        IQueries Queries { get; }

        /// <summary>
        /// Reference Properties list
        /// </summary>
        /// <value>The reference properties.</value>
        List<IProperty> ReferenceProperties { get; }

        /// <summary>
        /// Gets the name of the schema.
        /// </summary>
        /// <value>The name of the schema.</value>
        string SchemaName { get; }

        /// <summary>
        /// Suffix used for defining properties/table name
        /// </summary>
        /// <value>The suffix.</value>
        string Suffix { get; }

        /// <summary>
        /// Table name
        /// </summary>
        /// <value>The name of the table.</value>
        string TableName { get; }

        /// <summary>
        /// Adds an automatic key.
        /// </summary>
        void AddAutoKey();

        /// <summary>
        /// Determines whether the mapping contains a property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><c>true</c> if the mapping contains the specified property; otherwise, <c>false</c>.</returns>
        bool ContainsProperty(string propertyName);

        /// <summary>
        /// Copies the specified mapping.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        void Copy(IMapping mapping);

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        void CopyProperty(IIDProperty prop);

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        void CopyProperty(IProperty prop);

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        void CopyProperty(IMapProperty prop);

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        void CopyProperty(IManyToOneProperty prop);

        /// <summary>
        /// Copies the property.
        /// </summary>
        /// <param name="prop">The property.</param>
        void CopyProperty(IManyToManyProperty prop);

        /// <summary>
        /// Gets the name of the column based on property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The column name.</returns>
        string GetColumnName(string propertyName);

        /// <summary>
        /// Reduces this instance and removes duplicate properties
        /// </summary>
        /// <param name="logger">The logger.</param>
        void Reduce(ILogger logger);

        /// <summary>
        /// Reduces this instance based on parent mapping properties.
        /// </summary>
        /// <param name="parentMapping">The parent mapping.</param>
        /// <param name="logger">The logger.</param>
        void Reduce(IMapping parentMapping, ILogger logger);

        /// <summary>
        /// Sets the default query based on query type
        /// </summary>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="queryString">The query string.</param>
        /// <param name="databaseCommandType">Type of the database command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>This</returns>
        IMapping SetQuery(QueryType queryType, string queryString, CommandType databaseCommandType, params IParameter[] parameters);

        /// <summary>
        /// Sets up the mapping
        /// </summary>
        void Setup();
    }
}