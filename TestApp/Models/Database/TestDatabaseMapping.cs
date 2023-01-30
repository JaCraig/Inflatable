using Inflatable.DataSource;
using Inflatable.Interfaces;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Inflatable.Benchmarks.Models.Database
{
    public class TestDatabaseMapping : IDatabase
    {
        public string Name => "Default";

        public int Order => 1;

        public DbProviderFactory Provider => SqlClientFactory.Instance;

        public Options SourceOptions => new Options()
        {
            Optimize = true,
            Access = Enums.SourceAccess.Read | Enums.SourceAccess.Write,
            Audit = false,
            SchemaUpdate = Enums.SchemaGeneration.UpdateSchema,
            Analysis = Enums.SchemaAnalysis.NoAnalysis
        };
    }
}