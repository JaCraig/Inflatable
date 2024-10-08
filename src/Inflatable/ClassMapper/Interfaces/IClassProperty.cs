﻿/*
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
using Inflatable.QueryProvider;

namespace Inflatable.ClassMapper.Interfaces
{
    /// <summary>
    /// Single class property data holder
    /// </summary>
    public interface IClassProperty : IPropertyColumns
    {
        /// <summary>
        /// Gets the load property query.
        /// </summary>
        /// <value>The load property query.</value>
        Query? LoadPropertyQuery { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the parent mapping.
        /// </summary>
        /// <value>The parent mapping.</value>
        IMapping ParentMapping { get; }
    }
}