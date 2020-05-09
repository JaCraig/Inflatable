using BigBook;
using Data.Modeler;
using Holmes;
using Inflatable.ClassMapper;
using Inflatable.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using SQLHelperDB;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.BaseClasses
{
    [Collection("Test collection")]
    public abstract class TestingFixture : IDisposable
    {
        public TestingFixture()
        {
            var TestObject = new SchemaManager(MappingManager, Configuration, Logger, DataModeler, Sherlock, Helper);
        }

        protected static string DatabaseName = "TestDatabase";
        protected static string MasterString = "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false";
        public static IConfiguration Configuration => Canister.Builder.Bootstrapper.Resolve<IConfigurationRoot>();
        public static DataModeler DataModeler => Canister.Builder.Bootstrapper.Resolve<DataModeler>();
        public static DynamoFactory DynamoFactory => Canister.Builder.Bootstrapper.Resolve<DynamoFactory>();
        public static SQLHelper Helper => Canister.Builder.Bootstrapper.Resolve<SQLHelper>();
        public static ILogger Logger => Canister.Builder.Bootstrapper.Resolve<ILogger>();
        public static MappingManager MappingManager => Canister.Builder.Bootstrapper.Resolve<MappingManager>();
        public static ObjectPool<StringBuilder> ObjectPool => Canister.Builder.Bootstrapper.Resolve<ObjectPool<StringBuilder>>();
        public static Sherlock Sherlock => Canister.Builder.Bootstrapper.Resolve<Sherlock>();

        public void Dispose()
        {
            try
            {
                Task.Run(async () => await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
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