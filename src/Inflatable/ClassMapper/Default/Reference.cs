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
using System;
using System.Linq.Expressions;

namespace Inflatable.ClassMapper.Default
{
    /// <summary>
    /// Reference property
    /// </summary>
    /// <typeparam name="ClassType">The type of the lass type.</typeparam>
    /// <typeparam name="DataType">The type of the ata type.</typeparam>
    /// <seealso cref="IProperty"/>
    public class Reference<ClassType, DataType> : PropertyBase<ClassType, DataType, Reference<ClassType, DataType>>, IProperty
        where ClassType : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Reference{ClassType, DataType}"/> class.
        /// </summary>
        /// <param name="expression">Expression used to point to the property</param>
        /// <param name="mapping">Mapping the StringID is added to</param>
        public Reference(Expression<Func<ClassType, DataType>> expression, IMapping mapping) : base(expression, mapping)
        {
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
        /// Gets the property as a parameter (for classes, this will return the ID of the property)
        /// </summary>
        /// <param name="Object">Object to get the parameter from</param>
        /// <returns>The parameter version of the property</returns>
        public override object GetParameter(object Object)
        {
            return GetValue(Object);
        }

        /// <summary>
        /// Sets up the property (used internally)
        /// </summary>
        public override void Setup()
        {
        }
    }
}