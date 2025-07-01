using Inflatable.DataSource;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Inflatable.Tests.MockClasses
{
    public class SecondMockDatabaseMapping : Interfaces.IDatabase
    {
        public string Name => "SecondMockDatabase";

        public int Order => 3;

        public DbProviderFactory Provider => SqlClientFactory.Instance;

        public Options SourceOptions => new()
        {
            Optimize = true,
            Access = Enums.SourceAccess.Read | Enums.SourceAccess.Write,
            Audit = false,
            SchemaUpdate = Enums.SchemaGeneration.NoGeneration
        };
    }
}