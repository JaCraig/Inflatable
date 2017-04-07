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
using Data.Modeler.Providers.Interfaces;
using Inflatable.ClassMapper;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
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
        /// Initializes a new instance of the <see cref="DataModel"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// source
        /// or
        /// config
        /// or
        /// logger
        /// </exception>
        public DataModel(MappingSource source, IConfiguration config, ILogger logger)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            Logger = logger ?? Log.Logger ?? new LoggerConfiguration().CreateLogger() ?? throw new ArgumentNullException(nameof(logger));
            Source = source ?? throw new ArgumentNullException(nameof(source));

            SourceConnection = new Connection(config, source.Source.Provider, source.Source.Name);
            SourceSpec = DataModeler.CreateSource(SourceConnection.DatabaseName);
            GenerateSchema(source);
        }

        /// <summary>
        /// Gets the generated schema changes.
        /// </summary>
        /// <value>
        /// The generated schema changes.
        /// </value>
        public IEnumerable<string> GeneratedSchemaChanges { get; private set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ILogger Logger { get; }

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
        public ISource SourceSpec { get; }

        /// <summary>
        /// Gets the source connection.
        /// </summary>
        /// <value>
        /// The source connection.
        /// </value>
        private IConnection SourceConnection { get; }

        /// <summary>
        /// Generates the schema.
        /// </summary>
        /// <param name="source">The source.</param>
        private void GenerateSchema(MappingSource source)
        {
            bool Debug = Logger.IsEnabled(LogEventLevel.Debug);

            var Generator = DataModeler.GetSchemaGenerator(source.Source.Provider);

            Logger.Information("Getting structure for {Info:l}", SourceConnection.DatabaseName);
            var OriginalSource = !string.IsNullOrEmpty(SourceConnection.DatabaseName) ? Generator.GetSourceStructure(SourceConnection) : null;

            SetupTableStructures();

            Logger.Information("Generating schema changes for {Info:l}", SourceConnection.DatabaseName);
            GeneratedSchemaChanges = Generator.GenerateSchema(SourceSpec, OriginalSource);
            if (Debug)
            {
                Logger.Debug("Schema changes generated: {GeneratedSchemaChanges}", GeneratedSchemaChanges);
            }

            if (Source.Source.Update)
            {
                Logger.Information("Applying schema changes for {Info:l}", SourceConnection.DatabaseName);
                Generator.Setup(SourceSpec, SourceConnection);
            }
        }

        /// <summary>
        /// Sets up the foreign keys.
        /// </summary>
        private void SetupForeignKeys()
        {
            Logger.Information("Setting up foreign keys for {Info:l}", SourceConnection.DatabaseName);
            SourceSpec.Tables.ForEach(x => x.SetupForeignKeys());
        }

        /// <summary>
        /// Sets up the tables.
        /// </summary>
        private void SetupTables()
        {
            Logger.Information("Setting up table structure for {Info:l}", SourceConnection.DatabaseName);
            foreach (var Mapping in Source.Mappings.Values.OrderBy(x => x.Order))
            {
                var Table = SourceSpec.AddTable(Mapping.TableName);
                var Tree = Source.TypeGraphs[Mapping.ObjectType];
                var ParentMappings = Tree.Root.Nodes.ForEach(x => Source.Mappings[x.Data]);
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