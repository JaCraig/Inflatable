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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Inflatable.Interfaces
{
    /// <summary>
    /// Class mapping interface
    /// </summary>
    /// <typeparam name="ClassType">Class type</typeparam>
    /// <seealso cref="IMapping"/>
    public interface IMapping<ClassType> : IMapping
        where ClassType : class
    {
        /// <summary>
        /// Declares a property as an ID
        /// </summary>
        /// <typeparam name="DataType">Data type</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>the ID object</returns>
        ID<ClassType, DataType> ID<DataType>(Expression<Func<ClassType, DataType>> expression);

        /// <summary>
        /// Sets a property as a reference type
        /// </summary>
        /// <typeparam name="DataType">Data type</typeparam>
        /// <param name="expression">Expression pointing to the property</param>
        /// <returns>the reference object</returns>
        Reference<ClassType, DataType> Reference<DataType>(Expression<Func<ClassType, DataType>> expression);
    }

    /// <summary>
    /// Mapping interface
    /// </summary>
    public interface IMapping
    {
        /// <summary>
        /// Gets the type of the database configuration.
        /// </summary>
        /// <value>The type of the database configuration.</value>
        Type DatabaseConfigType { get; }

        /// <summary>
        /// ID properties
        /// </summary>
        /// <value>The identifier properties.</value>
        ICollection<IIDProperty> IDProperties { get; }

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
        ICollection<IProperty> ReferenceProperties { get; }

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
        /// Sets the default query based on query type
        /// </summary>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="queryString">The query string.</param>
        /// <param name="databaseCommandType">Type of the database command.</param>
        /// <returns>This</returns>
        IMapping SetQuery(QueryType queryType, string queryString, CommandType databaseCommandType);

        /// <summary>
        /// Sets up the mapping
        /// </summary>
        void Setup();
    }
}