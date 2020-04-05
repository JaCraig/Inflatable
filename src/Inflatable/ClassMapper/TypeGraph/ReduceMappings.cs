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
    public static class ReduceMapping
    {
        /// <summary>
        /// Reduces the mapping
        /// </summary>
        /// <param name="typeGraph">The type graph.</param>
        /// <param name="mappings">The mappings.</param>
        /// <param name="logger">The logger.</param>
        public static void Reduce(Tree<Type>? typeGraph, Dictionary<Type, IMapping> mappings, ILogger logger)
        {
            if (typeGraph is null || mappings is null)
                return;
            var Mapping = mappings[typeGraph.Root.Data];
            Mapping.Reduce(logger);
            var GraphList = typeGraph.ToList();
            for (int x = 0, maxCount = GraphList.Count; x < maxCount; x++)
            {
                var ParentMapping = mappings[GraphList[x]];
                if (Mapping != ParentMapping)
                {
                    Mapping.Reduce(ParentMapping, logger);
                }
            }
        }
    }
}