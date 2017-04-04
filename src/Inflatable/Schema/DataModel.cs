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

using BigBook;
using Data.Modeler;
using Inflatable.ClassMapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Inflatable.Schema
{
    /// <summary>
    /// Data model class
    /// </summary>
    public class DataModel
    {
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
            SetupTableStructures();
            GeneratedSchemaChanges = Generator.GenerateSchema(SourceSpec, OriginalSource);
            if (Source.Source.Update)
                Generator.Setup(SourceSpec, SourceConnection);
        }

        /// <summary>
        /// Gets the generated schema changes.
        /// </summary>
        /// <value>
        /// The generated schema changes.
        /// </value>
        public IEnumerable<string> GeneratedSchemaChanges { get; }

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
        /// Sets up the foreign keys.
        /// </summary>
        private void SetupForeignKeys()
        {
            SourceSpec.Tables.ForEach(x => x.SetupForeignKeys());
        }

        /// <summary>
        /// Sets up the tables.
        /// </summary>
        private void SetupTables()
        {
            foreach (var Mapping in Source.Mappings.Values.OrderBy(x => x.Order))
            {
                var Table = SourceSpec.AddTable(Mapping.TableName);
                var Tree = Source.TypeGraphs[Mapping.ObjectType];
                var ParentMappings = Tree.Root.Nodes.Select(x => Source.Mappings[x.Data]);
                foreach (var ID in Mapping.IDProperties)
                {
                    ID.AddToTable(Table);
                }
                foreach (var ID in Mapping.AutoIDProperties)
                {
                    ID.AddToTable(Table);
                }
                foreach (var Reference in Mapping.ReferenceProperties)
                {
                    Reference.AddToTable(Table);
                }
                foreach (var ParentMapping in ParentMappings)
                {
                    foreach (var ID in ParentMapping.IDProperties)
                    {
                        ID.AddToChildTable(Table);
                    }
                    foreach (var ID in ParentMapping.AutoIDProperties)
                    {
                        ID.AddToChildTable(Table);
                    }
                }
            }
        }

        /// <summary>
        /// Sets up the table structures.
        /// </summary>
        private void SetupTableStructures()
        {
            SetupTables();
            SetupForeignKeys();
        }
    }
}