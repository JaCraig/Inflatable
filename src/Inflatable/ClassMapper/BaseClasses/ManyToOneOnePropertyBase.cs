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
using Inflatable.ClassMapper.Column.Interfaces;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.BaseClasses
{
    /// <summary>
    /// Many to one single property base
    /// </summary>
    /// <typeparam name="TClassType">The type of the class type.</typeparam>
    /// <typeparam name="TDataType">The type of the data type.</typeparam>
    /// <typeparam name="TReturnType">The type of the return type.</typeparam>
    /// <seealso cref="IManyToOneProperty{ClassType, DataType, ReturnType}"/>
    /// <seealso cref="IManyToOneProperty{ClassType, DataType}"/>
    public abstract class ManyToOneOnePropertyBase<TClassType, TDataType, TReturnType> : IManyToOneProperty<TClassType, TDataType, TReturnType>, IManyToOneProperty<TClassType, TDataType>
        where TClassType : class
        where TDataType : class
        where TReturnType : IManyToOneProperty<TClassType, TDataType, TReturnType>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        protected ManyToOneOnePropertyBase(Expression<Func<TClassType, TDataType?>> expression, IMapping mapping)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            Name = expression.PropertyName();
            CompiledExpression = expression.Compile();
            Expression = expression;
            InternalFieldName = "_" + Name + "Derived";
            ParentMapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            PropertyType = typeof(TDataType);
            TypeName = PropertyType.GetName();
            ColumnName = "";
            ForeignMapping = [];
        }

        /// <summary>
        /// Gets a value indicating whether this is cascade.
        /// </summary>
        /// <value><c>true</c> if cascade; otherwise, <c>false</c>.</value>
        public bool Cascade { get; protected set; }

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        public string ColumnName { get; protected set; }

        /// <summary>
        /// Gets the columns associated with this property.
        /// </summary>
        /// <value>The columns associated with this property.</value>
        public IQueryColumnInfo[]? Columns { get; protected set; }

        /// <summary>
        /// Compiled version of the expression
        /// </summary>
        /// <value>The compiled expression.</value>
        public Func<TClassType, TDataType?> CompiledExpression { get; }

        /// <summary>
        /// Expression pointing to the property
        /// </summary>
        /// <value>The expression.</value>
        public Expression<Func<TClassType, TDataType?>> Expression { get; }

        /// <summary>
        /// Gets the foreign mapping.
        /// </summary>
        /// <value>The foreign mapping.</value>
        public List<IMapping> ForeignMapping { get; protected set; }

        /// <summary>
        /// Gets the name of the internal field.
        /// </summary>
        /// <value>The name of the internal field.</value>
        public string InternalFieldName { get; }

        /// <summary>
        /// Gets the load property query.
        /// </summary>
        /// <value>The load property query.</value>
        public Query? LoadPropertyQuery { get; protected set; }

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
        public Type PropertyType { get; }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        public string TypeName { get; }

        /// <summary>
        /// Gets or sets a value indicating whether [on delete do nothing].
        /// </summary>
        /// <value><c>true</c> if [on delete do nothing]; otherwise, <c>false</c>.</value>
        protected bool OnDeleteDoNothingValue { get; set; }

        /// <summary>
        /// != operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>returns true if they are not equal, false otherwise</returns>
        public static bool operator !=(ManyToOneOnePropertyBase<TClassType, TDataType, TReturnType> first, ManyToOneOnePropertyBase<TClassType, TDataType, TReturnType> second)
        {
            return !(first == second);
        }

        /// <summary>
        /// The &lt; operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>True if the first item is less than the second, false otherwise</returns>
        public static bool operator <(ManyToOneOnePropertyBase<TClassType, TDataType, TReturnType> first, ManyToOneOnePropertyBase<TClassType, TDataType, TReturnType> second)
        {
            return !ReferenceEquals(first, second) && first is not null && second is not null && first.GetHashCode() < second.GetHashCode();
        }

        /// <summary>
        /// The == operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>true if the first and second item are the same, false otherwise</returns>
        public static bool operator ==(ManyToOneOnePropertyBase<TClassType, TDataType, TReturnType> first, ManyToOneOnePropertyBase<TClassType, TDataType, TReturnType> second)
        {
            return ReferenceEquals(first, second) || (first is not null && second is not null && first.GetHashCode() == second.GetHashCode());
        }

        /// <summary>
        /// The &gt; operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>True if the first item is greater than the second, false otherwise</returns>
        public static bool operator >(ManyToOneOnePropertyBase<TClassType, TDataType, TReturnType> first, ManyToOneOnePropertyBase<TClassType, TDataType, TReturnType> second)
        {
            return !ReferenceEquals(first, second) && first is not null && second is not null && first.GetHashCode() > second.GetHashCode();
        }

        /// <summary>
        /// Cascades changes to the mapped instance.
        /// </summary>
        /// <returns>This</returns>
        public TReturnType CascadeChanges()
        {
            Cascade = true;
            return (TReturnType)((IManyToOneProperty<TClassType, TDataType, TReturnType>)this);
        }

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        public abstract IManyToOneProperty Convert<TResult>(IMapping mapping)
            where TResult : class;

        /// <summary>
        /// Determines if the two objects are equal and returns true if they are, false otherwise
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public override bool Equals(object? obj) => (obj is ManyToOneOnePropertyBase<TClassType, TDataType, TReturnType> SecondObj) && this == SecondObj;

        /// <summary>
        /// Gets the column information.
        /// </summary>
        /// <returns>The column information.</returns>
        public IQueryColumnInfo[] GetColumnInfo() => Columns ?? [];

        /// <summary>
        /// Returns the hash code for the property
        /// </summary>
        /// <returns>The hash code for the property</returns>
        public override int GetHashCode() => (Name.GetHashCode(StringComparison.InvariantCulture) * ParentMapping.GetHashCode()) % int.MaxValue;

        /// <summary>
        /// Gets the property's value from the object sent in
        /// </summary>
        /// <param name="modelObject">Object to get the value from</param>
        /// <returns>The value of the property</returns>
        public object? GetValue(object modelObject) => modelObject is not TClassType TempObject ? null : CompiledExpression(TempObject);

        /// <summary>
        /// Loads the property using the query specified.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <param name="type">The type.</param>
        /// <returns>This</returns>
        public TReturnType LoadUsing(string queryText, CommandType type)
        {
            LoadPropertyQuery = new Query(PropertyType, type, queryText, QueryType.LoadProperty);
            return (TReturnType)((IManyToOneProperty<TClassType, TDataType, TReturnType>)this);
        }

        /// <summary>
        /// Called when you want to override the default referential integrity and do nothing on delete.
        /// </summary>
        /// <returns>This</returns>
        public TReturnType OnDeleteDoNothing()
        {
            OnDeleteDoNothingValue = true;
            return (TReturnType)((IManyToOneProperty<TClassType, TDataType, TReturnType>)this);
        }

        /// <summary>
        /// Sets the column information.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        public abstract void SetColumnInfo(IMappingSource mappings);

        /// <summary>
        /// Sets the name of the column.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>This</returns>
        public TReturnType SetColumnName(string columnName)
        {
            ColumnName = columnName;
            return (TReturnType)((IManyToOneProperty<TClassType, TDataType, TReturnType>)this);
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="sourceSpec">The source spec.</param>
        public abstract void Setup(IMappingSource mappings, ISource sourceSpec);

        /// <summary>
        /// Checks if the properties are similar to one another
        /// </summary>
        /// <param name="secondProperty">The second property.</param>
        /// <returns>True if they are similar, false otherwise</returns>
        public bool Similar(IManyToOneProperty secondProperty) => secondProperty?.Name == Name;

        /// <summary>
        /// Gets the property as a string
        /// </summary>
        /// <returns>The string representation of the property</returns>
        public override string ToString() => PropertyType.GetName() + " " + ParentMapping + "." + Name;
    }
}