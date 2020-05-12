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
using System.Collections.Generic;
using System.Data;
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
            _ = Canister.Builder.Bootstrapper.Resolve<ISession>();
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
            Assert.Equal(15, Results2.Count());
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
            Assert.NotEmpty(Results2);
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
            await DeleteData().ConfigureAwait(false);
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
            _ = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Result = DbContext<ManyToOneManyProperties>.CreateQuery().First();
            Assert.NotNull(Result.ManyToOneClass);
            Assert.Single(Result.ManyToOneClass);
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
            Assert.Equal(2, Results.Max(x => x.ManyToOneClass.Count));
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
                var Result = TestObject.Save(x.ManyToOneClass.ToArray()).ExecuteAsync().GetAwaiter().GetResult();
            }).ToArray();
            await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false);
            Results = await TestObject.ExecuteAsync<ManyToOneManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToOneManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.Equal(2, Results.Max(x => x.ManyToOneClass.Count()));
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
            await DeleteData().ConfigureAwait(false);
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

        private static async Task DeleteData()
        {
            await Helper
                            .CreateBatch()
                            .AddQuery(CommandType.Text, "DELETE FROM ManyToOneManyProperties_")
                            .AddQuery(CommandType.Text, "DELETE FROM ManyToOneManyCascadeProperties_")
                            .ExecuteScalarAsync<int>().ConfigureAwait(false);
        }

        private async Task SetupDataAsync()
        {
            var TestObject = new SchemaManager(MappingManager, Configuration, Logger, DataModeler, Sherlock, Helper);
            var Session = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, "DELETE FROM ManyToOneManyProperties_")
                .AddQuery(CommandType.Text, "DELETE FROM ManyToOneManyCascadeProperties_")
                .ExecuteScalarAsync<int>().ConfigureAwait(false);
            var InitialData = new ManyToOneManyCascadeProperties[]
            {
                new ManyToOneManyCascadeProperties
                {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new ManyToOneOneProperties
                        {
                            BoolValue=true
                        }
                    }
                },
                new ManyToOneManyCascadeProperties
                {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new ManyToOneOneProperties
                        {
                            BoolValue=true
                        }
                    }
                },
                new ManyToOneManyCascadeProperties
                {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new ManyToOneOneProperties
                        {
                            BoolValue=true
                        }
                    }
                },
            };
            var InitialData2 = new ManyToOneManyProperties[]
            {
                new ManyToOneManyProperties
                {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new ManyToOneOneProperties
                        {
                            BoolValue=true
                        }
                    }
                },
                new ManyToOneManyProperties
                {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new ManyToOneOneProperties
                        {
                            BoolValue=true
                        }
                    }
                },
                new ManyToOneManyProperties
                {
                    BoolValue=true,
                    ManyToOneClass =new List<ManyToOneOneProperties>
                    {
                        new ManyToOneOneProperties
                        {
                            BoolValue=true
                        }
                    }
                },
            };

            await Session.Save(InitialData2.SelectMany(x => x.ManyToOneClass).ToArray()).ExecuteAsync().ConfigureAwait(false);
            await Session.Save(InitialData).ExecuteAsync().ConfigureAwait(false);
            await Session.Save(InitialData2).ExecuteAsync().ConfigureAwait(false);
        }
    }
}