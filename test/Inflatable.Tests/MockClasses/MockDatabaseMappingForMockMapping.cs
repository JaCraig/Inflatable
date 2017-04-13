using Inflatable.DataSource;
using System.Data.Common;
using System.Data.SqlClient;

namespace Inflatable.Tests.MockClasses
{
    public class MockDatabaseMappingForMockMapping : Interfaces.IDatabase
    {
        public string Name => "MockDatabase";

        public int Order => 1;

        public DbProviderFactory Provider => SqlClientFactory.Instance;

        public Options SourceOptions => new Options
        {
            Optimize = true,
            Access = Enums.SourceAccess.Read | Enums.SourceAccess.Write,
            Audit = false,
            SchemaUpdate = Enums.SchemaGeneration.NoGeneration
        };
    }
}