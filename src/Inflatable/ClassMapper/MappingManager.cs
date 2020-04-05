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
using Inflatable.QueryProvider;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// <param name="sources">The sources.</param>
        /// <param name="queryProvider">The query provider.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="objectPool">The object pool.</param>
        /// <exception cref="ArgumentNullException">logger</exception>
        public MappingManager(
            IEnumerable<IMapping> mappings,
            IEnumerable<IDatabase> sources,
            QueryProviderManager queryProvider,
            ILogger logger,
            ObjectPool<StringBuilder> objectPool)
        {
            Logger = logger ?? Log.Logger ?? new LoggerConfiguration().CreateLogger() ?? throw new ArgumentNullException(nameof(logger));
            ObjectPool = objectPool ?? throw new ArgumentNullException(nameof(objectPool));
            mappings ??= Array.Empty<IMapping>();

            var Debug = Logger.IsEnabled(LogEventLevel.Debug);

            Logger.Information("Setting up mapping information");
            var TempSourceMappings = new ListMapping<Type, IMapping>();
            foreach (var Item in mappings)
            {
                TempSourceMappings.Add(Item.DatabaseConfigType, Item);
            }
            var FinalList = new ConcurrentBag<IMappingSource>();
            TempSourceMappings.Keys.ForEachParallel(Key =>
            {
                FinalList.Add(new MappingSource(TempSourceMappings[Key],
                                                sources.FirstOrDefault(x => x.GetType() == Key),
                                                queryProvider,
                                                Logger,
                                                ObjectPool));
            });
            Sources = FinalList.ToArray();
            if (Debug)
            {
                var Builder = ObjectPool.Get();
                Builder.AppendLine("Final Mappings:");
                for (var i = 0; i < Sources.Length; i++)
                {
                    Builder.AppendLine(Sources[i].ToString());
                }
                Logger.Debug("{Info:l}", Builder.ToString());
                ObjectPool.Return(Builder);
            }
        }

        /// <summary>
        /// To string value
        /// </summary>
        private string? _ToString;

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the ObjectPool.
        /// </summary>
        /// <value>The ObjectPool.</value>
        public ObjectPool<StringBuilder> ObjectPool { get; }

        /// <summary>
        /// Gets or sets the sources.
        /// </summary>
        /// <value>The sources.</value>
        public IMappingSource[] Sources { get; set; }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(_ToString))
            {
                var Builder = ObjectPool.Get();
                for (var i = 0; i < Sources.Length; i++)
                {
                    Builder.AppendLine(Sources[i].ToString());
                }
                _ToString = Builder.ToString();
                ObjectPool.Return(Builder);
            }
            return _ToString;
        }
    }
}