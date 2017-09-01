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

using Inflatable.ClassMapper.Interfaces;
using Inflatable.Interfaces;
using System.Collections.Generic;

namespace Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators.HelperClasses
{
    /// <summary>
    /// Generator query data
    /// </summary>
    public class QueryGeneratorData
    {
        /// <summary>
        /// Gets or sets the associated mapping.
        /// </summary>
        /// <value>The associated mapping.</value>
        public IMapping AssociatedMapping { get; set; }

        /// <summary>
        /// Gets or sets the identifier properties.
        /// </summary>
        /// <value>The identifier properties.</value>
        public IEnumerable<IIDProperty> IDProperties { get; set; }

        /// <summary>
        /// Gets or sets the query text.
        /// </summary>
        /// <value>The query text.</value>
        public string QueryText { get; set; }
    }
}