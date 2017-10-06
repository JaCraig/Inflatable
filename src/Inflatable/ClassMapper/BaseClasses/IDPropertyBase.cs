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
using SQLHelper.HelperClasses;
using SQLHelper.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Inflatable.ClassMapper.BaseClasses
{
    /// <summary>
    /// ID property base class
    /// </summary>
    /// <typeparam name="ClassType">The type of the lass type.</typeparam>
    /// <typeparam name="DataType">The type of the ata type.</typeparam>
    /// <typeparam name="ReturnType">The type of the eturn type.</typeparam>
    /// <seealso cref="Inflatable.ClassMapper.Interfaces.IIDProperty{ClassType, DataType, ReturnType}"/>
    /// <seealso cref="Inflatable.ClassMapper.Interfaces.IIDProperty{ClassType, DataType}"/>
    public abstract class IDPropertyBase<ClassType, DataType, ReturnType> : IIDProperty<ClassType, DataType, ReturnType>, IIDProperty<ClassType, DataType>
        where ClassType : class
        where ReturnType : IIDProperty<ClassType, DataType, ReturnType>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        protected IDPropertyBase(Expression<Func<ClassType, DataType>> expression, IMapping mapping)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));
            var DataTypeInfo = typeof(DataType).GetTypeInfo();

            Name = expression.PropertyName();
            ColumnName = mapping.Prefix + Name + mapping.Suffix;
            CompiledExpression = expression.Compile();
            Constraints = new List<string>();
            ComputedColumnSpecification = "";
            DefaultValue = () => default(DataType);
            Expression = expression;
            SetAction = Expression.PropertySetter<ClassType, DataType>().Compile();
            InternalFieldName = "_" + Name + "Derived";
            MaxLength = typeof(DataType) == typeof(string) ? 100 : 0;
            ParentMapping = mapping;
            PropertyType = typeof(DataType);
            TypeName = PropertyType.GetName();
            Index = true;
            Unique = true;
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
        public IQueryColumnInfo[] Columns { get; protected set; }

        /// <summary>
        /// Compiled version of the expression
        /// </summary>
        public Func<ClassType, DataType> CompiledExpression { get; private set; }

        /// <summary>
        /// Gets the computed column specification.
        /// </summary>
        /// <value>The computed column specification.</value>
        public string ComputedColumnSpecification { get; private set; }

        /// <summary>
        /// Gets the constraints if the data source supports them.
        /// </summary>
        /// <value>The constraints if the data source supports them.</value>
        public IList<string> Constraints { get; private set; }

        /// <summary>
        /// Default value for this property
        /// </summary>
        public Func<DataType> DefaultValue { get; private set; }

        /// <summary>
        /// Expression pointing to the property
        /// </summary>
        public Expression<Func<ClassType, DataType>> Expression { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IIDProperty"/> is indexed.
        /// </summary>
        /// <value><c>true</c> if index; otherwise, <c>false</c>.</value>
        public bool Index { get; private set; }

        /// <summary>
        /// Gets the name of the internal field.
        /// </summary>
        /// <value>The name of the internal field.</value>
        public string InternalFieldName { get; private set; }

        /// <summary>
        /// Gets the maximum length.
        /// </summary>
        /// <value>The maximum length.</value>
        public int MaxLength { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IIDProperty"/> is nullable.
        /// </summary>
        /// <value><c>true</c> if nullable; otherwise, <c>false</c>.</value>
        public bool Nullable { get; private set; }

        /// <summary>
        /// Gets the parent mapping.
        /// </summary>
        /// <value>The parent mapping.</value>
        public IMapping ParentMapping { get; private set; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [read only].
        /// </summary>
        /// <value><c>true</c> if [read only]; otherwise, <c>false</c>.</value>
        public bool ReadOnly { get; private set; }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IIDProperty"/> is unique.
        /// </summary>
        /// <value><c>true</c> if unique; otherwise, <c>false</c>.</value>
        public bool Unique { get; private set; }

        /// <summary>
        /// Gets or sets the expression used to set the value.
        /// </summary>
        /// <value>The set expression.</value>
        protected Action<ClassType, DataType> SetAction { get; set; }

        /// <summary>
        /// != operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>returns true if they are not equal, false otherwise</returns>
        public static bool operator !=(IDPropertyBase<ClassType, DataType, ReturnType> first, IDPropertyBase<ClassType, DataType, ReturnType> second)
        {
            return !(first == second);
        }

        /// <summary>
        /// The &lt; operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>True if the first item is less than the second, false otherwise</returns>
        public static bool operator <(IDPropertyBase<ClassType, DataType, ReturnType> first, IDPropertyBase<ClassType, DataType, ReturnType> second)
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
        public static bool operator ==(IDPropertyBase<ClassType, DataType, ReturnType> first, IDPropertyBase<ClassType, DataType, ReturnType> second)
        {
            if (ReferenceEquals(first, second))
                return true;

            if ((object)first == null || (object)second == null)
                return false;

            return first.GetHashCode() == second.GetHashCode();
        }

        /// <summary>
        /// The &gt; operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>True if the first item is greater than the second, false otherwise</returns>
        public static bool operator >(IDPropertyBase<ClassType, DataType, ReturnType> first, IDPropertyBase<ClassType, DataType, ReturnType> second)
        {
            if (ReferenceEquals(first, second))
                return false;
            if ((object)first == null || (object)second == null)
                return false;
            return first.GetHashCode() > second.GetHashCode();
        }

        /// <summary>
        /// Adds to a child table.
        /// </summary>
        /// <param name="table">The table.</param>
        public void AddToChildTable(ITable table)
        {
            table.AddColumn(ParentMapping.TableName + ColumnName,
                PropertyType.To(DbType.Int32),
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
            table.AddColumn(ColumnName,
                PropertyType.To(DbType.Int32),
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
        public override bool Equals(object obj)
        {
            var SecondObj = obj as IDPropertyBase<ClassType, DataType, ReturnType>;
            if (((object)SecondObj) == null)
                return false;
            return this == SecondObj;
        }

        /// <summary>
        /// Gets the property as an IParameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="objectValue"></param>
        /// <returns>The parameter version of the property</returns>
        public IParameter GetAsParameter(object objectValue)
        {
            var ParamValue = GetParameter(objectValue);
            var TempParameter = ParamValue as string;
            if (PropertyType == typeof(string))
                return new StringParameter(Name, TempParameter);
            return new Parameter<object>(Name, PropertyType.To<Type, SqlDbType>(), ParamValue);
        }

        /// <summary>
        /// Gets the column information.
        /// </summary>
        /// <returns>The column information.</returns>
        public IQueryColumnInfo[] GetColumnInfo()
        {
            return Columns;
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        /// <returns>The default value</returns>
        public object GetDefaultValue()
        {
            return DefaultValue();
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
        /// Determines whether this [is auto incremented].
        /// </summary>
        /// <returns>this</returns>
        public ReturnType IsAutoIncremented()
        {
            AutoIncrement = true;
            return (ReturnType)((IIDProperty<ClassType, DataType, ReturnType>)this);
        }

        /// <summary>
        /// Determines whether the specified object's property is default.
        /// </summary>
        /// <param name="Object">The object.</param>
        /// <returns><c>true</c> if the specified object's property is default; otherwise, <c>false</c>.</returns>
        public bool IsDefault(object Object)
        {
            return Equals(GetValue(Object), default(DataType));
        }

        /// <summary>
        /// Determines whether this instance is indexed.
        /// </summary>
        /// <returns>this</returns>
        public ReturnType IsIndexed()
        {
            Index = true;
            return (ReturnType)((IIDProperty<ClassType, DataType, ReturnType>)this);
        }

        /// <summary>
        /// Determines whether [is read only].
        /// </summary>
        /// <returns>this</returns>
        public ReturnType IsReadOnly()
        {
            ReadOnly = true;
            return (ReturnType)((IIDProperty<ClassType, DataType, ReturnType>)this);
        }

        /// <summary>
        /// Determines whether this instance is unique.
        /// </summary>
        /// <returns>this</returns>
        public ReturnType IsUnique()
        {
            Unique = true;
            return (ReturnType)((IIDProperty<ClassType, DataType, ReturnType>)this);
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        public abstract void Setup();

        /// <summary>
        /// Sets the property's value for the object sent in.
        /// </summary>
        /// <param name="objectToSet">The object to set.</param>
        /// <param name="propertyValue">The property value.</param>
        public void SetValue(object objectToSet, object propertyValue)
        {
            var TempObject = objectToSet as ClassType;
            var TempPropertyValue = (DataType)propertyValue;
            SetAction(TempObject, TempPropertyValue);
        }

        /// <summary>
        /// Gets the property as a string
        /// </summary>
        /// <returns>The string representation of the property</returns>
        public override string ToString()
        {
            return PropertyType.GetName() + " " + ParentMapping + "." + Name;
        }

        /// <summary>
        /// Sets the name of the field in the database.
        /// </summary>
        /// <param name="columnName">Name of the field.</param>
        /// <returns>this</returns>
        public ReturnType WithColumnName(string columnName)
        {
            ColumnName = columnName;
            return (ReturnType)((IIDProperty<ClassType, DataType, ReturnType>)this);
        }

        /// <summary>
        /// Sets the computed column specification if the source allows it.
        /// </summary>
        /// <param name="computedColumnSpecification">The computed column specification.</param>
        /// <returns>this</returns>
        public ReturnType WithComputedColumnSpecification(string computedColumnSpecification)
        {
            ComputedColumnSpecification = computedColumnSpecification;
            return (ReturnType)((IIDProperty<ClassType, DataType, ReturnType>)this);
        }

        /// <summary>
        /// Sets a constraint on the field if the source allows it.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        /// <returns>this</returns>
        public ReturnType WithConstraint(string constraint)
        {
            Constraints.Add(constraint);
            return (ReturnType)((IIDProperty<ClassType, DataType, ReturnType>)this);
        }

        /// <summary>
        /// Sets the default value of the property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>this</returns>
        public ReturnType WithDefaultValue(Func<DataType> value)
        {
            DefaultValue = value;
            return (ReturnType)((IIDProperty<ClassType, DataType, ReturnType>)this);
        }

        /// <summary>
        /// Sets the max length.
        /// </summary>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>This</returns>
        public ReturnType WithMaxLength(int maxLength)
        {
            MaxLength = maxLength;
            return (ReturnType)((IIDProperty<ClassType, DataType, ReturnType>)this);
        }
    }
}