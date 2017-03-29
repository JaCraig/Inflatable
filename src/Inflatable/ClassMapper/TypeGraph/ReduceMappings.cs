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
using Serilog;
using System;
using System.Collections.Generic;

namespace Inflatable.ClassMapper.TypeGraph
{
    /// <summary>
    /// Reduces mappings and removes redundant items
    /// </summary>
    public class ReduceMappings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReduceMappings"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="logger">The logger.</param>
        public ReduceMappings(IDictionary<Type, IMapping> mappings, ILogger logger)
        {
            Logger = logger;
            Mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));
        }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the mappings.
        /// </summary>
        /// <value>The mappings.</value>
        private IDictionary<Type, IMapping> Mappings { get; set; }

        /// <summary>
        /// Reduces the mapping
        /// </summary>
        /// <param name="typeGraph">The type graph.</param>
        public void Reduce(Tree<Type> typeGraph)
        {
            var Mapping = Mappings[typeGraph.Root.Data];
            Mapping.Reduce(Logger);
            foreach (var ParentType in typeGraph.ToList())
            {
                var ParentMapping = Mappings[ParentType];
                if (Mapping != ParentMapping)
                {
                    Mapping.Reduce(ParentMapping, Logger);
                }
            }
        }
    }
}