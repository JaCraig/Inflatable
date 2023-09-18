using Data.Modeler;
using Holmes;
using Inflatable.ClassMapper;
using Inflatable.Schema;
using Inflatable.Tests.Fixtures;
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
        protected TestingFixture(SetupFixture setupFixture)
        {
            SetupFixture = setupFixture;
            setupFixture.InitProvider();
        }

        protected static readonly object TestRunLock = new object();
        protected static string DatabaseName = "TestDatabase";
        protected static string MasterString = "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True";
        public IConfiguration Configuration => Resolve<IConfigurationRoot>();
        public DataModeler DataModeler => Resolve<DataModeler>();
        public SQLHelper Helper => Resolve<SQLHelper>();
        public MappingManager MappingManager => Resolve<MappingManager>();
        public ObjectPool<StringBuilder> ObjectPool => Resolve<ObjectPool<StringBuilder>>();
        public SchemaManager SchemaManager => Resolve<SchemaManager>();
        public SetupFixture SetupFixture { get; }
        public Sherlock Sherlock => Resolve<Sherlock>();

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

        public ILogger<T>? GetLogger<T>()
        {
            try
            {
                return Resolve<ILogger<T>>();
            }
            catch { }
            return null;
        }

        public T Resolve<T>()
             where T : class
        {
            try
            {
                return SetupFixture.Resolve<T>();
            }
            catch { }
            return default;
        }
    }
}