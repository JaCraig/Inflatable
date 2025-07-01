using Inflatable.DataSource;
using Inflatable.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Inflatable.Benchmarks.Models.Database
{
    /// <summary>
    /// Represents the database mapping configuration for the test database.
    /// </summary>
    public class TestDatabaseMapping : IDatabase
    {
        /// <summary>
        /// Gets the name associated with the database/connection string.
        /// </summary>
        public string Name => "Default";

        /// <summary>
        /// Gets the order that this database should be in.
        /// </summary>
        public int Order => 1;

        /// <summary>
        /// Gets the database provider factory.
        /// </summary>
        public DbProviderFactory Provider => SqlClientFactory.Instance;

        /// <summary>
        /// Gets the source options for the database.
        /// </summary>
        public Options SourceOptions => new()
        {
            Optimize = true,
            Access = Enums.SourceAccess.Read | Enums.SourceAccess.Write,
            Audit = false,
            SchemaUpdate = Enums.SchemaGeneration.UpdateSchema,
            Analysis = Enums.SchemaAnalysis.NoAnalysis
        };
    }
}