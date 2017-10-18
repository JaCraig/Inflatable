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

using Inflatable.ClassMapper.BaseClasses;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.Utils;
using System;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.Default
{
    /// <summary>
    /// ID property
    /// </summary>
    /// <typeparam name="ClassType">The type of the lass type.</typeparam>
    /// <typeparam name="DataType">The type of the ata type.</typeparam>
    /// <seealso cref="IIDProperty"/>
    public class ID<ClassType, DataType> : IDPropertyBase<ClassType, DataType, ID<ClassType, DataType>>, IIDProperty
        where ClassType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ID{ClassType, DataType}"/> class.
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        public ID(Expression<Func<ClassType, DataType>> expression, IMapping mapping) : base(expression, mapping)
        {
        }

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        public override IIDProperty Convert<TResult>(IMapping mapping)
        {
            var Result = new ExpressionTypeConverter<ClassType, DataType>
            {
                Expression = Expression
            }.Convert<TResult>();
            var ReturnObject = new ID<TResult, DataType>(Result, mapping);
            if (Index)
                ReturnObject.IsIndexed();
            if (ReadOnly)
                ReturnObject.IsReadOnly();
            if (Unique)
                ReturnObject.IsUnique();
            ReturnObject.WithColumnName(ColumnName);
            ReturnObject.WithComputedColumnSpecification(ComputedColumnSpecification);
            foreach (var Constraint in Constraints)
            {
                ReturnObject.WithConstraint(Constraint);
            }
            if (AutoIncrement)
                ReturnObject.IsAutoIncremented();
            ReturnObject.WithDefaultValue(DefaultValue);
            ReturnObject.WithMaxLength(MaxLength);
            return ReturnObject;
        }

        /// <summary>
        /// Sets the column information.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        public override void SetColumnInfo(MappingSource mappings)
        {
            Setup();
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        public override void Setup()
        {
            if (Columns != null)
                return;
            Columns = new Column.SimpleColumnInfo<ClassType, DataType>[]
            {
                new Column.SimpleColumnInfo<ClassType,DataType>
                {
                    ColumnName=ColumnName,
                    DefaultValue=()=>default(DataType),
                    CompiledExpression=CompiledExpression,
                    PropertyName=Name,
                    PropertyType=PropertyType,
                    SchemaName=ParentMapping.SchemaName,
                    TableName=ParentMapping.TableName,
                    SetAction=SetAction,
                    IsNullable=true,
                    IsForeign=false
                }
            };
        }
    }
}