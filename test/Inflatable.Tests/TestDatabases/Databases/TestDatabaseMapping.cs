using Inflatable.DataSource;
using Inflatable.Interfaces;
using System.Data.Common;
using System.Data.SqlClient;

namespace Inflatable.Tests.TestDatabases.Databases
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
            Audit = true,
            SchemaUpdate = Enums.SchemaGeneration.UpdateSchema
        };
    }
}