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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Inflatable.ClassMapper.TypeGraph
{
    /// <summary>
    /// Discovers concrete types
    /// </summary>
    public class DiscoverConcreteTypes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscoverConcreteTypes"/> class.
        /// </summary>
        /// <param name="typeTrees">The type trees.</param>
        /// <exception cref="System.ArgumentNullException">typeTrees</exception>
        public DiscoverConcreteTypes(IDictionary<Type, Tree<Type>> typeTrees)
        {
            TypeTrees = typeTrees ?? throw new ArgumentNullException(nameof(typeTrees));
        }

        /// <summary>
        /// Gets or sets the type trees.
        /// </summary>
        /// <value>The type trees.</value>
        public IDictionary<Type, Tree<Type>> TypeTrees { get; set; }

        /// <summary>
        /// Find concrete types
        /// </summary>
        /// <returns>The concrete types of the mapping tree</returns>
        public IEnumerable<Type> FindConcreteTypes()
        {
            ConcurrentBag<Type> Result = new ConcurrentBag<Type>();
            for (int x = 0; x < TypeTrees.Keys.Count; ++x)
            {
                var KeyToCheck = TypeTrees.Keys.ElementAt(x);
                var Found = false;
                for (int y = 0; y < TypeTrees.Keys.Count; ++y)
                {
                    var CurrentKey = TypeTrees.Keys.ElementAt(y);
                    if (CurrentKey != KeyToCheck)
                    {
                        Found = TypeTrees[CurrentKey].ContainsNode(KeyToCheck, (i, j) => i == j);
                        if (Found)
                            break;
                    }
                }
                if (!Found)
                {
                    Result.Add(KeyToCheck);
                }
            }
            return Result;
        }
    }
}