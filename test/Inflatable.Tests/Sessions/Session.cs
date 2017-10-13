using BigBook;
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
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
    public class SessionTests : TestingFixture
    {
        public SessionTests()
        {
            InternalMappingManager = new MappingManager(new[] {
                new AllReferencesAndIDMappingWithDatabase()
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
        public void Creation()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            Assert.NotNull(TestObject);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT TOP 2 ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Single(Results);
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT TOP 1 ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Equal(2, Results.Count());
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            var Result = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT TOP 1 ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Empty(Results);
        }

        [Fact]
        public async Task Execute()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_ WHERE ID_=@0",
                CommandType.Text,
                "Default",
                2);
            Assert.Single(Result);
            Assert.Equal(2, Result.First().ID);
        }

        [Fact]
        public async Task ExecuteDynamic()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = await TestObject.ExecuteDynamicAsync("SELECT * FROM AllReferencesAndID_ WHERE ID_=@0",
                CommandType.Text,
                "Default",
                2);
            Assert.Single(Result);
            Assert.Equal(2, (long)Result.First().ID_);
        }

        [Fact]
        public async Task ExecuteScalar()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = await TestObject.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM AllReferencesAndID_",
                CommandType.Text,
                "Default");
            Assert.Equal(3, Result);
        }

        [Fact]
        public async Task InsertMultipleObjects()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result1 = new AllReferencesAndID
            {
                BoolValue = false,
                ByteArrayValue = new byte[] { 1, 2, 3, 4 },
                ByteValue = 34,
                CharValue = 'a',
                DateTimeValue = new DateTime(2000, 1, 1)
            };
            var Result2 = new AllReferencesAndID
            {
                BoolValue = false,
                ByteArrayValue = new byte[] { 5, 6, 7, 8 },
                ByteValue = 34,
                CharValue = 'b',
                DateTimeValue = new DateTime(2000, 1, 1)
            };
            var Result3 = new AllReferencesAndID
            {
                BoolValue = false,
                ByteArrayValue = new byte[] { 9, 10, 11, 12 },
                ByteValue = 34,
                CharValue = 'c',
                DateTimeValue = new DateTime(2000, 1, 1)
            };
            await TestObject.Save(Result1, Result2, Result3).ExecuteAsync();
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID], BoolValue_ as [BoolValue], ByteValue_ as [ByteValue], CharValue_ as [CharValue], DateTimeValue_ as [DateTimeValue] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Equal(6, Results.Count());
            Assert.Contains(Results, x => x.ID == Result1.ID
            && !x.BoolValue
            && x.ByteValue == 34
            && x.CharValue == 'a'
            && x.DateTimeValue == new DateTime(2000, 1, 1));
            Assert.Contains(Results, x => x.ID == Result2.ID
            && !x.BoolValue
            && x.ByteValue == 34
            && x.CharValue == 'b'
            && x.DateTimeValue == new DateTime(2000, 1, 1));
            Assert.Contains(Results, x => x.ID == Result3.ID
            && !x.BoolValue
            && x.ByteValue == 34
            && x.CharValue == 'c'
            && x.DateTimeValue == new DateTime(2000, 1, 1));
        }

        [Fact]
        public async Task InsertSingleObject()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = new AllReferencesAndID
            {
                BoolValue = false,
                ByteArrayValue = new byte[] { 1, 2, 3, 4 },
                ByteValue = 34,
                CharValue = 'a',
                DateTimeValue = new DateTime(2000, 1, 1)
            };
            await TestObject.Save(Result).ExecuteAsync();
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID], BoolValue_ as [BoolValue], ByteValue_ as [ByteValue], CharValue_ as [CharValue], DateTimeValue_ as [DateTimeValue] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Equal(4, Results.Count());
            Assert.Contains(Results, x => x.ID == Result.ID
            && !x.BoolValue
            && x.ByteValue == 34
            && x.CharValue == 'a'
            && x.DateTimeValue == new DateTime(2000, 1, 1));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID],CharValue_ as [CharValue] FROM AllReferencesAndID_", CommandType.Text, "Default");
            var UpdatedResults = Results.ForEach(x => { x.CharValue = 'p'; }).ToArray();
            Assert.Equal(3, await TestObject.Save(UpdatedResults).ExecuteAsync());
            Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID],CharValue_ as [CharValue] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.True(Results.All(x => x.CharValue == 'p'));
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            Assert.Equal(0, await TestObject.Save<AllReferencesAndID>(null).ExecuteAsync());
        }

        [Fact]
        public async Task UpdateWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = (await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT TOP 1 ID_ as [ID],CharValue_ as [CharValue] FROM AllReferencesAndID_", CommandType.Text, "Default")).First();
            Result.CharValue = 'p';
            Assert.Equal(1, await TestObject.Save(Result).ExecuteAsync());
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID],CharValue_ as [CharValue] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Contains(Results, x => x.CharValue == 'p');
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            var Result = new AllReferencesAndID()
            {
                CharValue = 'p'
            };
            await TestObject.Save(Result).ExecuteAsync();
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Single(Results);
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