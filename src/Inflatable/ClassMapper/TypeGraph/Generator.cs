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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflatable.ClassMapper.TypeGraph
{
    /// <summary>
    /// Type graph generator
    /// </summary>
    public static class Generator
    {
        /// <summary>
        /// Generates the specified mapping type.
        /// </summary>
        /// <param name="mappingType">Type of the mapping.</param>
        /// <param name="mappings">The mappings.</param>
        /// <returns>The type graph associated with the type.</returns>
        public static Tree<Type>? Generate(Type? mappingType, Dictionary<Type, IMapping> mappings)
        {
            if (mappingType is null || !(mappings?.ContainsKey(mappingType) ?? false))
                return null;

            var TempTypeGraph = new Tree<Type>(mappingType);
            mappingType = mappingType.BaseType;
            TreeNode<Type>? CurrentNode = TempTypeGraph.Root;
            while (mappingType is not null)
            {
                if (mappings.ContainsKey(mappingType))
                {
                    CurrentNode = CurrentNode.AddNode(mappingType);
                }
                mappingType = mappingType.BaseType;
            }
            while (CurrentNode is not null)
            {
                var CurrentInterfaces = CurrentNode.Data.GetInterfaces();
                var MaxLength = CurrentInterfaces.Length;
                if (MaxLength != 0)
                {
                    var PotentialNodes = new Tree<Type>?[MaxLength];
                    for (var X = 0; X < MaxLength; X++)
                    {
                        var Interface = CurrentInterfaces[X];
                        if (!TempTypeGraph.ContainsNode(Interface, (z, y) => z == y))
                        {
                            PotentialNodes[X] = Generate(Interface, mappings);
                        }
                    }
                    for (var X = 0; X < MaxLength; ++X)
                    {
                        var PotentialNode = PotentialNodes[X];
                        if (PotentialNode is null)
                        {
                            continue;
                        }

                        for (var Y = 0; Y < MaxLength; ++Y)
                        {
                            if (X != Y && PotentialNode.ContainsNode(CurrentInterfaces[Y], (i, j) => i == j))
                            {
                                PotentialNodes[Y] = null;
                            }
                        }
                    }
                    for (var X = 0; X < MaxLength; ++X)
                    {
                        var PotentialNode = PotentialNodes[X];
                        if (PotentialNode is null)
                            continue;
                        PotentialNode.Root.Parent = CurrentNode;
                        CurrentNode.Nodes.Add(PotentialNode.Root);
                    }
                }
                CurrentNode = CurrentNode.Parent;
            }

            return TempTypeGraph;
        }
    }
}