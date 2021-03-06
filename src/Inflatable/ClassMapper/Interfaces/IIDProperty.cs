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

using BigBook.Patterns;
using Data.Modeler.Providers.Interfaces;
using Inflatable.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.Interfaces
{
    /// <summary>
    /// Property interface
    /// </summary>
    /// <typeparam name="ClassType">Class type</typeparam>
    /// <typeparam name="DataType">Data type</typeparam>
    /// <typeparam name="ReturnType">Return type</typeparam>
    public interface IIDProperty<ClassType, DataType, ReturnType> : IFluentInterface
        where ClassType : class
        where ReturnType : IIDProperty<ClassType, DataType, ReturnType>
    {
        /// <summary>
        /// Determines whether this [is auto incremented].
        /// </summary>
        /// <returns>this</returns>
        ReturnType IsAutoIncremented();

        /// <summary>
        /// Sets the name of the field in the database.
        /// </summary>
        /// <param name="columnName">Name of the field.</param>
        /// <returns>this</returns>
        ReturnType WithColumnName(string columnName);

        /// <summary>
        /// Sets a constraint on the field if the source allows it.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns>this</returns>
        ReturnType WithConstraint(string constraint);

        /// <summary>
        /// Sets the max length for the property (or precision for items like decimal values)
        /// </summary>
        /// <param name="maxLength">Max length</param>
        /// <returns>this</returns>
        ReturnType WithMaxLength(int maxLength);

        /// <summary>
        /// Sets the length for the property to MAX.
        /// </summary>
        /// <returns>this.</returns>
        ReturnType WithMaxLength();
    }

    /// <summary>
    /// Property interface
    /// </summary>
    /// <typeparam name="ClassType">Class type</typeparam>
    /// <typeparam name="DataType"></typeparam>
    public interface IIDProperty<ClassType, DataType> : IIDProperty
        where ClassType : class
    {
        /// <summary>
        /// Compiled version of the expression
        /// </summary>
        Func<ClassType, DataType> CompiledExpression { get; }

        /// <summary>
        /// Expression pointing to the property
        /// </summary>
        Expression<Func<ClassType, DataType>> Expression { get; }
    }

    /// <summary>
    /// ID property interface
    /// </summary>
    public interface IIDProperty : IPropertyColumns
    {
        /// <summary>
        /// Gets a value indicating whether to [automatic increment].
        /// </summary>
        /// <value><c>true</c> if [automatic increment]; otherwise, <c>false</c>.</value>
        bool AutoIncrement { get; }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        string ColumnName { get; }

        /// <summary>
        /// Gets the constraints if the data source supports them.
        /// </summary>
        /// <value>The constraints if the data source supports them.</value>
        IList<string> Constraints { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IIDProperty"/> is index.
        /// </summary>
        /// <value><c>true</c> if index; otherwise, <c>false</c>.</value>
        bool Index { get; }

        /// <summary>
        /// Gets the name of the internal field.
        /// </summary>
        /// <value>The name of the internal field.</value>
        string InternalFieldName { get; }

        /// <summary>
        /// Gets the maximum length.
        /// </summary>
        /// <value>The maximum length.</value>
        int MaxLength { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IIDProperty"/> is nullable.
        /// </summary>
        /// <value><c>true</c> if nullable; otherwise, <c>false</c>.</value>
        bool Nullable { get; }

        /// <summary>
        /// Gets the parent mapping.
        /// </summary>
        /// <value>The parent mapping.</value>
        IMapping ParentMapping { get; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        Type PropertyType { get; }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        string TypeName { get; }

        /// <summary>
        /// Adds to a child table.
        /// </summary>
        /// <param name="table">The table.</param>
        void AddToChildTable(ITable table);

        /// <summary>
        /// Adds this instance to the table.
        /// </summary>
        /// <param name="table">The table.</param>
        void AddToTable(ITable table);

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        IIDProperty Convert<TResult>(IMapping mapping)
            where TResult : class;

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        void Setup();
    }
}