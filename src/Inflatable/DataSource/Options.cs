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

using Inflatable.Enums;

namespace Inflatable.DataSource
{
    /// <summary>
    /// Data source options
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets the access level for the source.
        /// </summary>
        /// <value>The access level for the source.</value>
        public SourceAccess Access { get; set; }

        /// <summary>
        /// Determines if audit tables are generated
        /// </summary>
        /// <value><c>true</c> if audit; otherwise, <c>false</c>.</value>
        public bool Audit { get; set; }

        /// <summary>
        /// Gets a value indicating whether this source should be optimized automatically.
        /// </summary>
        /// <value><c>true</c> if it should be optimized; otherwise, <c>false</c>.</value>
        public bool Optimize { get; set; }

        /// <summary>
        /// The level the system should update/generate schema changes for you.
        /// </summary>
        /// <value>The level the system should update/generate schema changes for you.</value>
        public SchemaGeneration SchemaUpdate { get; set; }
    }
}