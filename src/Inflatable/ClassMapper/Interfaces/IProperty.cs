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
    public interface IProperty<ClassType, DataType, ReturnType> : IFluentInterface
        where ClassType : class
        where ReturnType : IProperty<ClassType, DataType, ReturnType>
    {
        /// <summary>
        /// Determines whether this instance is indexed.
        /// </summary>
        /// <returns>this</returns>
        ReturnType IsIndexed();

        /// <summary>
        /// Determines whether [is read only].
        /// </summary>
        /// <returns>this</returns>
        ReturnType IsReadOnly();

        /// <summary>
        /// Determines whether this instance is unique.
        /// </summary>
        /// <returns>this</returns>
        ReturnType IsUnique();

        /// <summary>
        /// Sets the name of the field in the database.
        /// </summary>
        /// <param name="columnName">Name of the field.</param>
        /// <returns>this</returns>
        ReturnType WithColumnName(string columnName);

        /// <summary>
        /// Sets the computed column specification if the source allows it.
        /// </summary>
        /// <param name="computedColumnSpecification">The computed column specification.</param>
        /// <returns>this</returns>
        ReturnType WithComputedColumnSpecification(string computedColumnSpecification);

        /// <summary>
        /// Sets a constraint on the field if the source allows it.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns>this</returns>
        ReturnType WithConstraint(string constraint);

        /// <summary>
        /// Sets the default value of the property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>this</returns>
        ReturnType WithDefaultValue(Func<DataType> value);

        /// <summary>
        /// Sets the max length for the property (or precision for items like decimal values)
        /// </summary>
        /// <param name="maxLength">Max length</param>
        /// <returns>this</returns>
        ReturnType WithMaxLength(int maxLength);
    }

    /// <summary>
    /// Property interface
    /// </summary>
    /// <typeparam name="ClassType">Class type</typeparam>
    /// <typeparam name="DataType"></typeparam>
    public interface IProperty<ClassType, DataType> : IProperty
        where ClassType : class
    {
        /// <summary>
        /// Compiled version of the expression
        /// </summary>
        Func<ClassType, DataType> CompiledExpression { get; }

        /// <summary>
        /// Default value for this property
        /// </summary>
        Func<DataType> DefaultValue { get; }

        /// <summary>
        /// Expression pointing to the property
        /// </summary>
        Expression<Func<ClassType, DataType>> Expression { get; }
    }

    /// <summary>
    /// Property data holder interface
    /// </summary>
    public interface IProperty : IPropertyColumns
    {
        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        string ColumnName { get; }

        /// <summary>
        /// Gets the computed column specification.
        /// </summary>
        /// <value>The computed column specification.</value>
        string ComputedColumnSpecification { get; }

        /// <summary>
        /// Gets the constraints if the data source supports them.
        /// </summary>
        /// <value>The constraints if the data source supports them.</value>
        IList<string> Constraints { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IProperty"/> is indexed.
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
        /// Gets a value indicating whether this <see cref="IProperty"/> is nullable.
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
        /// Gets a value indicating whether [read only].
        /// </summary>
        /// <value><c>true</c> if [read only]; otherwise, <c>false</c>.</value>
        bool ReadOnly { get; }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        string TypeName { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IProperty"/> is unique.
        /// </summary>
        /// <value><c>true</c> if unique; otherwise, <c>false</c>.</value>
        bool Unique { get; }

        /// <summary>
        /// Adds this property to the table.
        /// </summary>
        /// <param name="table">The table.</param>
        void AddToTable(ITable table);

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        IProperty Convert<TResult>(IMapping mapping)
            where TResult : class;

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        void Setup();

        /// <summary>
        /// Similars the specified reference property2.
        /// </summary>
        /// <param name="secondProperty">The second property.</param>
        /// <returns>True if it is similar, false otherwise.</returns>
        bool Similar(IProperty secondProperty);
    }
}