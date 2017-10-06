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
using SQLHelper.HelperClasses.Interfaces;
using System;

namespace Inflatable.ClassMapper.Column.Interfaces
{
    /// <summary>
    /// IQuery column info
    /// </summary>
    public interface IQueryColumnInfo
    {
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        string ColumnName { get; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        string PropertyName { get; }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        Type PropertyType { get; }

        /// <summary>
        /// Gets or sets the name of the schema.
        /// </summary>
        /// <value>The name of the schema.</value>
        string SchemaName { get; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        string TableName { get; }

        /// <summary>
        /// Gets as parameter.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <returns></returns>
        IParameter GetAsParameter(object objectValue);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <returns></returns>
        object GetValue(Dynamo @object);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <returns></returns>
        object GetValue(object @object);

        /// <summary>
        /// Determines whether the specified object is default.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <returns><c>true</c> if the specified object is default; otherwise, <c>false</c>.</returns>
        bool IsDefault(object @object);
    }
}