using Data.Modeler;
using Holmes;
using Inflatable.ClassMapper;
using Inflatable.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using SQLHelperDB;
using System;
using System.Text;
using Xunit;

namespace Inflatable.Tests.BaseClasses
{
    [Collection("Test collection")]
    public abstract class TestingFixture : IDisposable
    {
        //protected TestingFixture()
        //{
        //    if (Canister.Builder.Bootstrapper is null)
        //    {
        //        lock (LockObject)
        //        {
        //            var Services = new ServiceCollection();
        //            Services.AddLogging(builder => builder.AddSerilog())
        //                .AddCanisterModules();
        //            Canister.Builder.Bootstrapper.Resolve<ISession>();
        //        }
        //    }

        //    _ = SchemaManager;
        //}

        public static IConfiguration Configuration => Canister.Builder.Bootstrapper.Resolve<IConfigurationRoot>();
        public static DataModeler DataModeler => Canister.Builder.Bootstrapper.Resolve<DataModeler>();
        public static SQLHelper Helper => Canister.Builder.Bootstrapper.Resolve<SQLHelper>();
        public static MappingManager MappingManager => Canister.Builder.Bootstrapper.Resolve<MappingManager>();
        public static ObjectPool<StringBuilder> ObjectPool => Canister.Builder.Bootstrapper.Resolve<ObjectPool<StringBuilder>>();
        public static SchemaManager SchemaManager => Canister.Builder.Bootstrapper.Resolve<SchemaManager>();
        public static Sherlock Sherlock => Canister.Builder.Bootstrapper.Resolve<Sherlock>();
        public static ILogger<SQLHelper> SQLHelperLogger => Canister.Builder.Bootstrapper.Resolve<ILogger<SQLHelper>>();
        protected static string DatabaseName = "TestDatabase";
        protected static string MasterString = "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false";
        private static readonly object LockObject = new object();

        public static ILogger<T> GetLogger<T>()
        {
            return Canister.Builder.Bootstrapper.Resolve<ILogger<T>>();
        }

        public void Dispose()
        {
            //try
            //{
            //    Task.Run(async () => await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
            //        .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
            //        .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
            //        .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
            //        .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
            //        .ExecuteScalarAsync<int>().ConfigureAwait(false)).GetAwaiter().GetResult();
            //}
            //catch { }
        }
    }
}