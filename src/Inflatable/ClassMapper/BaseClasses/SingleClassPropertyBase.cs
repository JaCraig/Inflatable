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
using Data.Modeler.Providers.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using SQLHelper.HelperClasses;
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.BaseClasses
{
    /// <summary>
    /// Property base class
    /// </summary>
    /// <typeparam name="ClassType">The type of the class type.</typeparam>
    /// <typeparam name="DataType">The type of the data type.</typeparam>
    /// <typeparam name="ReturnType">The type of the return type.</typeparam>
    /// <seealso cref="Inflatable.ClassMapper.Interfaces.IProperty{ClassType, DataType, ReturnType}"/>
    /// <seealso cref="Inflatable.ClassMapper.Interfaces.IProperty{ClassType, DataType}"/>
    public abstract class SingleClassPropertyBase<ClassType, DataType, ReturnType> : IMapProperty<ClassType, DataType, ReturnType>, IMapProperty<ClassType, DataType>
        where ClassType : class
        where ReturnType : IMapProperty<ClassType, DataType, ReturnType>
        where DataType : class
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        protected SingleClassPropertyBase(Expression<Func<ClassType, DataType>> expression, IMapping mapping)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            Name = expression.PropertyName();
            ColumnName = mapping.Prefix + Name + mapping.Suffix;
            CompiledExpression = expression.Compile();
            Expression = expression;
            InternalFieldName = "_" + Name + "Derived";
            ParentMapping = mapping;
            PropertyType = typeof(DataType);
            TypeName = PropertyType.GetName();
        }

        /// <summary>
        /// Gets a value indicating whether this is cascade.
        /// </summary>
        /// <value><c>true</c> if cascade; otherwise, <c>false</c>.</value>
        public bool Cascade { get; protected set; }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        public string ColumnName { get; protected set; }

        /// <summary>
        /// Compiled version of the expression
        /// </summary>
        /// <value>The compiled expression.</value>
        public Func<ClassType, DataType> CompiledExpression { get; protected set; }

        /// <summary>
        /// Expression pointing to the property
        /// </summary>
        /// <value>The expression.</value>
        public Expression<Func<ClassType, DataType>> Expression { get; protected set; }

        /// <summary>
        /// Gets the foreign mapping.
        /// </summary>
        /// <value>The foreign mapping.</value>
        public IMapping ForeignMapping { get; protected set; }

        /// <summary>
        /// Gets the name of the internal field.
        /// </summary>
        /// <value>The name of the internal field.</value>
        public string InternalFieldName { get; protected set; }

        /// <summary>
        /// Gets the load property query.
        /// </summary>
        /// <value>The load property query.</value>
        public Query LoadPropertyQuery { get; protected set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the parent mapping.
        /// </summary>
        /// <value>The parent mapping.</value>
        public IMapping ParentMapping { get; protected set; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        public string TypeName { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this is unique.
        /// </summary>
        /// <value><c>true</c> if unique; otherwise, <c>false</c>.</value>
        public bool Unique { get; protected set; }

        /// <summary>
        /// != operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>returns true if they are not equal, false otherwise</returns>
        public static bool operator !=(SingleClassPropertyBase<ClassType, DataType, ReturnType> first, SingleClassPropertyBase<ClassType, DataType, ReturnType> second)
        {
            return !(first == second);
        }

        /// <summary>
        /// The &lt; operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>True if the first item is less than the second, false otherwise</returns>
        public static bool operator <(SingleClassPropertyBase<ClassType, DataType, ReturnType> first, SingleClassPropertyBase<ClassType, DataType, ReturnType> second)
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
        public static bool operator ==(SingleClassPropertyBase<ClassType, DataType, ReturnType> first, SingleClassPropertyBase<ClassType, DataType, ReturnType> second)
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
        public static bool operator >(SingleClassPropertyBase<ClassType, DataType, ReturnType> first, SingleClassPropertyBase<ClassType, DataType, ReturnType> second)
        {
            if (ReferenceEquals(first, second))
                return false;
            if ((object)first == null || (object)second == null)
                return false;
            return first.GetHashCode() > second.GetHashCode();
        }

        /// <summary>
        /// Adds to table.
        /// </summary>
        /// <param name="table">The table.</param>
        public void AddToTable(ITable table)
        {
            ForeignMapping.IDProperties.ForEach(x =>
            {
                table.AddColumn(ParentMapping.Prefix + Name + ParentMapping.Suffix + ForeignMapping.TableName + x.ColumnName,
                                x.PropertyType.To(DbType.Int32),
                                x.MaxLength,
                                true,
                                false,
                                false,
                                false,
                                Unique,
                                ForeignMapping.TableName,
                                x.ColumnName,
                                "",
                                "",
                                false,
                                false,
                                true);
            });
        }

        /// <summary>
        /// Cascades changes to the mapped instance.
        /// </summary>
        /// <returns>This</returns>
        public ReturnType CascadeChanges()
        {
            Cascade = true;
            return (ReturnType)((IMapProperty<ClassType, DataType, ReturnType>)this);
        }

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        public abstract IMapProperty Convert<TResult>(IMapping mapping)
            where TResult : class;

        /// <summary>
        /// Determines if the two objects are equal and returns true if they are, false otherwise
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            var SecondObj = obj as SingleClassPropertyBase<ClassType, DataType, ReturnType>;
            if (((object)SecondObj) == null)
                return false;
            return this == SecondObj;
        }

        /// <summary>
        /// Gets the property as an IParameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="objectValue"></param>
        /// <returns>The parameter version of the property</returns>
        public IEnumerable<IParameter> GetAsParameter(object objectValue)
        {
            var ParamValue = (DataType)GetParameter(objectValue);
            return ForeignMapping.IDProperties.ForEach<IIDProperty, IParameter>(x =>
            {
                var Value = x.GetValue(ParamValue);
                if (PropertyType == typeof(string))
                {
                    var TempParameter = Value as string;
                    return new StringParameter(ParentMapping.Prefix + Name + ParentMapping.Suffix + ForeignMapping.TableName + x.ColumnName,
                        TempParameter);
                }
                return new Parameter<object>(ParentMapping.Prefix + Name + ParentMapping.Suffix + ForeignMapping.TableName + x.ColumnName,
                    PropertyType.To<Type, SqlDbType>(),
                    Value);
            });
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
        /// Gets the property as a parameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="Object">Object to get the parameter from</param>
        /// <returns>The parameter version of the property</returns>
        public abstract object GetParameter(object Object);

        /// <summary>
        /// Gets the property as a parameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="Object">Object to get the parameter from</param>
        /// <returns>The parameter version of the property</returns>
        public abstract object GetParameter(Dynamo Object);

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="Object">The object.</param>
        /// <returns></returns>
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
        /// Gets the property's value from the object sent in
        /// </summary>
        /// <param name="Object">Object to get the value from</param>
        /// <returns>The value of the property</returns>
        public object GetValue(Dynamo Object)
        {
            return Object[Name];
        }

        /// <summary>
        /// Determines whether this instance is unique.
        /// </summary>
        /// <returns>this</returns>
        public ReturnType IsUnique()
        {
            Unique = true;
            return (ReturnType)((IMapProperty<ClassType, DataType, ReturnType>)this);
        }

        /// <summary>
        /// Loads the property using the query specified.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <param name="type">The type.</param>
        /// <returns>This</returns>
        public ReturnType LoadUsing(string queryText, CommandType type)
        {
            LoadPropertyQuery = new Query(type, queryText, QueryType.LoadProperty);
            return (ReturnType)((IMapProperty<ClassType, DataType, ReturnType>)this);
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        public abstract void Setup(MappingSource mappings);

        /// <summary>
        /// Checks if the properties are similar to one another
        /// </summary>
        /// <param name="secondProperty">The second property.</param>
        /// <returns>True if they are similar, false otherwise</returns>
        public bool Similar(IMapProperty secondProperty)
        {
            return secondProperty.ColumnName == ColumnName
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