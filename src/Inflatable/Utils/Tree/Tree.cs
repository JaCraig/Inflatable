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

using System;
using System.Collections.Generic;

namespace Inflatable.Utils
{
    /// <summary>
    /// Tree holding data.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public class Tree<TData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tree{TData}"/> class.
        /// </summary>
        /// <param name="rootData">The root data.</param>
        public Tree(TData rootData)
        {
            Root = new TreeNode<TData>(rootData, null);
        }

        /// <summary>
        /// Gets or sets the root.
        /// </summary>
        /// <value>The root.</value>
        public TreeNode<TData> Root { get; set; }

        /// <summary>
        /// Determines whether the specified data contains node.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns><c>true</c> if the specified data contains node; otherwise, <c>false</c>.</returns>
        public bool ContainsNode(TData data, Func<TData, TData, bool> comparer) => Root.ContainsNode(data, comparer);

        /// <summary>
        /// Converts the tree to a list.
        /// </summary>
        /// <returns>The resulting list</returns>
        public List<TData> ToList()
        {
            return Root.ToList();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return Root.ToString();
        }
    }
}