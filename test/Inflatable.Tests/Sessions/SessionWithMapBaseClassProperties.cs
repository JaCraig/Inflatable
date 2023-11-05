using BigBook;
using DragonHoard.Core;
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.Fixtures;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.MapProperties;
using Inflatable.Tests.TestDatabases.MapProperties.Mappings;
using Inflatable.Tests.TestDatabases.SimpleTestWithDatabase;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    [Collection("Test collection")]
    public class SessionWithMapBaseClassProperties : TestingFixture
    {
        public SessionWithMapBaseClassProperties(SetupFixture setupFixture)
            : base(setupFixture)
        {
            InternalMappingManager = new MappingManager(new IMapping[] {
                new AllReferencesAndIDMappingWithDatabase(),
                new MapPropertiesWithBaseClassesMapping(),
                new MapProperty2Mapping(),
                new MapProperty1Mapping(),
                new MapPropertyBaseClassMapping(),
                new IMapPropertyInterfaceMapping()
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
            MapPropertiesWithBaseClasses[] Results = DbContext<MapPropertiesWithBaseClasses>.CreateQuery().ToArray();
            Assert.Equal(3, Results.Length);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            System.Collections.Generic.IEnumerable<MapPropertiesWithBaseClasses> Result = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT TOP 2 ID_ as [ID] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            System.Collections.Generic.IEnumerable<MapPropertiesWithBaseClasses> Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            Assert.Single(Results);
            System.Collections.Generic.IEnumerable<IMapPropertyInterface> Results2 = await TestObject.ExecuteAsync<IMapPropertyInterface>("SELECT ID_ as [ID] FROM IMapPropertyInterface_", CommandType.Text, "Default");
            Assert.NotEmpty(Results2);
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            await DeleteData();
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession TestObject = Resolve<ISession>();
            System.Collections.Generic.IEnumerable<MapPropertiesWithBaseClasses> Result = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT TOP 1 ID_ as [ID] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            System.Collections.Generic.IEnumerable<MapPropertiesWithBaseClasses> Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            Assert.Empty(Results);
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            var Result1 = new MapPropertiesWithBaseClasses
            {
                BoolValue = false,
                MappedClass = new MapProperty1
                {
                    BaseValue1 = 1,
                    ChildValue1 = 1
                }
            };
            var Result2 = new MapPropertiesWithBaseClasses
            {
                BoolValue = true,
                MappedClass = new MapProperty2
                {
                    BaseValue1 = 2,
                    ChildValue2 = 2
                }
            };
            var Result3 = new MapPropertiesWithBaseClasses
            {
                BoolValue = false,
                MappedClass = new MapProperty1
                {
                    BaseValue1 = 3,
                    ChildValue1 = 3
                }
            };
            await TestObject.Save(Result1, Result2, Result3).ExecuteAsync();
            System.Collections.Generic.IEnumerable<MapPropertiesWithBaseClasses> Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            Assert.Equal(6, Results.Count());
            Assert.Contains(Results, x => x.ID == Result1.ID
            && !x.BoolValue
            && x.MappedClass.BaseValue1 == 1
            && ((MapProperty1)x.MappedClass).ChildValue1 == 1);
            Assert.Contains(Results, x => x.ID == Result2.ID
            && x.BoolValue
            && x.MappedClass.BaseValue1 == 2
            && ((MapProperty2)x.MappedClass).ChildValue2 == 2);
            Assert.Contains(Results, x => x.ID == Result3.ID
            && !x.BoolValue
            && x.MappedClass.BaseValue1 == 3
            && ((MapProperty1)x.MappedClass).ChildValue1 == 3);
        }

        [Fact]
        public async Task LoadMapPropertyWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            MapPropertiesWithBaseClasses Result = DbContext<MapPropertiesWithBaseClasses>.CreateQuery().Where(x => x.ID == 1).First();
            Assert.NotNull(Result.MappedClass);
            Assert.Equal(1, Result.MappedClass.ID);
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            System.Collections.Generic.IEnumerable<MapPropertiesWithBaseClasses> Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            MapPropertiesWithBaseClasses[] UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.MappedClass = new MapProperty1
                {
                    ChildValue1 = 11,
                    BaseValue1 = 10
                };
            }).ToArray();
            await TestObject.Save(UpdatedResults).ExecuteAsync();
            Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.MappedClass.ID > 3));
            Assert.True(Results.All(x => x.MappedClass is MapProperty1));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            System.Collections.Generic.IEnumerable<MapPropertiesWithBaseClasses> Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            MapPropertiesWithBaseClasses[] UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.MappedClass = null;
            }).ToArray();
            Assert.Equal(6, await TestObject.Save(UpdatedResults).ExecuteAsync());
            Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.MappedClass == null));
        }

        private async Task DeleteData()
        {
            await Helper
                            .CreateBatch()
                            .AddQuery(CommandType.Text, "DELETE FROM MapPropertiesWithBaseClasses_")
                            .AddQuery(CommandType.Text, "DELETE FROM MapProperty1_")
                            .ExecuteScalarAsync<int>().ConfigureAwait(false);
        }

        private async Task SetupDataAsync()
        {
            var TestObject = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession Session = Resolve<ISession>();
            await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, "DELETE FROM MapPropertiesWithBaseClasses_")
                .AddQuery(CommandType.Text, "DELETE FROM MapProperty1_")
                .ExecuteScalarAsync<int>().ConfigureAwait(false);
            var InitialData = new MapPropertiesWithBaseClasses[]
            {
                new MapPropertiesWithBaseClasses
                {
                    BoolValue=true,
                    MappedClass = new MapProperty1
                    {
                        BaseValue1=1,
                    ChildValue1=2
                    }
                },
                new MapPropertiesWithBaseClasses
                {
                    BoolValue=false,
                    MappedClass = new MapProperty1
                    {
                        BaseValue1=1,
                    ChildValue1=2
                    }
                },
                new MapPropertiesWithBaseClasses
                {
                    BoolValue=true,
                    MappedClass = new MapProperty1
                    {
                        BaseValue1=1,
                    ChildValue1=2
                    }
                },
            };

            await Session.Save(InitialData.Select(x => x.MappedClass).ToArray()).ExecuteAsync().ConfigureAwait(false);
            await Session.Save(InitialData).ExecuteAsync().ConfigureAwait(false);
        }
    }
}