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
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.Schema;
using SQLHelper.HelperClasses;
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.BaseClasses
{
    /// <summary>
    /// Many class property base.
    /// </summary>
    /// <typeparam name="ClassType">The class type.</typeparam>
    /// <typeparam name="DataType">The data type.</typeparam>
    /// <typeparam name="ReturnType">The return type.</typeparam>
    public abstract class ManyClassPropertyBase<ClassType, DataType, ReturnType> : IManyToManyProperty<ClassType, IList<DataType>, ReturnType>, IManyToManyProperty<ClassType, IList<DataType>>
        where ClassType : class
        where ReturnType : IManyToManyProperty<ClassType, IList<DataType>, ReturnType>
        where DataType : class
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        protected ManyClassPropertyBase(Expression<Func<ClassType, IList<DataType>>> expression, IMapping mapping)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            Name = expression.PropertyName();
            CompiledExpression = expression.Compile();
            Expression = expression;
            InternalFieldName = "_" + Name + "Derived";
            ParentMapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            PropertyType = typeof(DataType);
            TypeName = PropertyType.GetName();
        }

        /// <summary>
        /// Gets a value indicating whether this <see
        /// cref="T:Inflatable.ClassMapper.Interfaces.IMapProperty"/> is cascade.
        /// </summary>
        /// <value><c>true</c> if cascade; otherwise, <c>false</c>.</value>
        public bool Cascade { get; protected set; }

        /// <summary>
        /// Compiled version of the expression
        /// </summary>
        /// <value>The compiled expression.</value>
        public Func<ClassType, IList<DataType>> CompiledExpression { get; }

        /// <summary>
        /// Expression pointing to the property
        /// </summary>
        /// <value>The expression.</value>
        public Expression<Func<ClassType, IList<DataType>>> Expression { get; }

        /// <summary>
        /// Gets the foreign mapping.
        /// </summary>
        /// <value>The foreign mapping.</value>
        public IMapping ForeignMapping { get; protected set; }

        /// <summary>
        /// Gets the name of the internal field.
        /// </summary>
        /// <value>The name of the internal field.</value>
        public string InternalFieldName { get; }

        /// <summary>
        /// Gets the load property query.
        /// </summary>
        /// <value>The load property query.</value>
        public Query LoadPropertyQuery { get; protected set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the parent mapping.
        /// </summary>
        /// <value>The parent mapping.</value>
        public IMapping ParentMapping { get; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        public Type PropertyType { get; protected set; }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; protected set; }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        public string TypeName { get; protected set; }

        /// <summary>
        /// != operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>returns true if they are not equal, false otherwise</returns>
        public static bool operator !=(ManyClassPropertyBase<ClassType, DataType, ReturnType> first, ManyClassPropertyBase<ClassType, DataType, ReturnType> second)
        {
            return !(first == second);
        }

        /// <summary>
        /// The &lt; operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>True if the first item is less than the second, false otherwise</returns>
        public static bool operator <(ManyClassPropertyBase<ClassType, DataType, ReturnType> first, ManyClassPropertyBase<ClassType, DataType, ReturnType> second)
        {
            if (ReferenceEquals(first, second))
                return false;
            if ((object)first == null || (object)second == null)
                return false;
            return first.GetHashCode() < second.GetHashCode();
        }

        /// <summary>
        /// The == operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>true if the first and second item are the same, false otherwise</returns>
        public static bool operator ==(ManyClassPropertyBase<ClassType, DataType, ReturnType> first, ManyClassPropertyBase<ClassType, DataType, ReturnType> second)
        {
            if (ReferenceEquals(first, second))
                return true;

            if ((object)first == null || (object)second == null) return false;

            return first.GetHashCode() == second.GetHashCode();
        }

        /// <summary>
        /// The &gt; operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>True if the first item is greater than the second, false otherwise</returns>
        public static bool operator >(ManyClassPropertyBase<ClassType, DataType, ReturnType> first, ManyClassPropertyBase<ClassType, DataType, ReturnType> second)
        {
            if (ReferenceEquals(first, second))
                return false;
            if ((object)first == null || (object)second == null)
                return false;
            return first.GetHashCode() > second.GetHashCode();
        }

        /// <summary>
        /// Cascades changes to the mapped instance.
        /// </summary>
        /// <returns>This</returns>
        public ReturnType CascadeChanges()
        {
            Cascade = true;
            return (ReturnType)((IManyToManyProperty<ClassType, IList<DataType>, ReturnType>)this);
        }

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        public abstract IManyToManyProperty Convert<TResult>(IMapping mapping)
            where TResult : class;

        /// <summary>
        /// Determines if the two objects are equal and returns true if they are, false otherwise
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            var SecondObj = obj as ManyClassPropertyBase<ClassType, DataType, ReturnType>;
            if (((object)SecondObj) == null)
                return false;
            return this == SecondObj;
        }

        /// <summary>
        /// Gets as parameter.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <returns></returns>
        public IEnumerable<IParameter> GetAsParameter(object queryObject, object propertyValue)
        {
            List<IParameter> Parameters = new List<IParameter>();
            Parameters.AddRange(ForeignMapping.IDProperties.ForEach<IIDProperty, IParameter>(x =>
            {
                var Value = x.GetValue(propertyValue);
                if (PropertyType == typeof(string))
                {
                    var TempParameter = Value as string;
                    return new StringParameter(ForeignMapping.TableName + x.ColumnName,
                        TempParameter);
                }
                return new Parameter<object>(ForeignMapping.TableName + x.ColumnName,
                    PropertyType.To<Type, SqlDbType>(),
                    Value);
            }));
            Parameters.AddRange(ParentMapping.IDProperties.ForEach<IIDProperty, IParameter>(x =>
            {
                var Value = x.GetValue(queryObject);
                if (PropertyType == typeof(string))
                {
                    var TempParameter = Value as string;
                    return new StringParameter(ParentMapping.TableName + x.ColumnName,
                        TempParameter);
                }
                return new Parameter<object>(ParentMapping.TableName + x.ColumnName,
                    PropertyType.To<Type, SqlDbType>(),
                    Value);
            }));
            return Parameters;
        }

        /// <summary>
        /// Returns the hash code for the property
        /// </summary>
        /// <returns>The hash code for the property</returns>
        public override int GetHashCode()
        {
            return (Name.GetHashCode() * ParentMapping.GetHashCode()) % int.MaxValue;
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="Object">The object.</param>
        /// <returns>The property</returns>
        public object GetProperty(ClassType Object)
        {
            return CompiledExpression(Object);
        }

        /// <summary>
        /// Gets the property's value from the object sent in
        /// </summary>
        /// <param name="Object">Object to get the value from</param>
        /// <returns>The value of the property</returns>
        public object GetValue(ClassType Object)
        {
            if (Object == default(ClassType))
                return null;
            return CompiledExpression(Object);
        }

        /// <summary>
        /// Gets the property's value from the object sent in
        /// </summary>
        /// <param name="Object">Object to get the value from</param>
        /// <returns>The value of the property</returns>
        public object GetValue(object Object)
        {
            return GetValue(Object as ClassType);
        }

        /// <summary>
        /// Loads the property using the query specified.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <param name="type">The type.</param>
        /// <returns>This</returns>
        public ReturnType LoadUsing(string queryText, CommandType type)
        {
            LoadPropertyQuery = new Query(PropertyType, type, queryText, QueryType.LoadProperty);
            return (ReturnType)((IManyToManyProperty<ClassType, IList<DataType>, ReturnType>)this);
        }

        /// <summary>
        /// Sets the table's name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>This</returns>
        public ReturnType SetTableName(string tableName)
        {
            TableName = tableName;
            return (ReturnType)((IManyToManyProperty<ClassType, IList<DataType>, ReturnType>)this);
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="dataModel">The data model.</param>
        public abstract void Setup(MappingSource mappings, DataModel dataModel);

        /// <summary>
        /// Checks if the properties are similar to one another
        /// </summary>
        /// <param name="secondProperty">The second property.</param>
        /// <returns>True if they are similar, false otherwise</returns>
        public bool Similar(IManyToManyProperty secondProperty)
        {
            return secondProperty.TableName == TableName
                && secondProperty.Name == Name;
        }

        /// <summary>
        /// Gets the property as a string
        /// </summary>
        /// <returns>The string representation of the property</returns>
        public override string ToString()
        {
            return PropertyType.GetName() + " " + ParentMapping + "." + Name;
        }
    }
}