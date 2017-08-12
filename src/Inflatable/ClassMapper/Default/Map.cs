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
using Inflatable.Utils;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.Default
{
    /// <summary>
    /// Map property
    /// </summary>
    /// <typeparam name="ClassType">The type of the class type.</typeparam>
    /// <typeparam name="DataType">The type of the data type.</typeparam>
    /// <seealso cref="IMapProperty"/>
    public class Map<ClassType, DataType> : SingleClassPropertyBase<ClassType, DataType, Map<ClassType, DataType>>
        where ClassType : class
        where DataType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Map{ClassType, DataType}"/> class.
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        public Map(Expression<Func<ClassType, DataType>> expression, IMapping mapping) : base(expression, mapping)
        {
            if (typeof(DataType).Is(typeof(IEnumerable)))
                throw new ArgumentException("Expression is an IEnumerable, use ManyToOne or ManyToMany instead");
        }

        /// <summary>
        /// Converts this instance to the class specified
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>The resulting property</returns>
        public override IMapProperty Convert<TResult>(IMapping mapping)
        {
            var Result = new ExpressionTypeConverter<ClassType, DataType>
            {
                Expression = Expression
            }.Convert<TResult>();
            var ReturnObject = new Map<TResult, DataType>(Result, mapping);
            return ReturnObject;
        }

        /// <summary>
        /// Gets the property as a parameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="Object">Object to get the parameter from</param>
        /// <returns>The parameter version of the property</returns>
        public override object GetParameter(object Object)
        {
            return GetValue(Object);
        }

        /// <summary>
        /// Gets the property as a parameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="Object">Object to get the parameter from</param>
        /// <returns>The parameter version of the property</returns>
        public override object GetParameter(Dynamo Object)
        {
            return GetValue(Object);
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        /// <param name="mappings"></param>
        public override void Setup(MappingSource mappings)
        {
            ForeignMapping = mappings.GetChildMappings<DataType>()
                                     .SelectMany(x => mappings.GetParentMapping(x.ObjectType))
                                     .FirstOrDefault(x => x.IDProperties.Any());
            if (ForeignMapping == null)
                throw new ArgumentException($"Foreign key IDs could not be found for {typeof(ClassType).Name}.{Name}");
        }
    }
}