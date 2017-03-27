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
using Inflatable.ClassMapper.TypeGraph;
using Inflatable.Interfaces;
using Inflatable.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Inflatable.ClassMapper
{
    /// <summary>
    /// Mapping manager
    /// </summary>
    public class MappingManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MappingManager"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        public MappingManager(IEnumerable<IMapping> mappings)
        {
            mappings = mappings ?? new List<IMapping>();
            Mappings = new ConcurrentDictionary<Type, IMapping>();
            foreach (var Mapping in mappings)
            {
                Mappings.Add(Mapping.ObjectType, Mapping);
            }
            var TempGenerator = new Generator(Mappings);
            TypeGraphs = new ConcurrentDictionary<Type, Tree<Type>>();
            foreach (var Key in Mappings.Keys)
            {
                TypeGraphs.Add(Key, TempGenerator.Generate(Key));
            }
            ChildTypes = new ListMapping<Type, Type>();
            ParentTypes = new ListMapping<Type, Type>();
            var TempConcreteDiscoverer = new DiscoverConcreteTypes(TypeGraphs);
            var ConcreteTypes = TempConcreteDiscoverer.FindConcreteTypes();
            foreach (var ConcreteType in ConcreteTypes)
            {
                var Parents = TypeGraphs[ConcreteType].ToList();
                foreach (var Parent in Parents)
                {
                    ChildTypes.Add(Parent, ConcreteType);
                }
            }

            var MappingMerger = new MergeMappings(Mappings);
            foreach (var TempTypeGraph in TypeGraphs.Values)
            {
                MappingMerger.Merge(TempTypeGraph);
            }

            foreach (var ConcreteType in ConcreteTypes)
            {
                var Parents = TypeGraphs[ConcreteType].ToList();
                foreach (var Parent in Parents)
                {
                    ParentTypes.Add(ConcreteType, Parent);
                }
            }

            var ReduceMapping = new ReduceMappings(Mappings);
            foreach (var TempTypeGraph in TypeGraphs.Values)
            {
                ReduceMapping.Reduce(TempTypeGraph);
            }
        }

        /// <summary>
        /// Gets the child types.
        /// </summary>
        /// <value>The child types.</value>
        public ListMapping<Type, Type> ChildTypes { get; private set; }

        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        /// <value>The mappings.</value>
        public IDictionary<Type, IMapping> Mappings { get; private set; }

        /// <summary>
        /// Gets the parent types.
        /// </summary>
        /// <value>The parent types.</value>
        public ListMapping<Type, Type> ParentTypes { get; private set; }

        /// <summary>
        /// Gets or sets the type graph.
        /// </summary>
        /// <value>The type graph.</value>
        public IDictionary<Type, Tree<Type>> TypeGraphs { get; private set; }
    }
}