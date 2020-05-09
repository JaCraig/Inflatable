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
using BigBook.Caching.Interfaces;
using BigBook.Comparison;
using Inflatable.ClassMapper;
using Inflatable.QueryProvider;
using Inflatable.Sessions.Commands.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inflatable.Sessions.Commands.BaseClasses
{
    /// <summary>
    /// Command base class
    /// </summary>
    /// <seealso cref="ICommand"/>
    public abstract class CommandBaseClass : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBaseClass"/> class.
        /// </summary>
        /// <param name="mappingManager">The mapping manager.</param>
        /// <param name="queryProviderManager">The query provider manager.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="objects">The objects.</param>
        protected CommandBaseClass(MappingManager mappingManager, QueryProviderManager queryProviderManager, ICache cache, object[] objects)
        {
            QueryProviderManager = queryProviderManager;
            Objects = (objects ?? Array.Empty<object>()).Where(x => x != null).ToArray();
            MappingManager = mappingManager;
            Cache = cache;
        }

        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <value>The cache.</value>
        public ICache Cache { get; }

        /// <summary>
        /// Gets the type of the command.
        /// </summary>
        /// <value>The type of the command.</value>
        public abstract Enums.CommandType CommandType { get; }

        /// <summary>
        /// Gets the objects.
        /// </summary>
        /// <value>The objects.</value>
        public object[] Objects { get; private set; }

        /// <summary>
        /// Gets the mapping manager.
        /// </summary>
        /// <value>The mapping manager.</value>
        protected MappingManager MappingManager { get; }

        /// <summary>
        /// Gets the query provider manager.
        /// </summary>
        /// <value>The query provider manager.</value>
        protected QueryProviderManager QueryProviderManager { get; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dynamoFactory">The dynamo factory.</param>
        /// <returns>The number of rows that are modified.</returns>
        public abstract int Execute(IMappingSource source, DynamoFactory dynamoFactory);

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dynamoFactory">The dynamo factory.</param>
        /// <returns>The number of rows that are modified.</returns>
        public abstract Task<int> ExecuteAsync(IMappingSource source, DynamoFactory dynamoFactory);

        /// <summary>
        /// Merges the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>True if the items are merged, false otherwise.</returns>
        public bool Merge(ICommand command)
        {
            if (command is null)
            {
                return true;
            }

            if (command.CommandType != CommandType)
            {
                return false;
            }

            if (!(command is CommandBaseClass TempCommand))
            {
                return false;
            }

            var Values = new List<object>();
            Values.AddRange(Objects);
            Values.AddRange(TempCommand.Objects);
            Objects = Values.ToArray();
            return true;
        }

        /// <summary>
        /// Determines whether this instance can execute the specified object.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="source">The source.</param>
        /// <returns>
        /// <c>true</c> if this instance can execute the specified object; otherwise, <c>false</c>.
        /// </returns>
        protected static bool CanExecute(object @object, IMappingSource source)
        {
            var TempType = GetActualType(@object);
            return source.Mappings.ContainsKey(TempType);
        }

        /// <summary>
        /// Compares the objects.
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <param name="source">The source.</param>
        /// <returns>True if they're the same, false otherwise.</returns>
        protected static bool CompareObjects(object obj1, object obj2, IMappingSource source)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            var Object1Type = obj1.GetType();
            if (Object1Type != obj2.GetType())
            {
                return false;
            }

            var ObjectIDs = source.GetParentMapping(Object1Type).SelectMany(x => x.IDProperties);
            if (!ObjectIDs.Any())
            {
                return false;
            }

            foreach (var ObjectID in ObjectIDs)
            {
                var Value1 = ObjectID.GetColumnInfo()[0].GetValue(obj1);
                var Value2 = ObjectID.GetColumnInfo()[0].GetValue(obj2);
                if (!Equals(Value1, Value2) || ObjectID.GetColumnInfo()[0].IsDefault(obj1))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines if the object was seen before.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="objectsSeen">The objects seen already.</param>
        /// <param name="source">The source.</param>
        /// <returns>True if it was seen, otherwise false.</returns>
        protected static bool WasObjectSeen(object @object, IList<object> objectsSeen, IMappingSource source) => objectsSeen.Contains(@object, new SimpleEqualityComparer<object>((x, y) => CompareObjects(x, y, source), x => x.GetHashCode()));

        /// <summary>
        /// Removes the items from cache.
        /// </summary>
        /// <param name="object">The object.</param>
        protected void RemoveItemsFromCache(object @object)
        {
            //TODO: CHANGE CACHE REMOVAL TO REMOVE INDIVIDUAL ITEM
            var TempType = GetActualType(@object);
            QueryResults.RemoveCacheTag(TempType.Name, Cache);
        }

        /// <summary>
        /// Gets the actual type.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <returns>The actual object type</returns>
        private static Type GetActualType(object @object)
        {
            var TempType = @object.GetType();
            if (TempType.Namespace.StartsWith("AspectusGeneratedTypes", StringComparison.Ordinal))
            {
                TempType = TempType.BaseType;
            }

            return TempType;
        }
    }
}