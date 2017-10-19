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

using BigBook;
using Inflatable.ClassMapper.Column.Interfaces;
using SQLHelper.HelperClasses.Interfaces;
using System;

namespace Inflatable.ClassMapper.Column
{
    /// <summary>
    /// Column information
    /// </summary>
    /// <typeparam name="TClassType">The type of the class type.</typeparam>
    /// <typeparam name="TDataType">The type of the data type.</typeparam>
    /// <seealso cref="IQueryColumnInfo"/>
    public class ComplexColumnInfo<TClassType, TDataType> : IQueryColumnInfo
        where TClassType : class
        where TDataType : class
    {
        /// <summary>
        /// Gets or sets the child.
        /// </summary>
        /// <value>The child.</value>
        public IQueryColumnInfo Child { get; set; }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        public string ColumnName { get; set; }

        /// <summary>
        /// The compiled expression
        /// </summary>
        /// <value>The compiled expression.</value>
        public Func<TClassType, TDataType> CompiledExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is foreign.
        /// </summary>
        /// <value><c>true</c> if this instance is foreign; otherwise, <c>false</c>.</value>
        public bool IsForeign { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string PropertyName => Child.PropertyName;

        /// <summary>
        /// Property type
        /// </summary>
        /// <value>The type of the property.</value>
        public Type PropertyType => Child.PropertyType;

        /// <summary>
        /// Gets the schema name.
        /// </summary>
        /// <value>The schema name.</value>
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        /// <value>The table name.</value>
        public string TableName { get; set; }

        /// <summary>
        /// Creates a copy.
        /// </summary>
        /// <returns>The resulting copy.</returns>
        public IQueryColumnInfo CreateCopy()
        {
            return new ComplexColumnInfo<TClassType, TDataType>
            {
                Child = Child,
                ColumnName = ColumnName,
                CompiledExpression = CompiledExpression,
                SchemaName = SchemaName,
                TableName = TableName
            };
        }

        /// <summary>
        /// Gets the property as an IParameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="objectValue"></param>
        /// <returns>The parameter version of the property</returns>
        public IParameter GetAsParameter(object objectValue)
        {
            var TempObject = objectValue as TClassType;
            object ParamValue = ReferenceEquals(objectValue, null) ? null : (object)CompiledExpression(TempObject);
            var TempParameter = Child.GetAsParameter(ParamValue);
            TempParameter.ID = ColumnName;
            return TempParameter;
        }

        /// <summary>
        /// Gets as parameter.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <returns>The object value as a parameter.</returns>
        public IParameter GetAsParameter(object objectValue, object paramValue)
        {
            return GetAsParameter(objectValue);
        }

        /// <summary>
        /// Gets the value of the item
        /// </summary>
        /// <param name="object">Object</param>
        /// <returns>The value specified</returns>
        public object GetValue(Dynamo @object)
        {
            return @object[PropertyName];
        }

        /// <summary>
        /// Gets the value of the item
        /// </summary>
        /// <param name="object">Object</param>
        /// <returns>The value specified</returns>
        public object GetValue(object @object)
        {
            return GetValue(@object as TClassType);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <returns>The resulting value.</returns>
        public object GetValue(object @object, object paramValue)
        {
            return GetValue(paramValue as TDataType);
        }

        /// <summary>
        /// Is this a default value?
        /// </summary>
        /// <param name="object">Object</param>
        /// <returns>True if it is, false otherwise.</returns>
        public bool IsDefault(object @object)
        {
            if (ReferenceEquals(@object, default(TClassType)))
                return true;
            var ParamValue = CompiledExpression(@object as TClassType);
            return IsDefault(@object, ParamValue);
        }

        /// <summary>
        /// Determines whether the specified object is default.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <returns><c>true</c> if the specified object is default; otherwise, <c>false</c>.</returns>
        public bool IsDefault(object @object, object paramValue)
        {
            return Child.IsDefault(paramValue);
        }

        /// <summary>
        /// Sets the property's value for the object sent in.
        /// </summary>
        /// <param name="objectToSet">The object to set.</param>
        /// <param name="propertyValue">The property value.</param>
        public void SetValue(object objectToSet, object propertyValue)
        {
            if (ReferenceEquals(objectToSet, default(TClassType)))
                return;
            var TempPropertyValue = CompiledExpression(objectToSet as TClassType);
            SetValue(objectToSet, TempPropertyValue, propertyValue);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="objectToSet">The object to set.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <param name="propertyValue">The property value.</param>
        public void SetValue(object objectToSet, object paramValue, object propertyValue)
        {
            Child.SetValue(paramValue, propertyValue);
        }

        /// <summary>
        /// Gets the property's value from the object sent in
        /// </summary>
        /// <param name="object">Object to get the value from</param>
        /// <returns>The value of the property</returns>
        private object GetValue(TClassType @object)
        {
            if (ReferenceEquals(@object, default(TClassType)))
                return null;
            var ParamValue = CompiledExpression(@object);
            return GetValue(ParamValue);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="paramValue">The parameter value.</param>
        /// <returns>The resulting value</returns>
        private object GetValue(TDataType paramValue)
        {
            return Child.GetValue(paramValue);
        }
    }
}