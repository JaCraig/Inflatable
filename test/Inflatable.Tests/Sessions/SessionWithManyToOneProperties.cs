using BigBook;
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.ManyToOneProperties;
using Inflatable.Tests.TestDatabases.ManyToOneProperties.Mappings;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    public class SessionWithManyToOneProperties : TestingFixture
    {
        public SessionWithManyToOneProperties()
        {
            InternalMappingManager = new MappingManager(new IMapping[] {
                new ManyToOneManyPropertiesMapping(),
                new ManyToOneOnePropertiesMapping(),
                new ManyToOneManyCascadePropertiesMapping()
            },
            new IDatabase[]{
                new TestDatabaseMapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
            Canister.Builder.Bootstrapper.Resolve<ILogger>(),
            ObjectPool);
            InternalSchemaManager = new SchemaManager(InternalMappingManager, Configuration, Logger, DataModeler, Sherlock, Helper);

            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool);
            InternalQueryProviderManager = new QueryProviderManager(new[] { TempQueryProvider }, Logger);

            CacheManager = Canister.Builder.Bootstrapper.Resolve<BigBook.Caching.Manager>();
            CacheManager.Cache().Clear();
        }

        public static Aspectus.Aspectus AOPManager => Canister.Builder.Bootstrapper.Resolve<Aspectus.Aspectus>();
        public BigBook.Caching.Manager CacheManager { get; set; }
        public MappingManager InternalMappingManager { get; set; }

        public QueryProviderManager InternalQueryProviderManager { get; set; }
        public SchemaManager InternalSchemaManager { get; set; }

        [Fact]
        public async Task AllNoParametersWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Results = DbContext<ManyToOneManyCascadeProperties>.CreateQuery().ToArray();
            Assert.Equal(3, Results.Length);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT TOP 2 ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
            var Results2 = await TestObject.ExecuteAsync<ManyToOneOneProperties>("SELECT ID_ as [ID] FROM ManyToOneOneProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(3, Results2.Count());
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabaseAndCascade()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT TOP 2 ID_ as [ID] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT ID_ as [ID] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
            var Results2 = await TestObject.ExecuteAsync<ManyToOneOneProperties>("SELECT ID_ as [ID] FROM ManyToOneOneProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results2);
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT TOP 1 ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(2, Results.Count());
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            try
            {
                await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false);
            }
            catch { }
            _ = new SchemaManager(MappingManager, Configuration, Logger, DataModeler, Sherlock, Helper);
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            var Result = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT TOP 1 ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Empty(Results);
        }

        [Fact]
        public async Task InsertHundredsOfObjectsWithCascade()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            for (int x = 0; x < 2000; ++x)
            {
                var Result1 = new ManyToOneManyCascadeProperties
                {
                    BoolValue = false
                };
                Result1.ManyToOneClass.Add(new ManyToOneOneProperties
                {
                    BoolValue = true
                });
                TestObject.Save(Result1);
            }
            await TestObject.ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(2003, Results.Count());
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Result1 = new ManyToOneManyCascadeProperties
            {
                BoolValue = false
            };
            Result1.ManyToOneClass.Add(new ManyToOneOneProperties
            {
                BoolValue = true
            });
            var Result2 = new ManyToOneManyCascadeProperties
            {
                BoolValue = false
            };
            Result2.ManyToOneClass.Add(new ManyToOneOneProperties
            {
                BoolValue = true
            });
            var Result3 = new ManyToOneManyCascadeProperties
            {
                BoolValue = false
            };
            Result3.ManyToOneClass.Add(new ManyToOneOneProperties
            {
                BoolValue = true
            });
            await TestObject.Save(Result1, Result2, Result3).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(6, Results.Count());
            Assert.Contains(Results, x => x.ID == Result1.ID
            && !x.BoolValue
            && x.ManyToOneClass.Count == 1
            && x.ManyToOneClass.Any(y => y.BoolValue));
            Assert.Contains(Results, x => x.ID == Result2.ID
            && !x.BoolValue
            && x.ManyToOneClass.Count == 1
            && x.ManyToOneClass.Any(y => y.BoolValue));
            Assert.Contains(Results, x => x.ID == Result3.ID
            && !x.BoolValue
            && x.ManyToOneClass.Count == 1
            && x.ManyToOneClass.Any(y => y.BoolValue));
        }

        [Fact]
        public async Task LoadMapPropertyWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Result = DbContext<ManyToOneManyProperties>.CreateQuery().Where(x => x.ID == 1).First();
            Assert.NotNull(Result.ManyToOneClass);
            Assert.Equal(1, Result.ManyToOneClass[0].ID);
        }

        [Fact]
        public async Task UpdateMultipleCascadeWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToOneClass.Add(new ManyToOneOneProperties
                {
                    BoolValue = true
                });
            }).ToArray();
            await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false);
            Results = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.Equal(6, Results.Max(x => x.ManyToOneClass.Max(y => y.ID)));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToOneClass.Add(new ManyToOneOneProperties
                {
                    BoolValue = true
                });
                var Result = TestObject.Save(x.ManyToOneClass).ExecuteAsync().GetAwaiter().GetResult();
            }).ToArray();
            await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false);
            Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.Equal(3, Results.Max(x => x.ManyToOneClass.Max(y => y.ID)));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToOneClass.Clear();
            }).ToArray();
            Assert.Equal(3, await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false));
            Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.ManyToOneClass.All(y => y.BoolValue)));
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            Assert.Equal(0, await TestObject.Save<ManyToOneManyProperties>(null).ExecuteAsync().ConfigureAwait(false));
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            try
            {
                await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false);
            }
            catch { }
            _ = new SchemaManager(MappingManager, Configuration, Logger, DataModeler, Sherlock, Helper);
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            var Result = new ManyToOneManyProperties
            {
                BoolValue = false
            };
            Result.ManyToOneClass.Add(new ManyToOneOneProperties
            {
                BoolValue = true
            });
            await TestObject.Save(Result).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
        }

        private async Task SetupDataAsync()
        {
            try
            {
                await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false);
            }
            catch { }
            var TestObject = new SchemaManager(MappingManager, Configuration, Logger, DataModeler, Sherlock, Helper);
            await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, @"INSERT INTO [dbo].[ManyToOneManyProperties_]([BoolValue_]) VALUES (1);
INSERT INTO [dbo].[ManyToOneManyCascadeProperties_]([BoolValue_]) VALUES (1);
INSERT INTO [dbo].[ManyToOneManyProperties_]([BoolValue_]) VALUES (1);
INSERT INTO [dbo].[ManyToOneManyCascadeProperties_]([BoolValue_]) VALUES (1);
INSERT INTO [dbo].[ManyToOneManyProperties_]([BoolValue_]) VALUES (1);
INSERT INTO [dbo].[ManyToOneManyCascadeProperties_]([BoolValue_]) VALUES (1);
INSERT INTO [dbo].[ManyToOneOneProperties_]([BoolValue_],[ManyToOneManyCascadeProperties_ID_],[ManyToOneManyProperties_ID_]) VALUES (1,1,1);
INSERT INTO [dbo].[ManyToOneOneProperties_]([BoolValue_],[ManyToOneManyCascadeProperties_ID_],[ManyToOneManyProperties_ID_]) VALUES (1,2,2);
INSERT INTO [dbo].[ManyToOneOneProperties_]([BoolValue_],[ManyToOneManyCascadeProperties_ID_],[ManyToOneManyProperties_ID_]) VALUES (1,3,3);")
                .ExecuteScalarAsync<int>().ConfigureAwait(false);
        }
    }
}