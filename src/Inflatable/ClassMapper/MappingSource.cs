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
using Microsoft.Extensions.ObjectPool;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflatable.ClassMapper
{
    /// <summary>
    /// Mapping source
    /// </summary>
    /// <seealso cref="IMappingSource"/>
    public class MappingSource : IMappingSource
    {
        /// <summary>
        /// Mapping source
        /// </summary>
        /// <param name="mappings">Mappings associated with the source</param>
        /// <param name="source">Database source</param>
        /// <param name="queryProvider">The query provider.</param>
        /// <param name="logger">Logging object</param>
        /// <param name="objectPool">The object pool.</param>
        /// <exception cref="ArgumentNullException">queryProvider or source</exception>
        public MappingSource(
            IEnumerable<IMapping> mappings,
            IDatabase source,
            QueryProviderManager queryProvider,
            ILogger logger,
            ObjectPool<StringBuilder> objectPool)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Logger = logger ?? Log.Logger ?? new LoggerConfiguration().CreateLogger() ?? throw new ArgumentNullException(nameof(logger));
            Logger.Information("Setting up {Name:l}", source.Name);

            QueryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
            ConcreteTypes = Array.Empty<Type>();
            mappings ??= Array.Empty<IMapping>();

            IsDebug = Logger.IsEnabled(LogEventLevel.Debug);
            var TempSourceOptions = Source.SourceOptions;
            if (!(TempSourceOptions is null))
            {
                CanRead = (TempSourceOptions.Access & SourceAccess.Read) != 0;
                CanWrite = (TempSourceOptions.Access & SourceAccess.Write) != 0;
                ApplyAnalysis = TempSourceOptions.Analysis == SchemaAnalysis.ApplyAnalysis;
                GenerateAnalysis = TempSourceOptions.Analysis == SchemaAnalysis.GenerateAnalysis;
                GenerateSchema = TempSourceOptions.SchemaUpdate == SchemaGeneration.GenerateSchemaChanges;
                UpdateSchema = TempSourceOptions.SchemaUpdate == SchemaGeneration.UpdateSchema;
                Optimize = TempSourceOptions.Optimize;
            }

            Order = Source.Order;

            ChildTypes = new ListMapping<Type, Type>();
            ParentTypes = new ListMapping<Type, Type>();
            ObjectPool = objectPool;

            Logger.Information("Adding mappings for {Name:l}", Source.Name);
            Mappings = mappings.ToDictionary(x => x.ObjectType);
            Logger.Information("Setting up type graphs for {Name:l}", Source.Name);
            TypeGraphs = Mappings.ToDictionary(Item => Item.Key, Item => Generator.Generate(Item.Key, Mappings));

            SetupChildTypes();
            MergeMappings();
            SetupParentTypes();
            ReduceMappings();
            RemoveDeadMappings();
            SetupAutoIDs();
        }

        /// <summary>
        /// The is debug
        /// </summary>
        private readonly bool IsDebug;

        /// <summary>
        /// To string value
        /// </summary>
        private string? _ToString;

        /// <summary>
        /// Gets a value indicating whether to [apply analysis].
        /// </summary>
        /// <value><c>true</c> if you should [apply analysis]; otherwise, <c>false</c>.</value>
        public bool ApplyAnalysis { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can be read.
        /// </summary>
        /// <value><c>true</c> if this instance can be read; otherwise, <c>false</c>.</value>
        public bool CanRead { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can be written to.
        /// </summary>
        /// <value><c>true</c> if this instance can be written to; otherwise, <c>false</c>.</value>
        public bool CanWrite { get; }

        /// <summary>
        /// Gets the child types.
        /// </summary>
        /// <value>The child types.</value>
        public ListMapping<Type, Type> ChildTypes { get; }

        /// <summary>
        /// Gets the concrete types.
        /// </summary>
        /// <value>The concrete types.</value>
        public Type[] ConcreteTypes { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to [generate analysis].
        /// </summary>
        /// <value><c>true</c> if you should [generate analysis]; otherwise, <c>false</c>.</value>
        public bool GenerateAnalysis { get; }

        /// <summary>
        /// Gets a value indicating whether to [generate schema].
        /// </summary>
        /// <value><c>true</c> if you should [generate schema]; otherwise, <c>false</c>.</value>
        public bool GenerateSchema { get; }

        /// <summary>
        /// Logger for the system
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        /// <value>The mappings.</value>
        public Dictionary<Type, IMapping> Mappings { get; }

        /// <summary>
        /// Gets or sets the ObjectPool.
        /// </summary>
        /// <value>The ObjectPool.</value>
        public ObjectPool<StringBuilder> ObjectPool { get; }

        /// <summary>
        /// Gets a value indicating whether to [optimize].
        /// </summary>
        /// <value><c>true</c> if you should [optimize]; otherwise, <c>false</c>.</value>
        public bool Optimize { get; }

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
        public Dictionary<Type, Tree<Type>?> TypeGraphs { get; }

        /// <summary>
        /// Gets a value indicating whether to [update schema].
        /// </summary>
        /// <value><c>true</c> if you should [update schema]; otherwise, <c>false</c>.</value>
        public bool UpdateSchema { get; }

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
            if (objectType is null)
                yield break;

            if (objectType.Namespace.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal))
                objectType = objectType.BaseType;

            if (!ChildTypes.ContainsKey(objectType))
                yield break;

            foreach (var Item in ChildTypes[objectType])
            {
                yield return Mappings[Item];
            }
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
            if (objectType is null)
                yield break;
            if (objectType.Namespace.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal))
            {
                objectType = objectType.BaseType;
            }
            if (!ParentTypes.ContainsKey(objectType))
                yield break;

            foreach (var Item in ParentTypes[objectType])
            {
                yield return Mappings[Item];
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(_ToString))
            {
                var Builder = ObjectPool.Get();
                Builder.AppendLineFormat("Source: {0}", Source.Name);
                foreach (var Mapping in Mappings.Values)
                {
                    Builder.AppendLineFormat("\tMapping: {0}", Mapping)
                        .AppendLine("\t\tIDs:");
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
                _ToString = Builder.ToString();
                ObjectPool.Return(Builder);
            }
            return _ToString;
        }

        /// <summary>
        /// Merges the mappings.
        /// </summary>
        private void MergeMappings()
        {
            if (!Optimize)
                return;

            Logger.Information("Merging mappings for {Name:l}", Source.Name);
            foreach (var TempTypeGraph in TypeGraphs.Values)
            {
                MergeMapping.Merge(TempTypeGraph, Mappings, Logger);
            }
        }

        /// <summary>
        /// Reduces the mappings.
        /// </summary>
        private void ReduceMappings()
        {
            if (!Optimize)
                return;

            Logger.Information("Reducing mappings for {Name:l}", Source.Name);
            foreach (var TempTypeGraph in TypeGraphs.Values)
            {
                ReduceMapping.Reduce(TempTypeGraph, Mappings, Logger);
            }
        }

        /// <summary>
        /// Removes the dead mappings.
        /// </summary>
        private void RemoveDeadMappings()
        {
            if (!Optimize)
                return;

            var NeededTypes = new List<Type>();

            foreach (var Mapping in Mappings.Keys)
            {
                NeededTypes.AddIfUnique(ChildTypes[Mapping]);
                NeededTypes.AddIfUnique(ParentTypes[Mapping]);
            }
            foreach (var Item in Mappings.Keys.Where(x => !NeededTypes.Contains(x)))
            {
                if (IsDebug)
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
            if (!Optimize)
                return;

            foreach (var CurrentTree in TypeGraphs.Values)
            {
                if (CurrentTree is null)
                    continue;
                var CurrentMapping = Mappings[CurrentTree.Root.Data];
                if (CurrentMapping.IDProperties.Count > 0)
                    continue;
                if (IsDebug)
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
            ConcreteTypes = DiscoverConcreteTypes.FindConcreteTypes(TypeGraphs);
            for (var i = 0; i < ConcreteTypes.Length; i++)
            {
                var ConcreteType = ConcreteTypes[i];
                if (ConcreteType is null)
                    continue;
                var Types = TypeGraphs[ConcreteType]?.ToList();
                if (Types is null)
                    continue;
                for (var x = 0; x < Types.Count; x++)
                {
                    ChildTypes.Add(Types[x], ConcreteType);
                }
            }
        }

        /// <summary>
        /// Sets up the parent types.
        /// </summary>
        private void SetupParentTypes()
        {
            Logger.Information("Setting up parent type discovery for {Name:l}", Source.Name);
            for (var i = 0; i < ConcreteTypes.Length; i++)
            {
                var ConcreteType = ConcreteTypes[i];
                var Types = TypeGraphs[ConcreteType]?.ToList();
                if (Types is null)
                    continue;
                ParentTypes.Add(ConcreteType, Types);
            }
        }
    }
}