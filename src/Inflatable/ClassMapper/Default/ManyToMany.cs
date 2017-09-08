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
using Inflatable.ClassMapper.BaseClasses;
using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using Inflatable.Schema;
using Inflatable.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.Default
{
    /// <summary>
    /// Many to many mapping type.
    /// </summary>
    /// <typeparam name="ClassType">The class type.</typeparam>
    /// <typeparam name="DataType">The data type.</typeparam>
    public class ManyToMany<ClassType, DataType> : ManyClassPropertyBase<ClassType, DataType, ManyToMany<ClassType, DataType>>
        where ClassType : class
        where DataType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManyToMany{ClassType, DataType}"/> class.
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        public ManyToMany(Expression<Func<ClassType, IList<DataType>>> expression, IMapping mapping)
            : base(expression, mapping)
        {
        }

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        public override IManyToManyProperty Convert<TResult>(IMapping mapping)
        {
            var Result = new ExpressionTypeConverter<ClassType, IList<DataType>>
            {
                Expression = Expression
            }.Convert<TResult>();
            var ReturnObject = new ManyToMany<TResult, DataType>(Result, mapping);
            return ReturnObject;
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="dataModel">The data model.</param>
        /// <exception cref="ArgumentException"></exception>
        public override void Setup(MappingSource mappings, DataModel dataModel)
        {
            ForeignMapping = mappings.GetChildMappings<DataType>()
                                     .SelectMany(x => mappings.GetParentMapping(x.ObjectType))
                                     .FirstOrDefault(x => x.IDProperties.Any());
            if (ForeignMapping == null)
                throw new ArgumentException($"Foreign key IDs could not be found for {typeof(ClassType).Name}.{Name}");

            if (string.IsNullOrEmpty(TableName))
            {
                string Class1 = typeof(ClassType).Name;
                string Class2 = ForeignMapping.ObjectType.Name;
                if (string.Compare(Class1, Class2, StringComparison.Ordinal) < 0)
                    SetTableName(Class1 + "_" + Class2);
                else
                    SetTableName(Class2 + "_" + Class1);
            }

            var JoinTable = dataModel.SourceSpec.AddTable(TableName, ParentMapping.SchemaName);
            JoinTable.AddColumn<long>("ID_", DbType.UInt64, 0, false, true, false, true, false);
            var ParentMappings = mappings.GetParentMapping(ParentMapping.ObjectType);
            var ParentIDMappings = ParentMappings.SelectMany(x => x.IDProperties);
            DatabaseJoinsCascade = !ParentMappings.Contains(ForeignMapping);
            foreach (var ParentIDMapping in ParentIDMappings)
            {
                JoinTable.AddColumn(ParentMapping.TableName + ParentIDMapping.ColumnName,
                                ParentIDMapping.PropertyType.To(DbType.Int32),
                                ParentIDMapping.MaxLength,
                                false,
                                false,
                                false,
                                false,
                                false,
                                ParentMapping.TableName,
                                ParentIDMapping.ColumnName,
                                "",
                                "",
                                DatabaseJoinsCascade,
                                DatabaseJoinsCascade,
                                !DatabaseJoinsCascade);
            }
            foreach (var ForeignIDMapping in ForeignMapping.IDProperties)
            {
                JoinTable.AddColumn(ForeignMapping.TableName + ForeignIDMapping.ColumnName,
                                ForeignIDMapping.PropertyType.To(DbType.Int32),
                                ForeignIDMapping.MaxLength,
                                false,
                                false,
                                false,
                                false,
                                false,
                                ForeignMapping.TableName,
                                ForeignIDMapping.ColumnName,
                                "",
                                "",
                                DatabaseJoinsCascade,
                                DatabaseJoinsCascade,
                                !DatabaseJoinsCascade);
            }
        }
    }
}