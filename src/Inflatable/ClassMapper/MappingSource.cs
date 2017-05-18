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
using Inflatable.QueryProvider.Enums;
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
        /// <param name="queryProvider">The query provider.</param>
        /// <param name="logger">Logging object</param>
        /// <exception cref="ArgumentNullException">queryProvider or source</exception>
        public MappingSource(IEnumerable<IMapping> mappings, IDatabase source, QueryProviderManager queryProvider, ILogger logger)
        {
            QueryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
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
            SetupChildTypes();
            MergeMappings();
            SetupParentTypes();
            ReduceMappings();
            RemoveDeadMappings();
            SetupAutoIDs();
            SetupQueries();
        }

        /// <summary>
        /// Gets a value indicating whether this instance can be read.
        /// </summary>
        /// <value><c>true</c> if this instance can be read; otherwise, <c>false</c>.</value>
        public bool CanRead => Source?.SourceOptions?.Access.HasFlag(SourceAccess.Read) ?? false;

        /// <summary>
        /// Gets a value indicating whether this instance can be written to.
        /// </summary>
        /// <value><c>true</c> if this instance can be written to; otherwise, <c>false</c>.</value>
        public bool CanWrite => Source?.SourceOptions?.Access.HasFlag(SourceAccess.Write) ?? false;

        /// <summary>
        /// Gets the child types.
        /// </summary>
        /// <value>The child types.</value>
        public ListMapping<Type, Type> ChildTypes { get; private set; }

        /// <summary>
        /// Gets the concrete types.
        /// </summary>
        /// <value>The concrete types.</value>
        public IEnumerable<Type> ConcreteTypes { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to [generate schema].
        /// </summary>
        /// <value><c>true</c> if you should [generate schema]; otherwise, <c>false</c>.</value>
        public bool GenerateSchema => Source?.SourceOptions?.SchemaUpdate.HasFlag(SchemaGeneration.GenerateSchemaChanges) ?? false;

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
        /// Gets the query provider.
        /// </summary>
        /// <value>The query provider.</value>
        public QueryProviderManager QueryProvider { get; }

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
        /// Gets a value indicating whether to [update schema].
        /// </summary>
        /// <value><c>true</c> if you should [update schema]; otherwise, <c>false</c>.</value>
        public bool UpdateSchema => Source?.SourceOptions?.SchemaUpdate.HasFlag(SchemaGeneration.UpdateSchema) ?? false;

        /// <summary>
        /// Gets the child mappings.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns>The IMapping list associated with the object type.</returns>
        public IEnumerable<IMapping> GetChildMappings<TObject>()
        {
            var ObjectType = typeof(TObject);
            return GetChildMappings(ObjectType);
        }

        /// <summary>
        /// Gets the child mappings.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The IMapping list associated with the object type.</returns>
        public IEnumerable<IMapping> GetChildMappings(Type objectType)
        {
            return ChildTypes.ContainsKey(objectType) ? ChildTypes[objectType].ForEach(x => Mappings[x]) : new List<IMapping>();
        }

        /// <summary>
        /// Gets the parent mappings.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns>The IMapping list associated with the object type.</returns>
        public IEnumerable<IMapping> GetParentMapping<TObject>()
        {
            var ObjectType = typeof(TObject);
            return GetParentMapping(ObjectType);
        }

        /// <summary>
        /// Gets the parent mappings.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The IMapping list associated with the object type.</returns>
        public IEnumerable<IMapping> GetParentMapping(Type objectType)
        {
            return ParentTypes.ContainsKey(objectType) ? ParentTypes[objectType].ForEach(x => Mappings[x]) : new List<IMapping>();
        }

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
            if (!Source.SourceOptions.Optimize) return;
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
            if (!Source.SourceOptions.Optimize) return;
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
            if (!Source.SourceOptions.Optimize) return;
            var NeededTypes = new List<Type>();
            foreach (var Mapping in Mappings.Keys)
            {
                NeededTypes.AddIfUnique(ChildTypes[Mapping]);
                NeededTypes.AddIfUnique(ParentTypes[Mapping]);
            }
            var Items = Mappings.Keys.Where(x => !NeededTypes.Contains(x));
            foreach (var Item in Items)
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
            if (!Source.SourceOptions.Optimize) return;
            foreach (var CurrentTree in TypeGraphs.Values)
            {
                var CurrentMapping = Mappings[CurrentTree.Root.Data];
                if (CurrentMapping.IDProperties.Count > 0)
                    continue;
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
                var Parents = TypeGraphs[ConcreteType].ToList();
                foreach (var Parent in Parents)
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
                var Parents = TypeGraphs[ConcreteType].ToList();
                foreach (var Parent in Parents)
                {
                    ParentTypes.Add(ConcreteType, Parent);
                }
            }
        }

        /// <summary>
        /// Sets up the default queries.
        /// </summary>
        private void SetupQueries()
        {
            Logger.Information("Setting up default queries.");
            var QueryTypes = new QueryType[]
            {
                QueryType.All,
                QueryType.Any,
                QueryType.Delete,
                QueryType.Insert,
                QueryType.Update
            };
            foreach (var ConcreteType in ConcreteTypes)
            {
                var Generator = QueryProvider.CreateGenerator(ConcreteType, this);
                var Queries = Generator.GenerateDefaultQueries();
                foreach (var TempQueryType in QueryTypes)
                {
                    if (!Mappings[ConcreteType].Queries.ContainsKey(TempQueryType) && !string.IsNullOrEmpty(Queries[TempQueryType].QueryString))
                    {
                        Mappings[ConcreteType].SetQuery(TempQueryType, Queries[TempQueryType].QueryString, Queries[TempQueryType].DatabaseCommandType);

                        Logger.Debug("Adding default query of type {Type:l} for {Name:l} in source {Source:l}",
                            Enum.GetName(typeof(QueryType), TempQueryType),
                            ConcreteType.GetName(),
                            Source.Name);
                    }
                    else if (string.IsNullOrEmpty(Queries[TempQueryType].QueryString))
                    {
                        Logger.Debug("Could not generate default query of type {Type:l} for {Name:l} in source {Source:l}",
                            Enum.GetName(typeof(QueryType), TempQueryType),
                            ConcreteType.GetName(),
                            Source.Name);
                    }
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