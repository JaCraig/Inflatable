using Inflatable.DataSource;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Inflatable.Tests.MockClasses
{
    public class MockDatabaseMapping : Interfaces.IDatabase
    {
        public string Name => "MockDatabase";

        public int Order => 2;

        public DbProviderFactory Provider => SqlClientFactory.Instance;

        public Options SourceOptions => new()
        {
            Optimize = true,
            Access = Enums.SourceAccess.None,
            Audit = false,
            SchemaUpdate = Enums.SchemaGeneration.UpdateSchema
        };
    }
}