using BigBook;
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.ManyToManyProperties;
using Inflatable.Tests.TestDatabases.ManyToManyProperties.Mappings;
using Serilog;
using SQLHelperDB;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    public class SessionWithManyToManyPropertiesSelfReferencing : TestingFixture
    {
        public SessionWithManyToManyPropertiesSelfReferencing()
        {
            InternalMappingManager = new MappingManager(new IMapping[] {
                new ManyToManyPropertySelfReferencingMapping()
            },
            new IDatabase[]{
                new TestDatabaseMapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration) }, Logger),
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
            InternalSchemaManager = new SchemaManager(InternalMappingManager, Configuration, Logger);

            var TempQueryProvider = new SQLServerQueryProvider(Configuration);
            InternalQueryProviderManager = new QueryProviderManager(new[] { TempQueryProvider }, Logger);

            CacheManager = Canister.Builder.Bootstrapper.Resolve<BigBook.Caching.Manager>();
            CacheManager.Cache().Clear();
        }

        public Aspectus.Aspectus AOPManager => Canister.Builder.Bootstrapper.Resolve<Aspectus.Aspectus>();
        public BigBook.Caching.Manager CacheManager { get; set; }
        public MappingManager InternalMappingManager { get; set; }

        public QueryProviderManager InternalQueryProviderManager { get; set; }
        public SchemaManager InternalSchemaManager { get; set; }

        [Fact]
        public void AllNoParametersWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Results = DbContext<ManyToManyPropertySelfReferencing>.CreateQuery().ToArray();
            Assert.Equal(6, Results.Length);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(2, Results.Count());
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabaseAndCascade()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(2, Results.Count());
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(4, Results.Count());
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            var Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Empty(Results);
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result1 = new ManyToManyPropertySelfReferencing
            {
                BoolValue = false
            };
            Result1.Children.Add(new ManyToManyPropertySelfReferencing
            {
                BoolValue = true
            });
            var Result2 = new ManyToManyPropertySelfReferencing
            {
                BoolValue = false
            };
            Result2.Children.Add(new ManyToManyPropertySelfReferencing
            {
                BoolValue = true
            });
            var Result3 = new ManyToManyPropertySelfReferencing
            {
                BoolValue = false
            };
            Result3.Children.Add(new ManyToManyPropertySelfReferencing
            {
                BoolValue = true
            });
            await TestObject.Save(Result1, Result2, Result3).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(12, Results.Count());
            Assert.Contains(Results, x => x.ID == Result1.ID
            && !x.BoolValue
            && x.Children.Count == 1
            && x.Children.All(y => y.BoolValue));
            Assert.Contains(Results, x => x.ID == Result2.ID
            && !x.BoolValue
            && x.Children.Count == 1
            && x.Children.All(y => y.BoolValue));
            Assert.Contains(Results, x => x.ID == Result3.ID
            && !x.BoolValue
            && x.Children.Count == 1
            && x.Children.All(y => y.BoolValue));
        }

        [Fact]
        public void LoadMapPropertyWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = DbContext<ManyToManyPropertySelfReferencing>.CreateQuery().Where(x => x.ID == 1).First();
            Assert.NotNull(Result.Children);
            Assert.Equal(4, Result.Children[0].ID);
        }

        [Fact]
        public async Task UpdateMultipleCascadeWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.Children.ForEach(y => y.BoolValue = false);
                x.Children.Add(new ManyToManyPropertySelfReferencing
                {
                    BoolValue = false
                });
            }).ToArray();
            await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false);
            Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Results = Results.Where(x => x.Children.Count > 0);
            Assert.Equal(9, Results.Max(x => x.Children.Max(y => y.ID)));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.Children.Add(new ManyToManyPropertySelfReferencing
                {
                    BoolValue = true
                });
                var Result = TestObject.Save(x.Children).ExecuteAsync().GetAwaiter().GetResult();
            }).ToArray();
            await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false);
            Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.False(Results.All(x => !x.BoolValue));
            Results = Results.Where(x => x.Children.Count > 0);
            Assert.Equal(9, Results.Max(x => x.Children.Max(y => y.ID)));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.Children.Clear();
            }).ToArray();
            Assert.Equal(9, await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false));
            Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.Children.Count == 0));
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            Assert.Equal(0, await TestObject.Save<ManyToManyPropertySelfReferencing>(null).ExecuteAsync().ConfigureAwait(false));
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            var Result = new ManyToManyPropertySelfReferencing
            {
                BoolValue = false
            };
            Result.Children.Add(new ManyToManyPropertySelfReferencing
            {
                BoolValue = true
            });
            await TestObject.Save(Result).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(2, Results.Count());
        }

        private void SetupData()
        {
            new SQLHelper(Configuration, SqlClientFactory.Instance)
                .CreateBatch()
                .AddQuery(@"INSERT INTO [dbo].[ManyToManyPropertySelfReferencing_]([BoolValue_]) VALUES (1)
INSERT INTO [dbo].[ManyToManyPropertySelfReferencing_]([BoolValue_]) VALUES (1)
INSERT INTO [dbo].[ManyToManyPropertySelfReferencing_]([BoolValue_]) VALUES (1)", CommandType.Text)
                .AddQuery(@"INSERT INTO [dbo].[ManyToManyPropertySelfReferencing_]([BoolValue_]) VALUES (1)
INSERT INTO [dbo].[ManyToManyPropertySelfReferencing_]([BoolValue_]) VALUES (1)
INSERT INTO [dbo].[ManyToManyPropertySelfReferencing_]([BoolValue_]) VALUES (1)", CommandType.Text)
                .AddQuery(@"INSERT INTO [dbo].[Parent_Child]([Parent_ManyToManyPropertySelfReferencing_ID_],[ManyToManyPropertySelfReferencing_ID_]) VALUES (1,4)
INSERT INTO [dbo].[Parent_Child]([Parent_ManyToManyPropertySelfReferencing_ID_],[ManyToManyPropertySelfReferencing_ID_]) VALUES (2,5)
INSERT INTO [dbo].[Parent_Child]([Parent_ManyToManyPropertySelfReferencing_ID_],[ManyToManyPropertySelfReferencing_ID_]) VALUES (3,6)", CommandType.Text)
                .ExecuteScalar<int>();
        }
    }
}