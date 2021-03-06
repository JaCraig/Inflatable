﻿using Inflatable.DataSource;
using System.Data.Common;
using System.Data.SqlClient;

namespace Inflatable.Tests.MockClasses
{
    public class MockDatabaseMapping : Interfaces.IDatabase
    {
        public string Name => "MockDatabase";

        public int Order => 2;

        public DbProviderFactory Provider => SqlClientFactory.Instance;

        public Options SourceOptions => new Options
        {
            Optimize = true,
            Access = Enums.SourceAccess.None,
            Audit = false,
            SchemaUpdate = Enums.SchemaGeneration.UpdateSchema
        };
    }
}