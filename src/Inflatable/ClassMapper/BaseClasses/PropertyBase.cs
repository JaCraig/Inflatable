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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Inflatable.ClassMapper.BaseClasses
{
    /// <summary>
    /// Property base class
    /// </summary>
    /// <typeparam name="TClassType">The type of the class type.</typeparam>
    /// <typeparam name="TDataType">The type of the data type.</typeparam>
    /// <typeparam name="TReturnType">The type of the return type.</typeparam>
    /// <seealso cref="IProperty{ClassType, DataType, ReturnType}"/>
    /// <seealso cref="IProperty{ClassType, DataType}"/>
    public abstract class PropertyBase<TClassType, TDataType, TReturnType> : IProperty<TClassType, TDataType, TReturnType>, IProperty<TClassType, TDataType>
        where TClassType : class
        where TReturnType : IProperty<TClassType, TDataType, TReturnType>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        protected PropertyBase(Expression<Func<TClassType, TDataType>> expression, IMapping mapping)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (mapping is null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }

            var DataTypeInfo = typeof(TDataType);

            Name = expression.PropertyName();
            ColumnName = mapping.Prefix + Name + mapping.Suffix;
            CompiledExpression = expression.Compile();
            Constraints = [];
            DefaultValue = () => default!;
            Expression = expression;
            SetAction = Expression.PropertySetter<TClassType, TDataType>()?.Compile() ?? new Action<TClassType, TDataType>((_, __) => { });
            InternalFieldName = "_" + Name + "Derived";
            var PropertyInfo = typeof(TClassType).GetProperty<TClassType>(Name);

            var Attributes = PropertyInfo?.GetCustomAttributes();
            var StringLengthAttribute = (Attributes?.FirstOrDefault(x => x is StringLengthAttribute) as StringLengthAttribute);

            if (Attributes?.FirstOrDefault(x => x is MaxLengthAttribute) is MaxLengthAttribute MaxLength)
                this.MaxLength = MaxLength.Length;
            else if (StringLengthAttribute is not null)
                this.MaxLength = StringLengthAttribute.MaximumLength;
            else
                this.MaxLength = typeof(TDataType) == typeof(string) || typeof(TDataType) == typeof(Uri) ? 100 : 0;

            var MinLength = Attributes?.FirstOrDefault(x => x is MinLengthAttribute) is MinLengthAttribute { Length: > 0 }
                || StringLengthAttribute?.MinimumLength > 0;

            Nullable = ((typeof(TDataType) == typeof(string)
                    || typeof(TDataType) == typeof(Uri)
                    || typeof(TDataType) == typeof(byte[]))
                        && Attributes?.Any(x => x is RequiredAttribute) == false
                        && !MinLength)
                || (DataTypeInfo.IsGenericType && DataTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>));

            ParentMapping = mapping;
            PropertyType = typeof(TDataType);
            TypeName = PropertyType.GetName();
            ComputedColumnSpecification = "";
        }

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
        /// Gets a value indicating whether this <see cref="IProperty"/> is indexed.
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
        /// Gets a value indicating whether this <see cref="IProperty"/> is nullable.
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
        /// Gets a value indicating whether this <see cref="IProperty"/> is unique.
        /// </summary>
        /// <value><c>true</c> if unique; otherwise, <c>false</c>.</value>
        public bool Unique { get; private set; }

        /// <summary>
        /// Gets or sets the expression used to set the value.
        /// </summary>
        /// <value>The set expression.</value>
        protected Action<TClassType, TDataType> SetAction { get; set; }

        /// <summary>
        /// != operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>returns true if they are not equal, false otherwise</returns>
        public static bool operator !=(PropertyBase<TClassType, TDataType, TReturnType> first, PropertyBase<TClassType, TDataType, TReturnType> second)
        {
            return !(first == second);
        }

        /// <summary>
        /// The &lt; operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>True if the first item is less than the second, false otherwise</returns>
        public static bool operator <(PropertyBase<TClassType, TDataType, TReturnType> first, PropertyBase<TClassType, TDataType, TReturnType> second)
        {
            return !ReferenceEquals(first, second) && first is not null && second is not null && first.GetHashCode() < second.GetHashCode();
        }

        /// <summary>
        /// The == operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>true if the first and second item are the same, false otherwise</returns>
        public static bool operator ==(PropertyBase<TClassType, TDataType, TReturnType> first, PropertyBase<TClassType, TDataType, TReturnType> second)
        {
            return ReferenceEquals(first, second) || (first is not null && second is not null && first.GetHashCode() == second.GetHashCode());
        }

        /// <summary>
        /// The &gt; operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>True if the first item is greater than the second, false otherwise</returns>
        public static bool operator >(PropertyBase<TClassType, TDataType, TReturnType> first, PropertyBase<TClassType, TDataType, TReturnType> second)
        {
            return !ReferenceEquals(first, second) && first is not null && second is not null && first.GetHashCode() > second.GetHashCode();
        }

        /// <summary>
        /// Adds to table.
        /// </summary>
        /// <param name="table">The table.</param>
        public void AddToTable(ITable table)
        {
            table.AddColumn(ColumnName,
                PropertyType.To<DbType>(),
                MaxLength,
                Nullable,
                false,
                Index,
                false,
                Unique,
                "",
                "",
                DefaultValue(),
                ComputedColumnSpecification);
        }

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        public abstract IProperty Convert<TResult>(IMapping mapping)
            where TResult : class;

        /// <summary>
        /// Determines if the two objects are equal and returns true if they are, false otherwise
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public override bool Equals(object? obj) => (obj is PropertyBase<TClassType, TDataType, TReturnType> SecondObj) && this == SecondObj;

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

            return Columns ?? [];
        }

        /// <summary>
        /// Returns the hash code for the property
        /// </summary>
        /// <returns>The hash code for the property</returns>
        public override int GetHashCode() => Name.GetHashCode() * ParentMapping.GetHashCode() % int.MaxValue;

        /// <summary>
        /// Determines whether this instance is indexed.
        /// </summary>
        /// <returns>this</returns>
        public TReturnType IsIndexed()
        {
            Index = true;
            return (TReturnType)(IProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Determines whether [is read only].
        /// </summary>
        /// <returns>this</returns>
        public TReturnType IsReadOnly()
        {
            ReadOnly = true;
            return (TReturnType)(IProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Determines whether this instance is unique.
        /// </summary>
        /// <returns>this</returns>
        public TReturnType IsUnique()
        {
            Unique = true;
            return (TReturnType)(IProperty<TClassType, TDataType, TReturnType>)this;
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
        /// Checks if the properties are similar to one another
        /// </summary>
        /// <param name="secondProperty">The second property.</param>
        /// <returns>True if they are similar, false otherwise</returns>
        public bool Similar(IProperty secondProperty)
        {
            return secondProperty.ColumnName == ColumnName
                && secondProperty.Name == Name;
        }

        /// <summary>
        /// Gets the property as a string
        /// </summary>
        /// <returns>The string representation of the property</returns>
        public override string ToString() => PropertyType.GetName() + " " + ParentMapping + "." + Name;

        /// <summary>
        /// Sets the name of the field in the database.
        /// </summary>
        /// <param name="columnName">Name of the field.</param>
        /// <returns>this</returns>
        public TReturnType WithColumnName(string columnName)
        {
            ColumnName = columnName;
            return (TReturnType)(IProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Sets the computed column specification if the source allows it.
        /// </summary>
        /// <param name="computedColumnSpecification">The computed column specification.</param>
        /// <returns>this</returns>
        public TReturnType WithComputedColumnSpecification(string computedColumnSpecification)
        {
            ComputedColumnSpecification = computedColumnSpecification;
            return (TReturnType)(IProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Sets a constraint on the field if the source allows it.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns>this</returns>
        public TReturnType WithConstraint(string constraint)
        {
            Constraints.Add(constraint);
            return (TReturnType)(IProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Sets the default value of the property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>this</returns>
        public TReturnType WithDefaultValue(Func<TDataType> value)
        {
            DefaultValue = value;
            return (TReturnType)(IProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Sets the max length.
        /// </summary>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>This</returns>
        public TReturnType WithMaxLength(int maxLength)
        {
            MaxLength = maxLength;
            return (TReturnType)(IProperty<TClassType, TDataType, TReturnType>)this;
        }

        /// <summary>
        /// Sets the length for the property to MAX.
        /// </summary>
        /// <returns>this.</returns>
        public TReturnType WithMaxLength() => WithMaxLength(-1);
    }
}