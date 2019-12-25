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
using Inflatable.Enums;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.Utils;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        /// <param name="queryProvider">The query provider.</param>
        /// <param name="logger">Logging object</param>
        /// <exception cref="ArgumentNullException">queryProvider or source</exception>
        public MappingSource(IEnumerable<IMapping> mappings, IDatabase source, QueryProviderManager queryProvider, ILogger logger)
        {
            QueryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
            Logger = logger ?? Log.Logger ?? new LoggerConfiguration().CreateLogger() ?? throw new ArgumentNullException(nameof(logger));
            ConcreteTypes = Array.Empty<Type>();
            mappings ??= new ConcurrentBag<IMapping>();
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Logger.Information("Setting up {Name:l}", source.Name);
            Order = Source.Order;
            Mappings = new ConcurrentDictionary<Type, IMapping>();
            TypeGraphs = new ConcurrentDictionary<Type, Tree<Type>?>();
            ChildTypes = new ListMapping<Type, Type>();
            ParentTypes = new ListMapping<Type, Type>();
            AddMappings(mappings);
            SetupTypeGraphs();
            SetupChildTypes();
            MergeMappings();
            SetupParentTypes();
            ReduceMappings();
            RemoveDeadMappings();
            SetupAutoIDs();
        }

        /// <summary>
        /// Gets a value indicating whether to [apply analysis].
        /// </summary>
        /// <value><c>true</c> if you should [apply analysis]; otherwise, <c>false</c>.</value>
        public bool ApplyAnalysis => Source?.SourceOptions?.Analysis == SchemaAnalysis.ApplyAnalysis;

        /// <summary>
        /// Gets a value indicating whether this instance can be read.
        /// </summary>
        /// <value><c>true</c> if this instance can be read; otherwise, <c>false</c>.</value>
        public bool CanRead => (Source?.SourceOptions?.Access & SourceAccess.Read) != 0;

        /// <summary>
        /// Gets a value indicating whether this instance can be written to.
        /// </summary>
        /// <value><c>true</c> if this instance can be written to; otherwise, <c>false</c>.</value>
        public bool CanWrite => (Source?.SourceOptions?.Access & SourceAccess.Write) != 0;

        /// <summary>
        /// Gets the child types.
        /// </summary>
        /// <value>The child types.</value>
        public ListMapping<Type, Type> ChildTypes { get; }

        /// <summary>
        /// Gets the concrete types.
        /// </summary>
        /// <value>The concrete types.</value>
        public IEnumerable<Type> ConcreteTypes { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to [generate analysis].
        /// </summary>
        /// <value><c>true</c> if you should [generate analysis]; otherwise, <c>false</c>.</value>
        public bool GenerateAnalysis => Source?.SourceOptions?.Analysis == SchemaAnalysis.GenerateAnalysis;

        /// <summary>
        /// Gets a value indicating whether to [generate schema].
        /// </summary>
        /// <value><c>true</c> if you should [generate schema]; otherwise, <c>false</c>.</value>
        public bool GenerateSchema => Source?.SourceOptions?.SchemaUpdate == SchemaGeneration.GenerateSchemaChanges;

        /// <summary>
        /// Logger for the system
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        /// <value>The mappings.</value>
        public IDictionary<Type, IMapping> Mappings { get; }

        /// <summary>
        /// Order that the source is used
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Gets the parent types.
        /// </summary>
        /// <value>The parent types.</value>
        public ListMapping<Type, Type> ParentTypes { get; }

        /// <summary>
        /// Gets the query provider.
        /// </summary>
        /// <value>The query provider.</value>
        public QueryProviderManager QueryProvider { get; }

        /// <summary>
        /// Source info
        /// </summary>
        public IDatabase Source { get; }

        /// <summary>
        /// Gets or sets the type graph.
        /// </summary>
        /// <value>The type graph.</value>
        public IDictionary<Type, Tree<Type>?> TypeGraphs { get; }

        /// <summary>
        /// Gets a value indicating whether to [update schema].
        /// </summary>
        /// <value><c>true</c> if you should [update schema]; otherwise, <c>false</c>.</value>
        public bool UpdateSchema => Source?.SourceOptions?.SchemaUpdate == SchemaGeneration.UpdateSchema;

        /// <summary>
        /// Determines whether the specified <see cref="object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => ReferenceEquals(obj, this);

        /// <summary>
        /// Gets the child mappings.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns>The IMapping list associated with the object type.</returns>
        public IEnumerable<IMapping> GetChildMappings<TObject>()
        {
            return GetChildMappings(typeof(TObject));
        }

        /// <summary>
        /// Gets the child mappings.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The IMapping list associated with the object type.</returns>
        public IEnumerable<IMapping> GetChildMappings(Type objectType)
        {
            if (objectType.Namespace.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal))
            {
                objectType = objectType.GetTypeInfo().BaseType;
            }

            return ChildTypes.ContainsKey(objectType) ? ChildTypes[objectType].ForEach(x => Mappings[x]) : new List<IMapping>();
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.
        /// </returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Gets the parent mappings.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns>The IMapping list associated with the object type.</returns>
        public IEnumerable<IMapping> GetParentMapping<TObject>()
        {
            return GetParentMapping(typeof(TObject));
        }

        /// <summary>
        /// Gets the parent mappings.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The IMapping list associated with the object type.</returns>
        public IEnumerable<IMapping> GetParentMapping(Type objectType)
        {
            if (objectType.Namespace.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal))
            {
                objectType = objectType.GetTypeInfo().BaseType;
            }

            return ParentTypes.ContainsKey(objectType) ? ParentTypes[objectType].ForEach(x => Mappings[x]) : new List<IMapping>();
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString()
        {
            var Builder = new StringBuilder();
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
                Builder.AppendLine("\t\tMap:");
                foreach (var Property in Mapping.MapProperties)
                {
                    Builder.AppendLineFormat("\t\t\t{0}", Property);
                }
                Builder.AppendLine("\t\tMany To Many:");
                foreach (var Property in Mapping.ManyToManyProperties)
                {
                    Builder.AppendLineFormat("\t\t\t{0}", Property);
                }
                Builder.AppendLine("\t\tMany To One:");
                foreach (var Property in Mapping.ManyToOneProperties)
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
            if (!Source.SourceOptions.Optimize)
            {
                return;
            }

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
            if (!Source.SourceOptions.Optimize)
            {
                return;
            }

            Logger.Information("Reducing mappings for {Name:l}", Source.Name);
            var ReduceMapping = new ReduceMappings(Mappings, Logger);
            foreach (var TempTypeGraph in TypeGraphs.Values)
            {
                ReduceMapping.Reduce(TempTypeGraph);
            }
        }

        /// <summary>
        /// Removes the dead mappings.
        /// </summary>
        private void RemoveDeadMappings()
        {
            if (!Source.SourceOptions.Optimize)
            {
                return;
            }

            var NeededTypes = new List<Type>();
            foreach (var Mapping in Mappings.Keys)
            {
                NeededTypes.AddIfUnique(ChildTypes[Mapping]);
                NeededTypes.AddIfUnique(ParentTypes[Mapping]);
            }
            foreach (var Item in Mappings.Keys.Where(x => !NeededTypes.Contains(x)))
            {
                Logger.Debug("Removing mapping {MappingName:l} from {SourceName:l} as mapping has been merged.", Item.Name, Source.Name);
                Mappings.Remove(Item);
                TypeGraphs.Remove(Item);
            }
        }

        /// <summary>
        /// Sets up the parent IDs.
        /// </summary>
        private void SetupAutoIDs()
        {
            if (!Source.SourceOptions.Optimize)
            {
                return;
            }

            foreach (var CurrentTree in TypeGraphs.Values)
            {
                if (CurrentTree is null)
                    continue;
                var CurrentMapping = Mappings[CurrentTree.Root.Data];
                if (CurrentMapping.IDProperties.Count > 0)
                {
                    continue;
                }

                Logger.Debug("Adding identity key to {Name:l} in {Source:l} as one is not defined.", CurrentMapping, Source.Name);
                CurrentMapping.AddAutoKey();
            }
        }

        /// <summary>
        /// Sets up the child types.
        /// </summary>
        /// <returns>The concrete types found</returns>
        private void SetupChildTypes()
        {
            Logger.Information("Setting up child type discovery for {Name:l}", Source.Name);
            var TempConcreteDiscoverer = new DiscoverConcreteTypes(TypeGraphs);
            ConcreteTypes = TempConcreteDiscoverer.FindConcreteTypes();
            foreach (var ConcreteType in ConcreteTypes)
            {
                if (ConcreteType is null)
                    continue;
                var Types = TypeGraphs[ConcreteType];
                if (Types is null)
                    continue;
                foreach (var Parent in Types.ToList())
                {
                    ChildTypes.Add(Parent, ConcreteType);
                }
            }
        }

        /// <summary>
        /// Sets up the parent types.
        /// </summary>
        private void SetupParentTypes()
        {
            Logger.Information("Setting up parent type discovery for {Name:l}", Source.Name);
            foreach (var ConcreteType in ConcreteTypes)
            {
                var Types = TypeGraphs[ConcreteType];
                if (Types is null)
                    continue;
                foreach (var Parent in Types.ToList())
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