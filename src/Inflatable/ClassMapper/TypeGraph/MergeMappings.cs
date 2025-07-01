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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Inflatable.ClassMapper.TypeGraph
{
    /// <summary>
    /// Merge mappings as needed
    /// </summary>
    public static class MergeMapping
    {
        /// <summary>
        /// Merges this instance.
        /// </summary>
        /// <param name="typeGraph">The type graph.</param>
        /// <param name="mappings">The mappings.</param>
        /// <param name="logger">The logger.</param>
        public static void Merge(Tree<Type>? typeGraph, Dictionary<Type, IMapping> mappings, ILogger? logger)
        {
            if (typeGraph is null || mappings is null)
                return;
            var CurrentNode = typeGraph.Root;
            if (CurrentNode.Nodes.Count == 0)
            {
                return;
            }

            for (var X = 0; X < CurrentNode.Nodes.Count; ++X)
            {
                if (MergeNode(CurrentNode.Nodes[X], mappings, logger))
                {
                    CurrentNode.Nodes[X].Remove();
                    --X;
                }
            }
        }

        /// <summary>
        /// Merges the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="mappings">The mappings.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>True if it is merged, false otherwise.</returns>
        private static bool MergeNode(TreeNode<Type> node, Dictionary<Type, IMapping> mappings, ILogger? logger)
        {
            for (var X = 0; X < node.Nodes.Count; ++X)
            {
                if (MergeNode(node.Nodes[X], mappings, logger))
                {
                    node.Nodes[X].Remove();
                    --X;
                }
            }
            var Mapping = mappings[node.Data];
            if (node.Parent != null && node.Nodes.Count == 0 && Mapping.IDProperties.Count == 0)
            {
                var MappingParent = mappings[node.Parent.Data];
                MappingParent.Copy(Mapping);
                logger?.LogDebug("Merging {objectTypeName} into {parentObjectTypeName}", Mapping.ObjectType.Name, MappingParent.ObjectType.Name);
                return true;
            }
            if (node.Parent != null && Mapping.Merge)
            {
                var MappingParent = mappings[node.Parent.Data];
                MappingParent.Copy(Mapping);
                node.Parent.Nodes.AddRange(node.Nodes);
                logger?.LogDebug("Merging {objectTypeName} into {parentObjectTypeName}", Mapping.ObjectType.Name, MappingParent.ObjectType.Name);
                return true;
            }
            return false;
        }
    }
}