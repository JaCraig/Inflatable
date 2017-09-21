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

using Inflatable.Interfaces;
using Inflatable.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Inflatable.ClassMapper.TypeGraph
{
    /// <summary>
    /// Type graph generator
    /// </summary>
    public class Generator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Generator"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        public Generator(IDictionary<Type, IMapping> mappings)
        {
            Mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));
        }

        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        /// <value>The mappings.</value>
        private IDictionary<Type, IMapping> Mappings { get; set; }

        /// <summary>
        /// Generates the specified mapping type.
        /// </summary>
        /// <param name="mappingType">Type of the mapping.</param>
        /// <returns>The type graph associated with the type.</returns>
        public Tree<Type> Generate(Type mappingType)
        {
            var TempTypeGraph = new Tree<Type>(mappingType);
            mappingType = mappingType.GetTypeInfo().BaseType;
            var CurrentNode = TempTypeGraph.Root;
            while (mappingType != null)
            {
                if (Mappings.Keys.Contains(mappingType))
                {
                    CurrentNode = CurrentNode.AddNode(mappingType);
                }
                mappingType = mappingType.GetTypeInfo().BaseType;
            }
            while (CurrentNode != null)
            {
                int MaxLength = CurrentNode.Data.GetInterfaces().Length;
                var CurrentInterfaces = CurrentNode.Data.GetInterfaces();
                for (int i = 0; i < MaxLength; i++)
                {
                    var Interface = CurrentInterfaces[i];
                    if (!TempTypeGraph.ContainsNode(Interface, (x, y) => x == y) && Mappings.Keys.Contains(Interface))
                    {
                        CurrentNode.AddNode(Interface);
                    }
                }
                CurrentNode = CurrentNode.Parent;
            }
            return TempTypeGraph;
        }
    }
}