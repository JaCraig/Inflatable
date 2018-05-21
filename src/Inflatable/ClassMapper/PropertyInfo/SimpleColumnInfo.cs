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
using Inflatable.ClassMapper.Column.Interfaces;
using SQLHelper.HelperClasses;
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Data;

namespace Inflatable.ClassMapper.Column
{
    /// <summary>
    /// Column information
    /// </summary>
    /// <typeparam name="TClassType">The type of the class type.</typeparam>
    /// <typeparam name="TDataType">The type of the data type.</typeparam>
    /// <seealso cref="Inflatable.ClassMapper.Column.Interfaces.IQueryColumnInfo"/>
    public class SimpleColumnInfo<TClassType, TDataType> : IQueryColumnInfo
        where TClassType : class
    {
        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        public string ColumnName { get; set; }

        /// <summary>
        /// The compiled expression
        /// </summary>
        public Func<TClassType, TDataType> CompiledExpression { get; set; }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        public Func<TDataType> DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is foreign.
        /// </summary>
        /// <value><c>true</c> if this instance is foreign; otherwise, <c>false</c>.</value>
        public bool IsForeign { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is nullable.
        /// </summary>
        /// <value><c>true</c> if this instance is nullable; otherwise, <c>false</c>.</value>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string PropertyName { get; set; }

        /// <summary>
        /// Property type
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Gets the schema name.
        /// </summary>
        /// <value>The schema name.</value>
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the expression used to set the value.
        /// </summary>
        /// <value>The set expression.</value>
        public Action<TClassType, TDataType> SetAction { get; set; }

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
            return new SimpleColumnInfo<TClassType, TDataType>
            {
                ColumnName = ColumnName,
                CompiledExpression = CompiledExpression,
                DefaultValue = DefaultValue,
                IsNullable = IsNullable,
                PropertyName = PropertyName,
                PropertyType = PropertyType,
                SchemaName = SchemaName,
                SetAction = SetAction,
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
            var ParamValue = GetValue(objectValue);
            if (Equals(ParamValue, DefaultValue()))
            {
                ParamValue = IsNullable ? null : (object)DefaultValue();
            }

            if (PropertyType == typeof(string))
            {
                var TempParameter = ParamValue as string;
                return new StringParameter(PropertyName, TempParameter);
            }
            return new Parameter<object>(PropertyName, PropertyType.To<Type, DbType>(), ParamValue);
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
            return GetValue(@object);
        }

        /// <summary>
        /// Is this a default value?
        /// </summary>
        /// <param name="object">Object</param>
        /// <returns>True if it is, false otherwise.</returns>
        public bool IsDefault(object @object)
        {
            if (ReferenceEquals(@object, default(TClassType)))
            {
                return true;
            }

            return Equals(GetValue(@object), DefaultValue());
        }

        /// <summary>
        /// Determines whether the specified object is default.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <returns><c>true</c> if the specified object is default; otherwise, <c>false</c>.</returns>
        public bool IsDefault(object @object, object paramValue)
        {
            return IsDefault(@object);
        }

        /// <summary>
        /// Sets the property's value for the object sent in.
        /// </summary>
        /// <param name="objectToSet">The object to set.</param>
        /// <param name="propertyValue">The property value.</param>
        public void SetValue(object objectToSet, object propertyValue)
        {
            var TempObject = objectToSet as TClassType;
            if (ReferenceEquals(TempObject, default(TClassType)))
            {
                return;
            }

            var TempPropertyValue = (TDataType)propertyValue;
            SetAction(TempObject, TempPropertyValue);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="objectToSet">The object to set.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <param name="propertyValue">The property value.</param>
        public void SetValue(object objectToSet, object paramValue, object propertyValue)
        {
            SetValue(objectToSet, propertyValue);
        }

        /// <summary>
        /// Gets the property's value from the object sent in
        /// </summary>
        /// <param name="object">Object to get the value from</param>
        /// <returns>The value of the property</returns>
        private object GetValue(TClassType @object)
        {
            if (ReferenceEquals(@object, default(TClassType)))
            {
                return null;
            }

            return CompiledExpression(@object);
        }
    }
}