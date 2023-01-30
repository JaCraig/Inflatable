using Inflatable.DataSource;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Inflatable.Tests.MockClasses
{
    public class MockDatabaseMappingForMockMapping : Interfaces.IDatabase
    {
        public string Name => "MockDatabaseForMockMapping";

        public int Order => 2;

        public DbProviderFactory Provider => SqlClientFactory.Instance;

        public Options SourceOptions => new Options
        {
            Optimize = true,
            Access = Enums.SourceAccess.Read | Enums.SourceAccess.Write,
            Audit = false,
            SchemaUpdate = Enums.SchemaGeneration.UpdateSchema
        };
    }
}