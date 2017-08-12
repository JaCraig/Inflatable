using BigBook;
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.MapProperties;
using Inflatable.Tests.TestDatabases.MapProperties.Mappings;
using Inflatable.Tests.TestDatabases.SimpleTest;
using Inflatable.Tests.TestDatabases.SimpleTestWithDatabase;
using Serilog;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    public class SessionWithMapBaseClassProperties : TestingFixture
    {
        public SessionWithMapBaseClassProperties()
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
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager, CacheManager);
            SetupData();
            var Results = DbContext<MapPropertiesWithBaseClasses>.CreateQuery().ToArray();
            Assert.Equal(3, Results.Count());
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager, CacheManager);
            SetupData();
            var Result = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT TOP 2 ID_ as [ID] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            await TestObject.DeleteAsync(Result.ToArray());
            var Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            Assert.Equal(1, Results.Count());
            var Results2 = await TestObject.ExecuteAsync<IMapPropertyInterface>("SELECT ID_ as [ID] FROM IMapPropertyInterface_", CommandType.Text, "Default");
            Assert.Equal(1, Results2.Count());
        }
        
        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager, CacheManager);
            var Result = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT TOP 1 ID_ as [ID] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            await TestObject.DeleteAsync(Result.ToArray());
            var Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            Assert.Empty(Results);
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager, CacheManager);
            SetupData();
            var Result1 = new MapPropertiesWithBaseClasses
            {
                BoolValue = false,
                MappedClass = new MapProperty1
                {
                    BaseValue1=1,
                    ChildValue1=1
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
            await TestObject.InsertAsync(Result1, Result2, Result3);
            var Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            Assert.Equal(6, Results.Count());
            Assert.True(Results.Any(x => x.ID == Result1.ID
            && !x.BoolValue
            && x.MappedClass.BaseValue1 == 1
            && ((MapProperty1)x.MappedClass).ChildValue1 == 1));
            Assert.True(Results.Any(x => x.ID == Result2.ID
            && x.BoolValue
            && x.MappedClass.BaseValue1 == 2
            && ((MapProperty2)x.MappedClass).ChildValue2 == 2));
            Assert.True(Results.Any(x => x.ID == Result3.ID
            && !x.BoolValue
            && x.MappedClass.BaseValue1 == 3
            && ((MapProperty1)x.MappedClass).ChildValue1 == 3));
        }


        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager, CacheManager);
            SetupData();
            var Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.MappedClass = new MapProperty1
                {
                    ChildValue1=11,
                    BaseValue1=10
                };
            }).ToArray();
            Assert.Equal(3, await TestObject.UpdateAsync(UpdatedResults));
            Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.MappedClass.ID > 3));
            Assert.True(Results.All(x => (x.MappedClass as MapProperty1) != null));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager, CacheManager);
            SetupData();
            var Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.MappedClass = null;
            }).ToArray();
            Assert.Equal(3, await TestObject.UpdateAsync(UpdatedResults));
            Results = await TestObject.ExecuteAsync<MapPropertiesWithBaseClasses>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithBaseClasses_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.MappedClass == null));
        }
        
        [Fact]
        public void LoadMapPropertyWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager, CacheManager);
            SetupData();
            var Result = DbContext<MapPropertiesWithBaseClasses>.CreateQuery().Where(x => x.ID == 1).First();
            Assert.NotNull(Result.MappedClass);
            Assert.Equal(1, Result.MappedClass.ID);
        }

        private void SetupData()
        {
            new SQLHelper.SQLHelper(Configuration, SqlClientFactory.Instance)
                .CreateBatch()
                .AddQuery(@"INSERT INTO [dbo].[AllReferencesAndID_]
           ([BoolValue_]
           ,[ByteArrayValue_]
           ,[ByteValue_]
           ,[CharValue_]
           ,[DateTimeValue_]
           ,[DecimalValue_]
           ,[DoubleValue_]
           ,[FloatValue_]
           ,[GuidValue_]
           ,[IntValue_]
           ,[LongValue_]
           ,[SByteValue_]
           ,[ShortValue_]
           ,[StringValue1_]
           ,[StringValue2_]
           ,[TimeSpanValue_]
           ,[UIntValue_]
           ,[ULongValue_]
           ,[UShortValue_])
     VALUES
           (1
           ,1
           ,1
           ,'a'
           ,'1/1/2008'
           ,13.2
           ,423.12341234
           ,1243.1
           ,'ad0d39ad-6889-4ab3-965d-3d4042344ee6'
           ,12
           ,2
           ,1
           ,2
           ,'asdfvzxcv'
           ,'qwerertyizjgposgj'
           ,'January 1, 1900 00:00:00.100'
           ,12
           ,5342
           ,1234)", CommandType.Text)
                .AddQuery(@"INSERT INTO [dbo].[AllReferencesAndID_]
           ([BoolValue_]
           ,[ByteArrayValue_]
           ,[ByteValue_]
           ,[CharValue_]
           ,[DateTimeValue_]
           ,[DecimalValue_]
           ,[DoubleValue_]
           ,[FloatValue_]
           ,[GuidValue_]
           ,[IntValue_]
           ,[LongValue_]
           ,[SByteValue_]
           ,[ShortValue_]
           ,[StringValue1_]
           ,[StringValue2_]
           ,[TimeSpanValue_]
           ,[UIntValue_]
           ,[ULongValue_]
           ,[UShortValue_])
     VALUES
           (1
           ,1
           ,2
           ,'a'
           ,'1/1/2008'
           ,13.2
           ,423.12341234
           ,1243.1
           ,'ad0d39ad-6889-4ab3-965d-3d4042344ee6'
           ,13
           ,2
           ,1
           ,2
           ,'asdfvzxcv'
           ,'qwerertyizjgposgj'
           ,'January 1, 1900 00:00:00.100'
           ,12
           ,5342
           ,1234)", CommandType.Text)
                .AddQuery(@"INSERT INTO [dbo].[AllReferencesAndID_]
           ([BoolValue_]
           ,[ByteArrayValue_]
           ,[ByteValue_]
           ,[CharValue_]
           ,[DateTimeValue_]
           ,[DecimalValue_]
           ,[DoubleValue_]
           ,[FloatValue_]
           ,[GuidValue_]
           ,[IntValue_]
           ,[LongValue_]
           ,[SByteValue_]
           ,[ShortValue_]
           ,[StringValue1_]
           ,[StringValue2_]
           ,[TimeSpanValue_]
           ,[UIntValue_]
           ,[ULongValue_]
           ,[UShortValue_])
     VALUES
           (1
           ,1
           ,3
           ,'a'
           ,'1/1/2008'
           ,13.2
           ,423.12341234
           ,1243.1
           ,'ad0d39ad-6889-4ab3-965d-3d4042344ee6'
           ,14
           ,2
           ,1
           ,2
           ,'asdfvzxcv'
           ,'qwerertyizjgposgj'
           ,'January 1, 1900 00:00:00.100'
           ,12
           ,5342
           ,1234)", CommandType.Text)

                .ExecuteScalar<int>();
        }
    }
}