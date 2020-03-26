using BigBook.DataMapper;
using Data.Modeler;
using FileCurator.Registration;
using Holmes;
using Inflatable.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using SQLHelperDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using TestFountain.Registration;
using Xunit;

namespace Inflatable.Tests.BaseClasses
{
    [Collection("DirectoryCollection")]
    public abstract class TestingFixture : IDisposable
    {
        protected TestingFixture()
        {
            if (Canister.Builder.Bootstrapper == null)
            {
                _ = Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                                                .AddAssembly(typeof(TestingFixture).Assembly)
                                                .RegisterInflatable()
                                                .RegisterFileCurator()
                                                .RegisterTestFountain()
                                                .Build();
            }
        }

        public static Aspectus.Aspectus Aspectus => Canister.Builder.Bootstrapper.Resolve<Aspectus.Aspectus>();
        public static IConfigurationRoot Configuration => Canister.Builder.Bootstrapper.Resolve<IConfigurationRoot>();
        public static Manager DataMapper => Canister.Builder.Bootstrapper.Resolve<Manager>();
        public static DataModeler DataModeler => Canister.Builder.Bootstrapper.Resolve<DataModeler>();
        public static ILogger Logger => Canister.Builder.Bootstrapper.Resolve<ILogger>();
        public static ObjectPool<StringBuilder> ObjectPool => Canister.Builder.Bootstrapper.Resolve<ObjectPool<StringBuilder>>();
        public static Sherlock Sherlock => Canister.Builder.Bootstrapper.Resolve<Sherlock>();
        protected static string DatabaseName = "TestDatabase";
        protected static string MasterString = "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false";

        public void Dispose()
        {
            try
            {
                Task.Run(async () => await new SQLHelper(Configuration, SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
                    .CreateBatch()
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false)).GetAwaiter().GetResult();
            }
            catch { }
        }
    }
}