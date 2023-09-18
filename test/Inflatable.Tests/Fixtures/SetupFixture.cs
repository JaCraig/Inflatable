using Inflatable.Schema;
using Inflatable.Sessions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SQLHelperDB;
using System;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Fixtures
{
    /// <summary>
    /// Setup collection
    /// </summary>
    /// <seealso cref="ICollectionFixture{SetupFixture}"/>
    [CollectionDefinition("Test collection")]
    public class SetupCollection : ICollectionFixture<SetupFixture>
    {
    }

    /// <summary>
    /// Setup fixture
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public class SetupFixture : IDisposable
    {
        public SetupFixture()
        {
            InitProvider();
            _ = SchemaManager;
        }

        private readonly object LockObject = new();
        public SQLHelper Helper => Resolve<SQLHelper>();
        public ServiceProvider Provider { get; set; }
        public SchemaManager SchemaManager => Resolve<SchemaManager>();

        public void Dispose()
        {
            try
            {
                Task.Run(async () => await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false)).GetAwaiter().GetResult();
            }
            catch { }
        }

        public void InitProvider()
        {
            if (Provider is null)
            {
                lock (LockObject)
                {
                    if (Provider is null)
                    {
                        var Services = new ServiceCollection();
                        Services.AddLogging(builder => builder.AddSerilog())
                            .AddCanisterModules();
                        Provider = Services.BuildServiceProvider();
                        Resolve<ISession>();
                    }
                }
            }
        }

        public T Resolve<T>()
             where T : class
        {
            try
            {
                T Result = Provider.GetService<T>();
                if (Result is ISession ResultSession)
                    ResultSession.ClearCache();
                return Result;
            }
            catch { }
            return default;
        }
    }
}