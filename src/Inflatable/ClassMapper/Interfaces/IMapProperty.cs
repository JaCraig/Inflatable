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
using BigBook.Patterns;
using Data.Modeler.Providers.Interfaces;
using Inflatable.Interfaces;
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.Interfaces
{
    /// <summary>
    /// Map property interface
    /// </summary>
    /// <typeparam name="ClassType">The type of the class type.</typeparam>
    /// <typeparam name="DataType">The type of the data type.</typeparam>
    /// <typeparam name="ReturnType">The type of the return type.</typeparam>
    /// <seealso cref="IFluentInterface"/>
    public interface IMapProperty<ClassType, DataType, ReturnType> : IFluentInterface
        where ClassType : class
        where DataType : class
        where ReturnType : IMapProperty<ClassType, DataType, ReturnType>
    {
        /// <summary>
        /// Cascades changes to the mapped instance.
        /// </summary>
        /// <returns>This</returns>
        ReturnType CascadeChanges();

        /// <summary>
        /// Determines whether this instance is unique.
        /// </summary>
        /// <returns>this</returns>
        ReturnType IsUnique();
    }

    /// <summary>
    /// Map property interface
    /// </summary>
    /// <typeparam name="ClassType">The type of the class type.</typeparam>
    /// <typeparam name="DataType">The type of the data type.</typeparam>
    /// <seealso cref="IFluentInterface"/>
    public interface IMapProperty<ClassType, DataType> : IMapProperty<ClassType>
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
    /// Map property interface
    /// </summary>
    /// <typeparam name="ClassType">The type of the class type.</typeparam>
    /// <seealso cref="IFluentInterface"/>
    public interface IMapProperty<ClassType> : IMapProperty
        where ClassType : class
    {
        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="Object">The object.</param>
        /// <returns></returns>
        object GetProperty(ClassType Object);
    }

    /// <summary>
    /// Map property
    /// </summary>
    /// <seealso cref="IFluentInterface"/>
    public interface IMapProperty
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IMapProperty"/> is cascade.
        /// </summary>
        /// <value><c>true</c> if cascade; otherwise, <c>false</c>.</value>
        bool Cascade { get; }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        string ColumnName { get; }

        /// <summary>
        /// Gets the foreign mapping.
        /// </summary>
        /// <value>The foreign mapping.</value>
        IMapping ForeignMapping { get; }

        /// <summary>
        /// Gets the name of the internal field.
        /// </summary>
        /// <value>The name of the internal field.</value>
        string InternalFieldName { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

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
        /// Gets a value indicating whether this <see cref="IMapProperty"/> is unique.
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
        IMapProperty Convert<TResult>(IMapping mapping)
            where TResult : class;

        /// <summary>
        /// Gets the property as a parameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <returns>The parameter version of the property</returns>
        IEnumerable<IParameter> GetAsParameter(object objectValue);

        /// <summary>
        /// Gets the property as a parameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="Object">Object to get the parameter from</param>
        /// <returns>The parameter version of the property</returns>
        object GetParameter(object Object);

        /// <summary>
        /// Gets the property as a parameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="Object">Object to get the parameter from</param>
        /// <returns>The parameter version of the property</returns>
        object GetParameter(Dynamo Object);

        /// <summary>
        /// Gets the property's value from the object sent in
        /// </summary>
        /// <param name="Object">Object to get the value from</param>
        /// <returns>The value of the property</returns>
        object GetValue(object Object);

        /// <summary>
        /// Gets the property's value from the object sent in
        /// </summary>
        /// <param name="Object">Object to get the value from</param>
        /// <returns>The value of the property</returns>
        object GetValue(Dynamo Object);

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        void Setup(MappingSource mappings);

        /// <summary>
        /// Similars the specified reference property2.
        /// </summary>
        /// <param name="secondProperty">The second property.</param>
        /// <returns>True if it is similar, false otherwise.</returns>
        bool Similar(IMapProperty secondProperty);
    }
}