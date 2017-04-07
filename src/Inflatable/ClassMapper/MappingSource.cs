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
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflatable.ClassMapper
{
    /// <summary>
    /// Mapping source
    /// </summary>
    public class MappingSource
    {
        /// <summary>
        /// Mapping source
        /// </summary>
        /// <param name="mappings">Mappings associated with the source</param>
        /// <param name="source">Database source</param>
        /// <param name="logger">Logging object</param>
        public MappingSource(IEnumerable<IMapping> mappings, IDatabase source, ILogger logger)
        {
            Logger = logger;
            mappings = mappings ?? new ConcurrentBag<IMapping>();
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Logger.Information("Setting up {Name:l}", source.Name);
            Order = Source.Order;
            Mappings = new ConcurrentDictionary<Type, IMapping>();
            TypeGraphs = new ConcurrentDictionary<Type, Tree<Type>>();
            ChildTypes = new ListMapping<Type, Type>();
            ParentTypes = new ListMapping<Type, Type>();
            AddMappings(mappings);
            SetupTypeGraphs();
            IEnumerable<Type> ConcreteTypes = SetupChildTypes();
            MergeMappings();
            SetupParentTypes(ConcreteTypes);
            ReduceMappings();
            RemoveDeadMappings();
            SetupAutoIDs();
        }

        /// <summary>
        /// Gets the child types.
        /// </summary>
        /// <value>The child types.</value>
        public ListMapping<Type, Type> ChildTypes { get; private set; }

        /// <summary>
        /// Logger for the system
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        /// <value>The mappings.</value>
        public IDictionary<Type, IMapping> Mappings { get; private set; }

        /// <summary>
        /// Order that the source is used
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the parent types.
        /// </summary>
        /// <value>The parent types.</value>
        public ListMapping<Type, Type> ParentTypes { get; private set; }

        /// <summary>
        /// Source info
        /// </summary>
        public IDatabase Source { get; private set; }

        /// <summary>
        /// Gets or sets the type graph.
        /// </summary>
        /// <value>The type graph.</value>
        public IDictionary<Type, Tree<Type>> TypeGraphs { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            StringBuilder Builder = new StringBuilder();
            Builder.AppendLineFormat("Source: {0}", Source.Name);
            foreach (var Mapping in Mappings.Values)
            {
                Builder.AppendLineFormat("\tMapping: {0}", Mapping);
                Builder.AppendLine("\t\tIDs:");
                foreach (var Property in Mapping.IDProperties)
                {
                    Builder.AppendLineFormat("\t\t\t{0}", Property);
                }
                Builder.AppendLine("\t\tAuto IDs:");
                foreach (var Property in Mapping.AutoIDProperties)
                {
                    Builder.AppendLineFormat("\t\t\t{0}", Property);
                }
                Builder.AppendLine("\t\tReferences:");
                foreach (var Property in Mapping.ReferenceProperties)
                {
                    Builder.AppendLineFormat("\t\t\t{0}", Property);
                }
                Builder.AppendLine();
            }
            return Builder.ToString();
        }

        /// <summary>
        /// Adds the mappings.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        private void AddMappings(IEnumerable<IMapping> mappings)
        {
            Logger.Information("Adding mappings for {Name:l}", Source.Name);
            foreach (var Mapping in mappings)
            {
                Mappings.Add(Mapping.ObjectType, Mapping);
            }
        }

        /// <summary>
        /// Merges the mappings.
        /// </summary>
        private void MergeMappings()
        {
            if (!Source.Optimize) return;
            Logger.Information("Merging mappings for {Name:l}", Source.Name);
            var MappingMerger = new MergeMappings(Mappings, Logger);
            foreach (var TempTypeGraph in TypeGraphs.Values)
            {
                MappingMerger.Merge(TempTypeGraph);
            }
        }

        /// <summary>
        /// Reduces the mappings.
        /// </summary>
        private void ReduceMappings()
        {
            if (!Source.Optimize) return;
            Logger.Information("Reducing mappings for {Name:l}", Source.Name);
            var ReduceMapping = new ReduceMappings(Mappings, Logger);
            foreach (var TempTypeGraph in TypeGraphs.Values)
            {
                ReduceMapping.Reduce(TempTypeGraph);
            }
        }

        private void RemoveDeadMappings()
        {
            if (!Source.Optimize) return;
            var NeededTypes = new List<Type>();
            foreach (var Mapping in Mappings.Keys)
            {
                NeededTypes.AddIfUnique(ChildTypes[Mapping]);
                NeededTypes.AddIfUnique(ParentTypes[Mapping]);
            }
            var Items = Mappings.Keys.Where(x => !NeededTypes.Contains(x));
            foreach (var Item in Items)
            {
                Logger.Debug("Removing mapping {Name:l} from manager as mapping has been merged.", Source.Name);
                Mappings.Remove(Item);
                TypeGraphs.Remove(Item);
            }
        }

        /// <summary>
        /// Sets up the parent IDs.
        /// </summary>
        private void SetupAutoIDs()
        {
            if (!Source.Optimize) return;
            foreach (var CurrentTree in TypeGraphs.Values)
            {
                var CurrentMapping = Mappings[CurrentTree.Root.Data];
                if (CurrentMapping.IDProperties.Count > 0)
                    continue;
                Logger.Debug("Adding identity key to {Name:l} as one is not defined.", CurrentMapping);
                CurrentMapping.AddAutoKey();
            }
        }

        /// <summary>
        /// Sets up the child types.
        /// </summary>
        /// <returns>The concrete types found</returns>
        private IEnumerable<Type> SetupChildTypes()
        {
            Logger.Information("Setting up child type discovery for {Name:l}", Source.Name);
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

            return ConcreteTypes;
        }

        /// <summary>
        /// Sets up the parent types.
        /// </summary>
        /// <param name="ConcreteTypes">The concrete types.</param>
        private void SetupParentTypes(IEnumerable<Type> ConcreteTypes)
        {
            Logger.Information("Setting up parent type discovery for {Name:l}", Source.Name);
            foreach (var ConcreteType in ConcreteTypes)
            {
                var Parents = TypeGraphs[ConcreteType].ToList();
                foreach (var Parent in Parents)
                {
                    ParentTypes.Add(ConcreteType, Parent);
                }
            }
        }

        /// <summary>
        /// Sets up the type graphs.
        /// </summary>
        private void SetupTypeGraphs()
        {
            Logger.Information("Setting up type graphs for {Name:l}", Source.Name);
            var TempGenerator = new Generator(Mappings);
            foreach (var Key in Mappings.Keys)
            {
                TypeGraphs.Add(Key, TempGenerator.Generate(Key));
            }
        }
    }
}