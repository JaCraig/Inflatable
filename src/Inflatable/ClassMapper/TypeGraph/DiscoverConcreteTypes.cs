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

using Inflatable.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflatable.ClassMapper.TypeGraph
{
    /// <summary>
    /// Discovers concrete types
    /// </summary>
    public static class DiscoverConcreteTypes
    {
        /// <summary>
        /// Find concrete types
        /// </summary>
        /// <returns>The concrete types of the mapping tree</returns>
        public static Type[] FindConcreteTypes(Dictionary<Type, Tree<Type>?> typeTrees)
        {
            return typeTrees is null
                ? []
                : [.. typeTrees.Keys.Where(keyToCheck => !typeTrees.Keys.Any(x => x != keyToCheck
                                                   && typeTrees[x]?.ContainsNode(keyToCheck, (i, j) => i == j) == true))];
        }
    }
}