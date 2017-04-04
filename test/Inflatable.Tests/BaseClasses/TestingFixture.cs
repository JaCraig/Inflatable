using FileCurator.Registration;
using Inflatable.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SQLHelper.ExtensionMethods;
using System;
using System.Collections.Generic;
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
            SetupConfiguration();
            SetupIoC();
            SetupDatabases();
        }

        public IConfigurationRoot Configuration { get; set; }

        protected string ConnectionString => "Data Source=localhost;Initial Catalog=TestDatabase;Integrated Security=SSPI;Pooling=false";

        protected string ConnectionString2 => "Data Source=localhost;Initial Catalog=TestDatabase2;Integrated Security=SSPI;Pooling=false";

        protected string DatabaseName => "TestDatabase";

        protected string MasterString => "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false";

        public void Dispose()
        {
            using (var TempConnection = SqlClientFactory.Instance.CreateConnection())
            {
                TempConnection.ConnectionString = MasterString;
                using (var TempCommand = TempConnection.CreateCommand())
                {
                    try
                    {
                        TempCommand.CommandText = "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase";
                        TempCommand.Open();
                        TempCommand.ExecuteNonQuery();
                    }
                    catch { }
                    finally { TempCommand.Close(); }
                }
            }
            using (var TempConnection = SqlClientFactory.Instance.CreateConnection())
            {
                TempConnection.ConnectionString = MasterString;
                using (var TempCommand = TempConnection.CreateCommand())
                {
                    try
                    {
                        TempCommand.CommandText = "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2";
                        TempCommand.Open();
                        TempCommand.ExecuteNonQuery();
                    }
                    catch { }
                    finally { TempCommand.Close(); }
                }
            }
        }

        private void SetupConfiguration()
        {
            var dict = new Dictionary<string, string>
                {
                    { "ConnectionStrings:Default", ConnectionString },
                    { "ConnectionStrings:Default2", ConnectionString2 }
                };
            Configuration = new ConfigurationBuilder()
                             .AddInMemoryCollection(dict)
                             .Build();
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
                var Container = Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                                                .AddAssembly(typeof(TestingFixture).GetTypeInfo().Assembly)
                                                .RegisterInflatable()
                                                .RegisterFileCurator()
                                                .Build();
                Container.Register(Configuration, ServiceLifetime.Singleton);
            }
        }
    }
}