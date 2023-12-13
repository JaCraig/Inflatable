using BigBook;
using DragonHoard.Core;
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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    [Collection("Test collection")]
    public class SessionWithManyToOneProperties : TestingFixture
    {
        public SessionWithManyToOneProperties(SetupFixture setupFixture)
            : base(setupFixture)
        {
            InternalMappingManager = new MappingManager(new IMapping[] {
                new ManyToOneManyPropertiesMapping(),
                new ManyToOneOnePropertiesMapping(),
                new ManyToOneManyCascadePropertiesMapping()
            },
            new IDatabase[]{
                new TestDatabaseMapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>()) }, GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>());
            InternalSchemaManager = new SchemaManager(InternalMappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());

            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>());
            InternalQueryProviderManager = new QueryProviderManager(new[] { TempQueryProvider }, GetLogger<QueryProviderManager>());

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
            ManyToOneManyCascadeProperties[] Results = DbContext<ManyToOneManyCascadeProperties>.CreateQuery().ToArray();
            Assert.Equal(3, Results.Length);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            IEnumerable<ManyToOneManyProperties> Result = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT TOP 2 ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToOneManyProperties> Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default");
            _ = Assert.Single(Results);
            IEnumerable<ManyToOneOneProperties> Results2 = await TestObject.ExecuteAsync<ManyToOneOneProperties>("SELECT ID_ as [ID] FROM ManyToOneOneProperties_", CommandType.Text, "Default");
            Assert.Equal(15, Results2.Count());
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabaseAndCascade()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            IEnumerable<ManyToOneManyCascadeProperties> Result = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT TOP 2 ID_ as [ID] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToOneManyCascadeProperties> Results = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT ID_ as [ID] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default");
            _ = Assert.Single(Results);
            IEnumerable<ManyToOneOneProperties> Results2 = await TestObject.ExecuteAsync<ManyToOneOneProperties>("SELECT ID_ as [ID] FROM ManyToOneOneProperties_", CommandType.Text, "Default");
            Assert.NotEmpty(Results2);
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            IEnumerable<ManyToOneManyProperties> Result = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT TOP 1 ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToOneManyProperties> Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default");
            Assert.Equal(2, Results.Count());
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            await DeleteData();
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession TestObject = Resolve<ISession>();
            IEnumerable<ManyToOneManyProperties> Result = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT TOP 1 ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToOneManyProperties> Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default");
            Assert.Empty(Results);
        }

        [Fact]
        public async Task InsertHundredsOfObjectsWithCascade()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            for (var x = 0; x < 2000; ++x)
            {
                var Result1 = new ManyToOneManyCascadeProperties
                {
                    BoolValue = false
                };
                Result1.ManyToOneClass.Add(new ManyToOneOneProperties
                {
                    BoolValue = true
                });
                _ = TestObject.Save(Result1);
            }
            _ = await TestObject.ExecuteAsync();
            IEnumerable<ManyToOneManyCascadeProperties> Results = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default");
            Assert.Equal(2003, Results.Count());
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
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
            _ = await TestObject.Save(Result1, Result2, Result3).ExecuteAsync();
            IEnumerable<ManyToOneManyCascadeProperties> Results = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default");
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
            _ = Resolve<ISession>();
            await SetupDataAsync();
            ManyToOneManyProperties Result = DbContext<ManyToOneManyProperties>.CreateQuery().First();
            Assert.NotNull(Result.ManyToOneClass);
            _ = Assert.Single(Result.ManyToOneClass);
        }

        [Fact]
        public async Task UpdateMultipleCascadeWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            IEnumerable<ManyToOneManyCascadeProperties> Results = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default");
            ManyToOneManyCascadeProperties[] UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToOneClass.Add(new ManyToOneOneProperties
                {
                    BoolValue = true
                });
            }).ToArray();
            _ = await TestObject.Save(UpdatedResults).ExecuteAsync();
            Results = await TestObject.ExecuteAsync<ManyToOneManyCascadeProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyCascadeProperties_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.Equal(2, Results.Max(x => x.ManyToOneClass.Count));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            IEnumerable<ManyToOneManyProperties> Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyProperties_", CommandType.Text, "Default");
            ManyToOneManyProperties[] UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToOneClass.Add(new ManyToOneOneProperties
                {
                    BoolValue = true
                });
                var Result = AsyncHelper.RunSync(async () => await TestObject.Save(x.ManyToOneClass.ToArray()).ExecuteAsync());
            }).ToArray();
            _ = await TestObject.Save(UpdatedResults).ExecuteAsync();
            Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyProperties_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.Equal(2, Results.Max(x => x.ManyToOneClass.Count()));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            ISession TestObject = Resolve<ISession>();

            await SetupDataAsync();
            IEnumerable<ManyToOneManyProperties> Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyProperties_", CommandType.Text, "Default");
            ManyToOneManyProperties[] UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToOneClass.Clear();
            }).ToArray();
            Assert.Equal(3, await TestObject.Save(UpdatedResults).ExecuteAsync());
            Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyProperties_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.ManyToOneClass.All(y => y.BoolValue)));
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            Assert.Equal(0, await TestObject.Save<ManyToOneManyProperties>(null).ExecuteAsync());
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            await DeleteData();
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession TestObject = Resolve<ISession>();
            var Result = new ManyToOneManyProperties
            {
                BoolValue = false
            };
            Result.ManyToOneClass.Add(new ManyToOneOneProperties
            {
                BoolValue = true
            });
            _ = await TestObject.Save(Result).ExecuteAsync();
            IEnumerable<ManyToOneManyProperties> Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID] FROM ManyToOneManyProperties_", CommandType.Text, "Default");
            _ = Assert.Single(Results);
        }

        private async Task DeleteData()
        {
            _ = await Helper
                            .CreateBatch()
                            .AddQuery(CommandType.Text, "DELETE FROM ManyToOneManyProperties_")
                            .AddQuery(CommandType.Text, "DELETE FROM ManyToOneManyCascadeProperties_")
                            .ExecuteScalarAsync<int>().ConfigureAwait(false);
        }

        private async Task SetupDataAsync()
        {
            var TestObject = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession Session = Resolve<ISession>();
            _ = await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, "DELETE FROM ManyToOneManyProperties_")
                .AddQuery(CommandType.Text, "DELETE FROM ManyToOneManyCascadeProperties_")
                .ExecuteScalarAsync<int>().ConfigureAwait(false);
            var InitialData = new ManyToOneManyCascadeProperties[]
            {
                new() {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new() {
                            BoolValue=true
                        }
                    }
                },
                new() {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new() {
                            BoolValue=true
                        }
                    }
                },
                new() {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new() {
                            BoolValue=true
                        }
                    }
                },
            };
            var InitialData2 = new ManyToOneManyProperties[]
            {
                new() {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new() {
                            BoolValue=true
                        }
                    }
                },
                new() {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new() {
                            BoolValue=true
                        }
                    }
                },
                new() {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new() {
                            BoolValue=true
                        }
                    }
                },
            };

            _ = await Session.Save(InitialData2.SelectMany(x => x.ManyToOneClass).ToArray()).ExecuteAsync().ConfigureAwait(false);
            _ = await Session.Save(InitialData).ExecuteAsync().ConfigureAwait(false);
            _ = await Session.Save(InitialData2).ExecuteAsync().ConfigureAwait(false);
        }
    }
}