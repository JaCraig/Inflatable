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
using System.Reflection;
using Xunit;

namespace Inflatable.Tests.BaseClasses
{
    [Collection("DirectoryCollection")]
    public class TestingFixture : IDisposable
    {
        public TestingFixture()
        {
            SetupIoC();
            SetupDatabases();
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
            }
            catch { }
        }

        private void SetupDatabases()
        {
            //using (var TempConnection = SqlClientFactory.Instance.CreateConnection())
            //{
            //    TempConnection.ConnectionString = MasterString;
            //    using (var TempCommand = TempConnection.CreateCommand())
            //    {
            //        try
            //        {
            //            TempCommand.CommandText = "Create Database TestDatabase";
            //            TempCommand.Open();
            //            TempCommand.ExecuteNonQuery();
            //        }
            //        catch { }
            //        finally { TempCommand.Close(); }
            //    }
            //}
            //using (var TempConnection = SqlClientFactory.Instance.CreateConnection())
            //{
            //    TempConnection.ConnectionString = MasterString;
            //    using (var TempCommand = TempConnection.CreateCommand())
            //    {
            //        try
            //        {
            //            TempCommand.CommandText = "Create Database TestDatabase2";
            //            TempCommand.Open();
            //            TempCommand.ExecuteNonQuery();
            //        }
            //        catch { }
            //        finally { TempCommand.Close(); }
            //    }
            //}
        }

        private void SetupIoC()
        {
            if (Canister.Builder.Bootstrapper == null)
            {
                _ = Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                                                .AddAssembly(typeof(TestingFixture).GetTypeInfo().Assembly)
                                                .RegisterInflatable()
                                                .RegisterFileCurator()
                                                .Build();
            }
        }
    }
}