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

using Data.Modeler;
using Inflatable.ClassMapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Inflatable.Schema
{
    /// <summary>
    /// Data model class
    /// </summary>
    public class DataModel
    {
        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public MappingSource Source { get; }

        /// <summary>
        /// Gets the source spec.
        /// </summary>
        /// <value>
        /// The source spec.
        /// </value>
        public Data.Modeler.Providers.Interfaces.ISource SourceSpec { get; }

        /// <summary>
        /// Gets the generated schema changes.
        /// </summary>
        /// <value>
        /// The generated schema changes.
        /// </value>
        public IEnumerable<string> GeneratedSchemaChanges { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataModel" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="config">The configuration.</param>
        public DataModel(MappingSource source, IConfiguration config)
        {
            Source = source;
            SourceSpec = DataModeler.CreateSource(Source.Source.Name);
            var Generator = DataModeler.GetSchemaGenerator(source.Source.Provider);
            var SourceConnection = new Connection(config, source.Source.Provider, source.Source.Name);
            var OriginalSource = Generator.GetSourceStructure(SourceConnection);
            foreach (var Mapping in Source.Mappings)
            {
                var Table = SourceSpec.AddTable(Mapping.Value.TableName);
                foreach (var ID in Mapping.Value.IDProperties)
                {
                    ID.AddToTable(Table);
                }
                foreach (var Reference in Mapping.Value.ReferenceProperties)
                {
                    Reference.AddToTable(Table);
                }
            }
            GeneratedSchemaChanges = Generator.GenerateSchema(SourceSpec, OriginalSource);
            Generator.Setup(SourceSpec, SourceConnection);
        }
    }
}