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
using ObjectCartographer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.BaseClasses
{
    /// <summary>
    /// ID property base class
    /// </summary>
    /// <typeparam name="TClassType">The type of the lass type.</typeparam>
    /// <typeparam name="TDataType">The type of the ata type.</typeparam>
    /// <typeparam name="TReturnType">The type of the eturn type.</typeparam>
    /// <seealso cref="IIDProperty{ClassType, DataType, ReturnType}"/>
    /// <seealso cref="IIDProperty{ClassType, DataType}"/>
    public abstract class IDPropertyBase<TClassType, TDataType, TReturnType> : IIDProperty<TClassType, TDataType, TReturnType>, IIDProperty<TClassType, TDataType>
        where TClassType : class
        where TReturnType : IIDProperty<TClassType, TDataType, TReturnType>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        protected IDPropertyBase(Expression<Func<TClassType, TDataType>> expression, IMapping mapping)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            if (mapping is null)
                throw new ArgumentNullException(nameof(mapping));

            Name = expression.PropertyName();
            ColumnName = mapping.Prefix + Name + mapping.Suffix;
            CompiledExpression = expression.Compile();
            Constraints = new List<string>();
            ComputedColumnSpecification = string.Empty;
            DefaultValue = DefaultDefaultValue;
            Expression = expression;
            SetAction = Expression.PropertySetter<TClassType, TDataType>()?.Compile() ?? DefaultSetAction;
            InternalFieldName = $"_{Name}Derived";
            PropertyType = typeof(TDataType);
            MaxLength = PropertyType == typeof(string) ? 100 : 0;
            ParentMapping = mapping;
            TypeName = PropertyType.GetName();
            Index = true;
            Unique = true;
            _HashCode = Name.GetHashCode(StringComparison.Ordinal) * ParentMapping.GetHashCode() % int.MaxValue;
            _ToString = $"{PropertyType.GetName()} {ParentMapping}.{Name}";
        }

        /// <summary>
        /// Gets a value indicating whether to [automatic increment].
        /// </summary>
        /// <value><c>true</c> if [automatic increment]; otherwise, <c>false</c>.</value>
        public bool AutoIncrement { get; private set; }

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        public string ColumnName { get; private set; }

        /// <summary>
        /// Gets the columns associated with this property.
        /// </summary>
        /// <value>The columns associated with this property.</value>
        public IQueryColumnInfo[]? Columns { get; protected set; }

        /// <summary>
        /// Compiled version of the expression
        /// </summary>
        public Func<TClassType, TDataType> CompiledExpression { get; }

        /// <summary>
        /// Gets the computed column specification.
        /// </summary>
        /// <value>The computed column specification.</value>
        public string ComputedColumnSpecification { get; private set; }

        /// <summary>
        /// Gets the constraints if the data source supports them.
        /// </summary>
        /// <value>The constraints if the data source supports them.</value>
        public IList<string> Constraints { get; }

        /// <summary>
        /// Default value for this property
        /// </summary>
        public Func<TDataType> DefaultValue { get; private set; }

        /// <summary>
        /// Expression pointing to the property
        /// </summary>
        public Expression<Func<TClassType, TDataType>> Expression { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IIDProperty"/> is indexed.
        /// </summary>
        /// <value><c>true</c> if index; otherwise, <c>false</c>.</value>
        public bool Index { get; private set; }

        /// <summary>
        /// Gets the name of the internal field.
        /// </summary>
        /// <value>The name of the internal field.</value>
        public string InternalFieldName { get; }

        /// <summary>
        /// Gets the maximum length.
        /// </summary>
        /// <value>The maximum length.</value>
        public int MaxLength { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IIDProperty"/> is nullable.
        /// </summary>
        /// <value><c>true</c> if nullable; otherwise, <c>false</c>.</value>
        public bool Nullable { get; }

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
        /// Gets a value indicating whether [read only].
        /// </summary>
        /// <value><c>true</c> if [read only]; otherwise, <c>false</c>.</value>
        public bool ReadOnly { get; private set; }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        public string TypeName { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IIDProperty"/> is unique.
        /// </summary>
        /// <value><c>true</c> if unique; otherwise, <c>false</c>.</value>
        public bool Unique { get; private set; }

        /// <summary>
        /// Gets or sets the expression used to set the value.
        /// </summary>
        /// <value>The set expression.</value>
        protected Action<TClassType, TDataType> SetAction { get; set; }

        /// <summary>
        /// The hash code
        /// </summary>
        private readonly int _HashCode;

        /// <summary>
        /// To string
        /// </summary>
        private readonly string _ToString;

        /// <summary>
        /// != operator
        /// </summary>
        /// <param name="left">left item</param>
        /// <param name="right">right item</param>
        /// <returns>returns true if they are not equal, false otherwise</returns>
        public static bool operator !=(IDPropertyBase<TClassType, TDataType, TReturnType> left, IDPropertyBase<TClassType, TDataType, TReturnType> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// The &lt; operator
        /// </summary>
        /// <param name="left">left item</param>
        /// <param name="right">right item</param>
        /// <returns>True if the left item is less than the right, false otherwise</returns>
        public static bool operator <(IDPropertyBase<TClassType, TDataType, TReturnType> left, IDPropertyBase<TClassType, TDataType, TReturnType> right)
        {
            return !ReferenceEquals(left, right)
                && !(left is null)
                && !(right is null)
                && left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator <=(IDPropertyBase<TClassType, TDataType, TReturnType> left, IDPropertyBase<TClassType, TDataType, TReturnType> right)
        {
            return ReferenceEquals(left, right)
                || (!(left is null)
                && !(right is null)
                && left.CompareTo(right) <= 0);
        }

        /// <summary>
        /// The == operator
        /// </summary>
        /// <param name="left">left item</param>
        /// <param name="right">right item</param>
        /// <returns>true if the left and right item are the same, false otherwise</returns>
        public static bool operator ==(IDPropertyBase<TClassType, TDataType, TReturnType> left, IDPropertyBase<TClassType, TDataType, TReturnType> right)
        {
            return ReferenceEquals(left, right)
                || (!(left is null)
                    && !(right is null)
                    && left.CompareTo(right) == 0);
        }

        /// <summary>
        /// The &gt; operator
        /// </summary>
        /// <param name="left">left item</param>
        /// <param name="right">right item</param>
        /// <returns>True if the left item is greater than the right, false otherwise</returns>
        public static bool operator >(IDPropertyBase<TClassType, TDataType, TReturnType> left, IDPropertyBase<TClassType, TDataType, TReturnType> right)
        {
            return !ReferenceEquals(left, right)
                && !(left is null)
                && !(right is null)
                && left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator >=(IDPropertyBase<TClassType, TDataType, TReturnType> left, IDPropertyBase<TClassType, TDataType, TReturnType> right)
        {
            return ReferenceEquals(left, right)
                || (!(left is null)
                && !(right is null)
                && left.CompareTo(right) >= 0);
        }

        /// <summary>
        /// Adds to a child table.
        /// </summary>
        /// <param name="table">The table.</param>
        public void AddToChildTable(ITable table)
        {
            table?.AddColumn(ParentMapping.TableName + ColumnName,
                PropertyType.To<DbType>(),
                MaxLength,
                Nullable,
                false,
                Index,
                false,
                true,
                ParentMapping.TableName,
                ColumnName,
                DefaultValue(),
                ComputedColumnSpecification,
                true,
                true);
        }

        /// <summary>
        /// Adds to table.
        /// </summary>
        /// <param name="table">The table.</param>
        public void AddToTable(ITable table)
        {
            table?.AddColumn(ColumnName,
                PropertyType.To<DbType>(),
                MaxLength,
                Nullable,
                AutoIncrement,
                Index,
                true,
                false,
                "",
                "",
                DefaultValue(),
                ComputedColumnSpecification);
        }

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>1 if it's greater, 0 if they're equal, -1 if it's less than.</returns>
        public virtual int CompareTo(IDPropertyBase<TClassType, TDataType, TReturnType>? other) => other is null ? 1 : GetHashCode().CompareTo(other.GetHashCode());

        /// <summary>
        /// Compares the object to another object
        /// </summary>
        /// <param name="other">Object to compare to</param>
        /// <returns>0 if they are equal, -1 if this is smaller, 1 if it is larger</returns>
        public int CompareTo(object? other) => other is IDPropertyBase<TClassType, TDataType, TReturnType> objectBaseClass ? CompareTo(objectBaseClass) : 1;

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        public abstract IIDProperty Convert<TResult>(IMapping mapping)
            where TResult : class;

        /// <summary>
        /// Determines if the two objects are equal and returns true if they are, false otherwise
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public override bool Equals(object obj) => (obj is IDPropertyBase<TClassType, TDataType, TReturnType> rightObj) && CompareTo(rightObj) == 0;

        /// <summary>
        /// Gets the column information.
        /// </summary>
        /// <returns>The column information.</returns>
        public IQueryColumnInfo[] GetColumnInfo()
        {
            if (Columns is null)
            {
                SetColumnInfo(null);
            }

            return Columns ?? Array.Empty<IQueryColumnInfo>();
        }

        /// <summary>
        /// Returns the hash code for the property
        /// </summary>
        /// <returns>The hash code for the property</returns>
        public override int GetHashCode() => _HashCode;

        /// <summary>
        /// Determines whether this [is auto incremented].
        /// </summary>
        /// <returns>this</returns>
        public TReturnType IsAutoIncremented()
        {
            AutoIncrement = true;
            return (TReturnType)(IIDProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Determines whether this instance is indexed.
        /// </summary>
        /// <returns>this</returns>
        public TReturnType IsIndexed()
        {
            Index = true;
            return (TReturnType)(IIDProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Determines whether [is read only].
        /// </summary>
        /// <returns>this</returns>
        public TReturnType IsReadOnly()
        {
            ReadOnly = true;
            return (TReturnType)(IIDProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Determines whether this instance is unique.
        /// </summary>
        /// <returns>this</returns>
        public TReturnType IsUnique()
        {
            Unique = true;
            return (TReturnType)(IIDProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Sets the column information.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        public abstract void SetColumnInfo(IMappingSource? mappings);

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        public abstract void Setup();

        /// <summary>
        /// Gets the property as a string
        /// </summary>
        /// <returns>The string representation of the property</returns>
        public override string ToString() => _ToString;

        /// <summary>
        /// Sets the name of the field in the database.
        /// </summary>
        /// <param name="columnName">Name of the field.</param>
        /// <returns>this</returns>
        public TReturnType WithColumnName(string columnName)
        {
            ColumnName = columnName;
            return (TReturnType)(IIDProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Sets the computed column specification if the source allows it.
        /// </summary>
        /// <param name="computedColumnSpecification">The computed column specification.</param>
        /// <returns>this</returns>
        public TReturnType WithComputedColumnSpecification(string computedColumnSpecification)
        {
            ComputedColumnSpecification = computedColumnSpecification;
            return (TReturnType)(IIDProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Sets a constraint on the field if the source allows it.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns>this</returns>
        public TReturnType WithConstraint(string constraint)
        {
            Constraints.Add(constraint);
            return (TReturnType)(IIDProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Sets the default value of the property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>this</returns>
        public TReturnType WithDefaultValue(Func<TDataType> value)
        {
            DefaultValue = value;
            return (TReturnType)(IIDProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Sets the max length.
        /// </summary>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>This</returns>
        public TReturnType WithMaxLength(int maxLength)
        {
            MaxLength = maxLength;
            return (TReturnType)(IIDProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Sets the length for the property to MAX.
        /// </summary>
        /// <returns>this.</returns>
        public TReturnType WithMaxLength() => WithMaxLength(-1);

        /// <summary>
        /// The "Default" default value method.
        /// </summary>
        /// <returns>The default value.</returns>
        private static TDataType DefaultDefaultValue() => default!;

        /// <summary>
        /// Default set action.
        /// </summary>
        /// <param name="_">The class</param>
        /// <param name="__">The data</param>
        private static void DefaultSetAction(TClassType _, TDataType __) { }
    }
}