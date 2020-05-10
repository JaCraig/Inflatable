using FileCurator.Registration;
using Inflatable.Registration;
using Inflatable.Schema;
using Inflatable.Sessions;
using Microsoft.Extensions.DependencyInjection;
using SQLHelperDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TestFountain.Registration;
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
            if (Canister.Builder.Bootstrapper == null)
            {
                _ = Canister.Builder.CreateContainer(new List<ServiceDescriptor>())
                                                .AddAssembly(typeof(SetupFixture).Assembly)
                                                .RegisterInflatable()
                                                .RegisterFileCurator()
                                                .RegisterTestFountain()
                                                .Build();
                Canister.Builder.Bootstrapper.Resolve<ISession>();
            }
            var Temp = SchemaManager;
        }

        public static SQLHelper Helper => Canister.Builder.Bootstrapper.Resolve<SQLHelper>();
        public static SchemaManager SchemaManager => Canister.Builder.Bootstrapper.Resolve<SchemaManager>();

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