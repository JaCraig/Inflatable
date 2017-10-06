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
        /// Gets the table name.
        /// </summary>
        /// <value>The table name.</value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets the property as an IParameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="objectValue"></param>
        /// <returns>The parameter version of the property</returns>
        public IParameter GetAsParameter(object objectValue)
        {
            var ParamValue = GetValue(objectValue);
            if (Equals(ParamValue, DefaultValue()))
                ParamValue = null;
            if (PropertyType == typeof(string))
            {
                var TempParameter = ParamValue as string;
                return new StringParameter(PropertyName, TempParameter);
            }
            return new Parameter<object>(PropertyName, PropertyType.To<Type, SqlDbType>(), ParamValue);
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
        /// Is this a default value?
        /// </summary>
        /// <param name="object">Object</param>
        /// <returns>True if it is, false otherwise.</returns>
        public bool IsDefault(object @object)
        {
            return Equals(GetValue(@object), DefaultValue());
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
            return CompiledExpression(@object);
        }
    }
}