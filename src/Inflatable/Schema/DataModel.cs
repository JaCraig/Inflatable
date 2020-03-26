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
using BigBook.DataMapper;
using Data.Modeler;
using Data.Modeler.Providers.Interfaces;
using Holmes;
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using Serilog.Events;
using SQLHelperDB;
using SQLHelperDB.HelperClasses;
using SQLHelperDB.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="dataModeler">The data modeler.</param>
        /// <param name="sherlock">The sherlock.</param>
        /// <param name="stringBuilderPool">The string builder pool.</param>
        /// <param name="aopManager">The aop manager.</param>
        /// <param name="dataMapper">The data mapper.</param>
        /// <exception cref="ArgumentNullException">source or config or logger</exception>
        public DataModel(IMappingSource source, IConfiguration config, ILogger logger, DataModeler dataModeler, Sherlock sherlock, ObjectPool<StringBuilder> stringBuilderPool, Aspectus.Aspectus aopManager, Manager dataMapper)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Logger = logger ?? Log.Logger ?? new LoggerConfiguration().CreateLogger() ?? throw new ArgumentNullException(nameof(logger));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            GeneratedSchemaChanges = Array.Empty<string>();

            SourceConnection = new Connection(config, source.Source.Provider, source.Source.Name);
            SourceSpec = DataModeler.CreateSource(SourceConnection.DatabaseName ?? "");
            DataModeler = dataModeler;
            Sherlock = sherlock;
            StringBuilderPool = stringBuilderPool;
            AopManager = aopManager;
            DataMapper = dataMapper;
            Task.Run(async () => await GenerateSchemaAsync(source).ConfigureAwait(false)).GetAwaiter().GetResult();
            Task.Run(async () => await AnalyzeSchemaAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the aop manager.
        /// </summary>
        /// <value>The aop manager.</value>
        public Aspectus.Aspectus AopManager { get; }

        /// <summary>
        /// Gets the data mapper.
        /// </summary>
        /// <value>The data mapper.</value>
        public Manager DataMapper { get; }

        /// <summary>
        /// Gets the data modeler.
        /// </summary>
        /// <value>The data modeler.</value>
        public DataModeler DataModeler { get; }

        /// <summary>
        /// Gets the generated schema changes.
        /// </summary>
        /// <value>The generated schema changes.</value>
        public IEnumerable<string> GeneratedSchemaChanges { get; private set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets the sherlock.
        /// </summary>
        /// <value>The sherlock.</value>
        public Sherlock Sherlock { get; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        public IMappingSource Source { get; }

        /// <summary>
        /// Gets the source spec.
        /// </summary>
        /// <value>The source spec.</value>
        public ISource SourceSpec { get; }

        /// <summary>
        /// Gets the string builder pool.
        /// </summary>
        /// <value>The string builder pool.</value>
        public ObjectPool<StringBuilder> StringBuilderPool { get; }

        /// <summary>
        /// Gets the source connection.
        /// </summary>
        /// <value>The source connection.</value>
        private IConnection SourceConnection { get; }

        /// <summary>
        /// The default schemas
        /// </summary>
        private static readonly string[] DefaultSchemas = {
            "dbo",
            "guest",
            "INFORMATION_SCHEMA",
            "sys",
            "db_owner",
            "db_accessadmin",
            "db_securityadmin",
            "db_ddladmin",
            "db_backupoperator",
            "db_datareader",
            "db_datawriter",
            "db_denydatareader",
            "db_denydatawriter"
        };

        /// <summary>
        /// Analyze the schema.
        /// </summary>
        private async Task AnalyzeSchemaAsync()
        {
            if (!Source.ApplyAnalysis
                && !Source.GenerateAnalysis)
            {
                return;
            }

            Logger.Information("Analyzing {Info:l} for suggestions.", SourceConnection.DatabaseName);
            var Results = await Sherlock.AnalyzeAsync(SourceConnection).ConfigureAwait(false);
            var Batch = new SQLHelper(SourceConnection, StringBuilderPool, AopManager, DataMapper);
            foreach (var Result in Results)
            {
                Logger.Information("Finding: {Info:l}", Result.Text);
                Logger.Information("Metrics: {Data:l}", Result.Metrics);
                Logger.Information("Suggested Fix: {Fix:l}", Result.Fix);
                if (Source.ApplyAnalysis && string.IsNullOrEmpty(Result.Fix))
                {
                    Batch.AddQuery(CommandType.Text, Result.Fix);
                }
            }
            if (Source.ApplyAnalysis)
            {
                Logger.Information("Applying fixes for {Info:l}.", SourceConnection.DatabaseName);
                await Batch.ExecuteScalarAsync<int>().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Generates the schema.
        /// </summary>
        /// <param name="source">The source.</param>
        private async Task GenerateSchemaAsync(IMappingSource source)
        {
            if (!Source.UpdateSchema
                && !Source.GenerateSchema)
            {
                return;
            }

            var Debug = Logger.IsEnabled(LogEventLevel.Debug);

            var Generator = DataModeler.GetSchemaGenerator(source.Source.Provider);
            if (Generator is null)
                return;

            Logger.Information("Getting structure for {Info:l}", SourceConnection.DatabaseName);
            var OriginalSource = !string.IsNullOrEmpty(SourceConnection.DatabaseName) ? (await Generator.GetSourceStructureAsync(SourceConnection).ConfigureAwait(false)) : null;

            SetupTableStructures();

            Logger.Information("Generating schema changes for {Info:l}", SourceConnection.DatabaseName);
            GeneratedSchemaChanges = Generator.GenerateSchema(SourceSpec, OriginalSource!) ?? Array.Empty<string>();
            if (Debug)
            {
                Logger.Debug("Schema changes generated: {GeneratedSchemaChanges}", GeneratedSchemaChanges);
            }

            if (!Source.UpdateSchema)
            {
                return;
            }

            Logger.Information("Applying schema changes for {Info:l}", SourceConnection.DatabaseName);
            await Generator.SetupAsync(SourceSpec, SourceConnection).ConfigureAwait(false);
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
                if (!DefaultSchemas.Contains(Mapping.SchemaName))
                {
                    SourceSpec.Schemas.AddIfUnique(Mapping.SchemaName);
                }

                var Table = SourceSpec.AddTable(Mapping.TableName, Mapping.SchemaName);
                var Tree = Source.TypeGraphs[Mapping.ObjectType];
                var ParentMappings = Tree?.Root.Nodes.ForEach(x => Source.Mappings[x.Data]) ?? Array.Empty<IMapping>();
                foreach (var ID in Mapping.IDProperties)
                {
                    ID.Setup();
                    ID.AddToTable(Table);
                }
                foreach (var ID in Mapping.AutoIDProperties)
                {
                    ID.Setup();
                    ID.AddToTable(Table);
                }
                foreach (var Reference in Mapping.ReferenceProperties)
                {
                    Reference.Setup();
                    Reference.AddToTable(Table);
                }
                foreach (var Map in Mapping.MapProperties)
                {
                    Map.Setup(Source);
                    Map.AddToTable(Table);
                }
                foreach (var Map in Mapping.ManyToManyProperties)
                {
                    Map.Setup(Source, this);
                }
                foreach (var ParentMapping in ParentMappings)
                {
                    foreach (var ID in ParentMapping.IDProperties)
                    {
                        ID.Setup();
                        ID.AddToChildTable(Table);
                    }
                    foreach (var ID in ParentMapping.AutoIDProperties)
                    {
                        ID.Setup();
                        ID.AddToChildTable(Table);
                    }
                }
            }
            foreach (var Mapping in Source.Mappings.Values.OrderBy(x => x.Order))
            {
                foreach (var Map in Mapping.ManyToOneProperties)
                {
                    Map.Setup(Source, this);
                }
            }
            foreach (var Mapping in Source.Mappings.Values.OrderBy(x => x.Order))
            {
                foreach (var Map in Mapping.ManyToOneProperties)
                {
                    Map.SetColumnInfo(Source);
                }
                foreach (var Map in Mapping.ManyToManyProperties)
                {
                    Map.SetColumnInfo(Source);
                }
                foreach (var Map in Mapping.MapProperties)
                {
                    Map.SetColumnInfo(Source);
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