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
using Inflatable.ClassMapper.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflatable.Aspect.InterfaceImplementation
{
    /// <summary>
    /// Sets up many to many fields.
    /// </summary>
    /// <seealso cref="IInterfaceImplementationHelper"/>
    public class SetupManyToManyFields : IInterfaceImplementationHelper
    {
        /// <summary>
        /// Setups the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="aspect">The aspect.</param>
        /// <returns>The resulting code in string format.</returns>
        public string Setup(Type type, ORMAspect aspect)
        {
            aspect.ManyToManyFields = new List<IManyToManyProperty>();
            aspect.ManyToOneFields = new List<IManyToOneProperty>();
            var Builder = new StringBuilder();
            foreach (var Source in aspect.ClassManager.Sources.Where(x => x.ConcreteTypes.Contains(type)))
            {
                var Mapping = Source.Mappings[type];
                foreach (var ParentType in Source.ParentTypes[type])
                {
                    Mapping = Source.Mappings[ParentType];
                    foreach (var Property in Mapping.ManyToManyProperties
                                                          .Where(x => !aspect.ManyToManyFields.Any(y => y.Name == x.Name)))
                    {
                        aspect.ManyToManyFields.Add(Property);
                        Builder.AppendLineFormat("private {0} {1};", "IList<" + Property.TypeName + ">", Property.InternalFieldName);
                        Builder.AppendLineFormat("private bool {0};", Property.InternalFieldName + "Loaded");
                    }
                    foreach (var Property in Mapping.ManyToOneProperties
                                                          .Where(x => !aspect.ManyToOneFields.Any(y => y.Name == x.Name)))
                    {
                        aspect.ManyToOneFields.Add(Property);
                        if (Property is IManyToOneListProperty)
                        {
                            Builder.AppendLineFormat("private {0} {1};", "IList<" + Property.TypeName + ">", Property.InternalFieldName);
                        }
                        else
                        {
                            Builder.AppendLineFormat("private {0} {1};", Property.TypeName, Property.InternalFieldName);
                        }

                        Builder.AppendLineFormat("private bool {0};", Property.InternalFieldName + "Loaded");
                    }
                }
            }
            return Builder.ToString();
        }
    }
}