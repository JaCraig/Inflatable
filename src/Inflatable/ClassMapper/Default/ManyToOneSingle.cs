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
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.Default
{
    /// <summary>
    /// Many to one single side
    /// </summary>
    /// <typeparam name="ClassType">The type of the lass type.</typeparam>
    /// <typeparam name="DataType">The type of the ata type.</typeparam>
    public class ManyToOneSingle<ClassType, DataType> : ManyToOneOnePropertyBase<ClassType, DataType, ManyToOneSingle<ClassType, DataType>>
        where ClassType : class
        where DataType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManyToOneSingle{ClassType, DataType}"/> class.
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        public ManyToOneSingle(Expression<Func<ClassType, DataType>> expression, IMapping mapping)
            : base(expression, mapping)
        {
        }

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        public override IManyToOneProperty Convert<TResult>(IMapping mapping)
        {
            var Result = new ExpressionTypeConverter<ClassType, DataType>
            {
                Expression = Expression
            }.Convert<TResult>();
            var ReturnObject = new ManyToOneSingle<TResult, DataType>(Result, mapping);
            return ReturnObject;
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="dataModel">The data model.</param>
        /// <exception cref="System.ArgumentException"></exception>
        public override void Setup(MappingSource mappings, DataModel dataModel)
        {
            ForeignMapping = mappings.GetChildMappings<DataType>()
                                     .SelectMany(x => mappings.GetParentMapping(x.ObjectType))
                                     .FirstOrDefault(x => x.IDProperties.Any());
            if (ForeignMapping == null)
                throw new ArgumentException($"Foreign key IDs could not be found for {typeof(ClassType).Name}.{Name}");
            var ParentTable = dataModel.SourceSpec.Tables.FirstOrDefault(x => x.Name == ParentMapping.TableName);
            foreach (var IDMapping in ForeignMapping.IDProperties)
            {
                if (ParentTable.Columns.Any(x => x.Name == ColumnName + ForeignMapping.TableName + IDMapping.ColumnName))
                    continue;
                ParentTable.AddColumn<object>(ColumnName + ForeignMapping.TableName + IDMapping.ColumnName,
                                IDMapping.PropertyType.To(DbType.Int32),
                                IDMapping.MaxLength,
                                true,
                                false,
                                false,
                                false,
                                false,
                                ForeignMapping.TableName,
                                IDMapping.ColumnName,
                                null,
                                "",
                                false,
                                false,
                                true);
            }
        }
    }
}