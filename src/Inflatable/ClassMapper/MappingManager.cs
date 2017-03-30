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
        /// <param name="logger">The logger.</param>
        public MappingManager(IEnumerable<IMapping> mappings, IEnumerable<IDatabase> sources, ILogger logger)
        {
            Logger = logger ?? Log.Logger ?? new LoggerConfiguration().CreateLogger();
            if (Logger == null)
                throw new ArgumentNullException(nameof(logger));
            mappings = mappings ?? new List<IMapping>();
            bool Debug = Logger.IsEnabled(LogEventLevel.Debug);
            Logger.Information("Setting up mapping information");
            var TempSourceMappings = new ListMapping<Type, IMapping>();
            mappings.ForEachParallel(x => TempSourceMappings.Add(x.DatabaseConfigType, x));
            var FinalList = new ConcurrentBag<MappingSource>();
            TempSourceMappings.Keys.ForEachParallel(Key =>
            {
                FinalList.Add(new MappingSource(TempSourceMappings[Key],
                                                sources.FirstOrDefault(x => x.GetType() == Key),
                                                logger));
            });
            Sources = FinalList;
            if (Debug)
            {
                StringBuilder Builder = new StringBuilder();
                Builder.AppendLine("Final Mappings:");
                foreach (var Source in Sources)
                {
                    Builder.AppendLine(Source.ToString());
                }
                Logger.Debug("{Info:l}", Builder.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the sources.
        /// </summary>
        /// <value>The sources.</value>
        public IEnumerable<MappingSource> Sources { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            StringBuilder Builder = new StringBuilder();
            foreach (var Source in Sources)
            {
                Builder.AppendLine(Source.ToString());
            }
            return Builder.ToString();
        }
    }
}