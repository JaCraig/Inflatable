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
using System.Linq;

namespace Inflatable.Utils
{
    /// <summary>
    /// Tree node
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public class TreeNode<TData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode{TData}"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="parent">The parent.</param>
        public TreeNode(TData data, TreeNode<TData> parent)
        {
            Data = data;
            Parent = parent;
            Nodes = new List<TreeNode<TData>>();
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public TData Data { get; set; }

        /// <summary>
        /// Gets or sets the nodes.
        /// </summary>
        /// <value>The nodes.</value>
        public List<TreeNode<TData>> Nodes { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public TreeNode<TData> Parent { get; set; }

        /// <summary>
        /// Adds the node.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The resulting node.</returns>
        public TreeNode<TData> AddNode(TData data)
        {
            var TempNode = new TreeNode<TData>(data, this);
            Nodes.Add(TempNode);
            return TempNode;
        }

        /// <summary>
        /// Determines whether the specified data contains node.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns><c>true</c> if the specified data contains node; otherwise, <c>false</c>.</returns>
        public bool ContainsNode(TData data, Func<TData, TData, bool> comparer) => comparer(data, Data) || Nodes.Any(x => x.ContainsNode(data, comparer));

        /// <summary>
        /// Removes this instance from the tree
        /// </summary>
        /// <returns>This</returns>
        public TreeNode<TData> Remove()
        {
            Parent.Nodes.Remove(this);
            Parent = null;
            return this;
        }

        /// <summary>
        /// Returns the sub tree as a list.
        /// </summary>
        /// <returns>The resulting list.</returns>
        public List<TData> ToList()
        {
            List<TData> ReturnList = new List<TData>
            {
                Data
            };
            foreach (var Node in Nodes)
            {
                ReturnList.AddRange(Node.ToList());
            }
            return ReturnList;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return Data.ToString();
        }
    }
}