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
using Inflatable.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            TypeGraph = new Graph<IMapping>();
            SetupTypeGraph();
        }

        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        /// <value>The mappings.</value>
        public IDictionary<Type, IMapping> Mappings { get; set; }

        /// <summary>
        /// Gets or sets the type graph.
        /// </summary>
        /// <value>The type graph.</value>
        public Graph<IMapping> TypeGraph { get; set; }

        private Vertex<IMapping> GetCurrentNode(Type CurrentType, Vertex<IMapping> PreviousVertex)
        {
            if (!Mappings.ContainsKey(CurrentType))
                return PreviousVertex;
            Vertex<IMapping> CurrentVertex = TypeGraph.FirstOrDefault(x => x.Data == Mappings[CurrentType]);
            if (CurrentVertex == null)
                CurrentVertex = TypeGraph.AddVertex(Mappings[CurrentType]);
            if (PreviousVertex != null)
                TypeGraph.AddEdge(PreviousVertex, CurrentVertex);
            return CurrentVertex;
        }

        private void SetupInterfaceNodes(Vertex<IMapping> CurrentVertex, Type CurrentType)
        {
            var CurrentInterfaces = CurrentType.GetInterfaces();
            foreach (var Interface in CurrentInterfaces)
            {
                var TempInterfaceVertex = TypeGraph.FirstOrDefault(x => x.Data == Mappings[Interface]);
                if (TempInterfaceVertex == null && Mappings.Keys.Contains(Interface))
                {
                    TempInterfaceVertex = TypeGraph.AddVertex(Mappings[Interface]);
                    TypeGraph.AddEdge(CurrentVertex, TempInterfaceVertex);
                }
            }
        }

        private void SetupTypeGraph()
        {
            foreach (var Key in Mappings.Keys)
            {
                var CurrentVertex = TypeGraph.FirstOrDefault(x => x.Data == Mappings[Key]);
                if (CurrentVertex == null)
                {
                    var CurrentType = Key;
                    while (CurrentType != null)
                    {
                        var PreviousVertex = CurrentVertex;
                        CurrentVertex = GetCurrentNode(CurrentType, PreviousVertex);
                        SetupInterfaceNodes(CurrentVertex, CurrentType);
                        CurrentType = CurrentType.GetTypeInfo().BaseType;
                    }
                }
            }
        }
    }
}