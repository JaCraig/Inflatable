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
using Inflatable.Aspect.Interfaces;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Linq;
using System.Text;

namespace Inflatable.Aspect.InterfaceImplementation
{
    /// <summary>
    /// Sets up the reference fields
    /// </summary>
    /// <seealso cref="IInterfaceImplementationHelper"/>
    public class SetupReferenceFields : IInterfaceImplementationHelper
    {
        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; }

        /// <summary>
        /// Setups the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="aspect">The aspect.</param>
        /// <param name="objectPool">The object pool.</param>
        /// <returns>The resulting code in string format.</returns>
        public string Setup(Type type, ORMAspect aspect, ObjectPool<StringBuilder> objectPool)
        {
            if (aspect is null || objectPool is null)
                return string.Empty;
            aspect.ReferenceFields.Clear();
            StringBuilder Builder = objectPool.Get();
            foreach (ClassMapper.IMappingSource? Source in aspect.ClassManager.Sources.Where(x => x.ConcreteTypes.Contains(type)))
            {
                Inflatable.Interfaces.IMapping Mapping = Source.Mappings[type];
                foreach (Type ParentType in Source.ParentTypes[type])
                {
                    Mapping = Source.Mappings[ParentType];
                    foreach (ClassMapper.Interfaces.IProperty Property in Mapping.ReferenceProperties)
                    {
                        if (aspect.ReferenceFields.Any(y => y.Name == Property.Name))
                            continue;
                        aspect.ReferenceFields.Add(Property);
                        _ = Builder.Append("private ").Append(Property.TypeName).Append(" ").Append(Property.InternalFieldName).AppendLine(";")
                            .Append("private bool ").Append(Property.InternalFieldName).AppendLine("Loaded;");
                    }
                }
            }
            var Result = Builder.ToString();
            objectPool.Return(Builder);
            return Result;
        }
    }
}