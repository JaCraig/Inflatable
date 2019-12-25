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
    /// Merge mappings as needed
    /// </summary>
    public class MergeMappings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeMappings"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="logger">The logger.</param>
        public MergeMappings(IDictionary<Type, IMapping> mappings, ILogger logger)
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
        private IDictionary<Type, IMapping> Mappings { get; }

        /// <summary>
        /// Merges this instance.
        /// </summary>
        /// <param name="typeGraph">The type graph.</param>
        public void Merge(Tree<Type>? typeGraph)
        {
            if (typeGraph is null)
                return;
            var CurrentNode = typeGraph.Root;
            if (CurrentNode.Nodes.Count == 0)
            {
                return;
            }

            for (var x = 0; x < CurrentNode.Nodes.Count; ++x)
            {
                if (MergeNode(CurrentNode.Nodes[x]))
                {
                    CurrentNode.Nodes[x].Remove();
                    --x;
                }
            }
        }

        private bool MergeNode(TreeNode<Type> node)
        {
            for (var x = 0; x < node.Nodes.Count; ++x)
            {
                if (MergeNode(node.Nodes[x]))
                {
                    node.Nodes[x].Remove();
                    --x;
                }
            }
            var Mapping = Mappings[node.Data];
            if (node.Nodes.Count == 0 && Mapping.IDProperties.Count == 0)
            {
                var MappingParent = Mappings[node.Parent.Data];
                MappingParent.Copy(Mapping);
                Logger.Debug("Merging {ParentMapping:l} into {Mapping:l}", Mapping.ObjectType.Name, MappingParent.ObjectType.Name);
                return true;
            }
            if (Mapping.Merge)
            {
                var MappingParent = Mappings[node.Parent.Data];
                MappingParent.Copy(Mapping);
                node.Parent.Nodes.AddRange(node.Nodes);
                Logger.Debug("Merging {ParentMapping:l} into {Mapping:l}", Mapping.ObjectType.Name, MappingParent.ObjectType.Name);
                return true;
            }
            return false;
        }
    }
}