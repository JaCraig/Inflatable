using BigBook;
using DragonHoard.Core;
using Inflatable.ClassMapper;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.ManyToManyProperties;
using Inflatable.Tests.TestDatabases.ManyToManyProperties.Mappings;
using Microsoft.Data.SqlClient;
using SQLHelperDB;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    [Collection("Test collection")]
    public class SessionWithManyToManyPropertiesSelfReferencing : TestingFixture
    {
        public SessionWithManyToManyPropertiesSelfReferencing(SetupFixture setupFixture)
            : base(setupFixture)
        {
            InternalMappingManager = new MappingManager([
                new ManyToManyPropertySelfReferencingMapping()
            ],
            [
                new TestDatabaseMapping()
            ],
            new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelper>())], GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>());
            InternalSchemaManager = new SchemaManager(InternalMappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());

            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelper>());
            InternalQueryProviderManager = new QueryProviderManager([TempQueryProvider], GetLogger<QueryProviderManager>());

            CacheManager = Resolve<Cache>();
            CacheManager.GetOrAddCache("Inflatable").Compact(1);
        }

        public Aspectus.Aspectus AOPManager => Resolve<Aspectus.Aspectus>();
        public Cache CacheManager { get; set; }
        public MappingManager InternalMappingManager { get; set; }

        public QueryProviderManager InternalQueryProviderManager { get; set; }
        public SchemaManager InternalSchemaManager { get; set; }

        [Fact]
        public async Task AllNoParametersWithDataInDatabase()
        {
            _ = Resolve<ISession>();

            await SetupDataAsync();
            ManyToManyPropertySelfReferencing[] Results = [.. DbContext<ManyToManyPropertySelfReferencing>.CreateQuery()];
            Assert.Equal(6, Results.Length);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();

            await SetupDataAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            Assert.Equal(4, Results.Count());
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabaseAndCascade()
        {
            ISession TestObject = Resolve<ISession>();

            await SetupDataAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            Assert.Equal(4, Results.Count());
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();

            await SetupDataAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            Assert.Equal(5, Results.Count());
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            try
            {
                _ = await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>();
            }
            catch { }
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession TestObject = Resolve<ISession>();

            IEnumerable<ManyToManyPropertySelfReferencing> Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            Assert.Empty(Results);
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            ISession TestObject = Resolve<ISession>();

            await SetupDataAsync();
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
            _ = await TestObject.Save(Result1, Result2, Result3).ExecuteAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
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
        public async Task LoadMapPropertyWithDataInDatabase()
        {
            _ = Resolve<ISession>();

            await SetupDataAsync();
            ManyToManyPropertySelfReferencing Result = DbContext<ManyToManyPropertySelfReferencing>.CreateQuery().Skip(1).Take(1).First();
            Assert.NotNull(Result.Children);
            _ = Assert.Single(Result.Children);
        }

        [Fact]
        public async Task UpdateMultipleCascadeWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();

            await SetupDataAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            ManyToManyPropertySelfReferencing[] UpdatedResults = [.. Results.ForEach(x =>
            {
                x.BoolValue = false;
                _ = x.Children.ForEach(y => y.BoolValue = false);
                x.Children.Add(new ManyToManyPropertySelfReferencing
                {
                    BoolValue = false
                });
            })];
            _ = await TestObject.Save(UpdatedResults).ExecuteAsync();
            Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Results = Results.Where(x => x.Children.Count > 0);
            Assert.Equal(6, Results.Count());
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();

            await SetupDataAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            ManyToManyPropertySelfReferencing[] UpdatedResults = [.. Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.Children.Add(new ManyToManyPropertySelfReferencing
                {
                    BoolValue = true
                });
                var Result = AsyncHelper.RunSync(async () => await TestObject.Save(x.Children).ExecuteAsync());
            })];
            _ = await TestObject.Save(UpdatedResults).ExecuteAsync();
            Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            Assert.False(Results.All(x => !x.BoolValue));
            Results = Results.Where(x => x.Children.Count > 0);
            Assert.Equal(6, Results.Count());
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            ISession TestObject = Resolve<ISession>();

            await SetupDataAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            ManyToManyPropertySelfReferencing[] UpdatedResults = [.. Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.Children.Clear();
            })];
            Assert.Equal(9, await TestObject.Save(UpdatedResults).ExecuteAsync());
            Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.Children.Count == 0));
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            Assert.Equal(0, await TestObject.Save<ManyToManyPropertySelfReferencing>(null).ExecuteAsync());
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            try
            {
                _ = await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>();
            }
            catch { }
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession TestObject = Resolve<ISession>();
            var Result = new ManyToManyPropertySelfReferencing
            {
                BoolValue = false
            };
            Result.Children.Add(new ManyToManyPropertySelfReferencing
            {
                BoolValue = true
            });
            _ = await TestObject.Save(Result).ExecuteAsync();
            IEnumerable<ManyToManyPropertySelfReferencing> Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default");
            Assert.Equal(2, Results.Count());
        }

        private async Task SetupDataAsync()
        {
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession Session = Resolve<ISession>();
            _ = await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, "DELETE FROM Parent_Child")
                .AddQuery(CommandType.Text, "DELETE FROM ManyToManyPropertySelfReferencing_")
                .ExecuteAsync()
                .ConfigureAwait(false);
            var InitialData = new ManyToManyPropertySelfReferencing[]
            {
                new() {
                    BoolValue=true,
                    Children=
                    [
                        new() {
                            BoolValue=true
                        }
                    ]
                },
                new() {
                    BoolValue=true,
                    Children=
                    [
                        new() {
                            BoolValue=true
                        }
                    ]
                },
                new() {
                    BoolValue=true,
                    Children=
                    [
                        new() {
                            BoolValue=true
                        }
                    ]
                }
            };
            _ = await Session.Save(InitialData).ExecuteAsync().ConfigureAwait(false);
        }
    }
}