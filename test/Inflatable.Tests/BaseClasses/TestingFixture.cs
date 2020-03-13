using FileCurator.Registration;
using Inflatable.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SQLHelperDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TestFountain.Registration;
using Xunit;

namespace Inflatable.Tests.BaseClasses
{
    [Collection("DirectoryCollection")]
    public abstract class TestingFixture : IDisposable
    {
        protected TestingFixture()
        {
            Canister.Builder.Bootstrapper?.Dispose();
            _ = Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                                            .AddAssembly(typeof(TestingFixture).Assembly)
                                            .RegisterInflatable()
                                            .RegisterFileCurator()
                                            .RegisterTestFountain()
                                            .Build();
        }

        protected static string DatabaseName = "TestDatabase";
        protected static string MasterString = "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false";
        public static IConfigurationRoot Configuration => Canister.Builder.Bootstrapper.Resolve<IConfigurationRoot>();

        public static ILogger Logger => Canister.Builder.Bootstrapper.Resolve<ILogger>();

        public void Dispose()
        {
            try
            {
                new SQLHelper(Configuration, SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
                    .CreateBatch()
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalar<int>();
                Canister.Builder.Bootstrapper?.Dispose();
            }
            catch { }
        }
    }
}