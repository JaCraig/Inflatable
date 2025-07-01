using Inflatable.DataSource;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Inflatable.Tests.MockClasses
{
    public class MockDatabaseMappingForMockMapping : Interfaces.IDatabase
    {
        public string Name => "MockDatabaseForMockMapping";

        public int Order => 2;

        public DbProviderFactory Provider => SqlClientFactory.Instance;

        public Options SourceOptions => new()
        {
            Optimize = true,
            Access = Enums.SourceAccess.Read | Enums.SourceAccess.Write,
            Audit = false,
            SchemaUpdate = Enums.SchemaGeneration.UpdateSchema
        };
    }
}