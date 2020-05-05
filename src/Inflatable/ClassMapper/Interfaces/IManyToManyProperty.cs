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

using BigBook.Patterns;
using Data.Modeler.Providers.Interfaces;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.Interfaces
{
    /// <summary>
    /// Many to many property interface
    /// </summary>
    /// <typeparam name="ClassType">The class type.</typeparam>
    /// <typeparam name="DataType">The data type.</typeparam>
    /// <typeparam name="ReturnType">The return type.</typeparam>
    /// <seealso cref="IClassProperty"/>
    /// <seealso cref="IFluentInterface"/>
    public interface IManyToManyProperty<ClassType, DataType, ReturnType> : IFluentInterface
        where ClassType : class
        where DataType : class
        where ReturnType : IManyToManyProperty<ClassType, DataType, ReturnType>
    {
        /// <summary>
        /// Cascades changes to the mapped instance.
        /// </summary>
        /// <returns>This</returns>
        ReturnType CascadeChanges();

        /// <summary>
        /// Loads the property using the query specified.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <param name="type">The type.</param>
        /// <returns>This</returns>
        ReturnType LoadUsing(string queryText, CommandType type);

        /// <summary>
        /// Called when you want to override the default referential integrity and do nothing on delete.
        /// </summary>
        /// <returns>This</returns>
        ReturnType OnDeleteDoNothing();

        /// <summary>
        /// Sets the table's name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>This</returns>
        ReturnType SetTableName(string tableName);
    }

    /// <summary>
    /// Many to many property
    /// </summary>
    /// <typeparam name="ClassType">The class type.</typeparam>
    /// <typeparam name="DataType">The data type.</typeparam>
    /// <seealso cref="IFluentInterface"/>
    public interface IManyToManyProperty<ClassType, DataType> : IManyToManyProperty
        where ClassType : class
        where DataType : class
    {
        /// <summary>
        /// Compiled version of the expression
        /// </summary>
        /// <value>The compiled expression.</value>
        Func<ClassType, DataType> CompiledExpression { get; }

        /// <summary>
        /// Expression pointing to the property
        /// </summary>
        /// <value>The expression.</value>
        Expression<Func<ClassType, DataType>> Expression { get; }
    }

    /// <summary>
    /// Many to many property
    /// </summary>
    /// <seealso cref="IFluentInterface"/>
    public interface IManyToManyProperty : IClassProperty
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IMapProperty"/> is cascade.
        /// </summary>
        /// <value><c>true</c> if cascade; otherwise, <c>false</c>.</value>
        bool Cascade { get; }

        /// <summary>
        /// Gets a value indicating whether [database joins cascade].
        /// </summary>
        /// <value><c>true</c> if [database joins cascade]; otherwise, <c>false</c>.</value>
        bool DatabaseJoinsCascade { get; }

        /// <summary>
        /// Gets the foreign mapping.
        /// </summary>
        /// <value>The foreign mapping.</value>
        List<IMapping> ForeignMapping { get; }

        /// <summary>
        /// Gets the name of the internal field.
        /// </summary>
        /// <value>The name of the internal field.</value>
        string InternalFieldName { get; }

        /// <summary>
        /// Gets the load property query.
        /// </summary>
        /// <value>The load property query.</value>
        Query? LoadPropertyQuery { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        Type PropertyType { get; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        string? TableName { get; }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        string TypeName { get; }

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        IManyToManyProperty Convert<TResult>(IMapping mapping)
            where TResult : class;

        /// <summary>
        /// Gets the property's value from the object sent in
        /// </summary>
        /// <param name="Object">Object to get the value from</param>
        /// <returns>The value of the property</returns>
        object? GetValue(object Object);

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="sourceSpec">The source spec.</param>
        void Setup(IMappingSource mappings, ISource sourceSpec);

        /// <summary>
        /// Similars the specified reference property2.
        /// </summary>
        /// <param name="secondProperty">The second property.</param>
        /// <returns>True if it is similar, false otherwise.</returns>
        bool Similar(IManyToManyProperty secondProperty);
    }
}