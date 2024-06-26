﻿using Inflatable.DataSource;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Inflatable.Tests.MockClasses
{
    public class SecondMockDatabaseMapping : Interfaces.IDatabase
    {
        public string Name => "SecondMockDatabase";

        public int Order => 3;

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