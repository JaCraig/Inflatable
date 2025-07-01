using Inflatable.DataSource;
using Inflatable.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Inflatable.Tests.TestDatabases.Databases
{
    public class TestDatabaseMapping : IDatabase
    {
        public string Name => "Default";

        public int Order => 1;

        public DbProviderFactory Provider => SqlClientFactory.Instance;

        public Options SourceOptions => new()
        {
            Optimize = true,
            Access = Enums.SourceAccess.Read | Enums.SourceAccess.Write,
            Audit = true,
            SchemaUpdate = Enums.SchemaGeneration.UpdateSchema,
            Analysis = Enums.SchemaAnalysis.ApplyAnalysis
        };
    }
}