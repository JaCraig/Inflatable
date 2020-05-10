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
        public void Creation()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            Assert.NotNull(TestObject);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT TOP 2 ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT TOP 1 ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(2, Results.Count());
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            await DeleteDatabaseData().ConfigureAwait(false);
            _ = new SchemaManager(MappingManager, Configuration, Logger, DataModeler, Sherlock, Helper);
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            var Result = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT TOP 1 ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Empty(Results);
        }

        [Fact]
        public async Task Execute()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var TempResults = DbContext<AllReferencesAndID>.CreateQuery().ToList();
            var Result = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_ WHERE ID_=@0",
                CommandType.Text,
                "Default",
                TempResults[0].ID).ConfigureAwait(false);
            Assert.Single(Result);
            Assert.Equal(TempResults[0].ID, Result.First().ID);
        }

        [Fact]
        public async Task ExecuteDynamic()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var TempResults = DbContext<AllReferencesAndID>.CreateQuery().ToList();
            var Result = await TestObject.ExecuteDynamicAsync("SELECT * FROM AllReferencesAndID_ WHERE ID_=@0",
                CommandType.Text,
                "Default",
                TempResults[0].ID).ConfigureAwait(false);
            Assert.Single(Result);
            Assert.Equal(TempResults[0].ID, (long)Result.First().ID_);
        }

        [Fact]
        public async Task ExecuteScalar()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM AllReferencesAndID_",
                CommandType.Text,
                "Default").ConfigureAwait(false);
            Assert.Equal(3, Result);
        }

        [Fact(Skip = "Takes a bit")]
        public async Task InsertHundredsOfObjects()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            for (int x = 0; x < 1000; ++x)
            {
                var Result1 = new AllReferencesAndID
                {
                    BoolValue = false,
                    ByteArrayValue = new byte[] { 1, 2, 3, 4 },
                    ByteValue = 34,
                    CharValue = 'a',
                    DateTimeValue = new DateTime(2000, 1, 1)
                };
                TestObject.Save(Result1);
            }
            await TestObject.ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID], BoolValue_ as [BoolValue], ByteValue_ as [ByteValue], CharValue_ as [CharValue], DateTimeValue_ as [DateTimeValue] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(1003, Results.Count());
        }

        [Fact]
        public async Task InsertMultipleObjects()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
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
            await TestObject.Save(Result1, Result2, Result3).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID], BoolValue_ as [BoolValue], ByteValue_ as [ByteValue], CharValue_ as [CharValue], DateTimeValue_ as [DateTimeValue] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
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
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Result = new AllReferencesAndID
            {
                BoolValue = false,
                ByteArrayValue = new byte[] { 1, 2, 3, 4 },
                ByteValue = 34,
                CharValue = 'a',
                DateTimeValue = new DateTime(2000, 1, 1)
            };
            await TestObject.Save(Result).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID], BoolValue_ as [BoolValue], ByteValue_ as [ByteValue], CharValue_ as [CharValue], DateTimeValue_ as [DateTimeValue] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
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
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID],CharValue_ as [CharValue] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach<AllReferencesAndID>(x => x.CharValue = 'p').ToArray();
            await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false);
            Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID],CharValue_ as [CharValue] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(3, Results.Count());
            Assert.True(Results.All(x => x.CharValue == 'p'));
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            Assert.Equal(0, await TestObject.Save<AllReferencesAndID>(null).ExecuteAsync().ConfigureAwait(false));
        }

        [Fact]
        public async Task UpdateWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Result = (await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT TOP 1 ID_ as [ID],CharValue_ as [CharValue] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false)).First();
            Result.CharValue = 'p';
            await TestObject.Save(Result).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID],CharValue_ as [CharValue] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Contains(Results, x => x.CharValue == 'p');
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            await DeleteDatabaseData().ConfigureAwait(false);
            _ = new SchemaManager(MappingManager, Configuration, Logger, DataModeler, Sherlock, Helper);
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();

            var Result = new AllReferencesAndID()
            {
                CharValue = 'p'
            };
            await TestObject.Save(Result).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
        }

        /// <summary>
        /// Deletes the database data.
        /// </summary>
        private static async Task DeleteDatabaseData()
        {
            await Helper
                         .CreateBatch()
                         .AddQuery(CommandType.Text, "DELETE FROM AllReferencesAndID_")
                         .ExecuteAsync().ConfigureAwait(false);
        }

        private async Task SetupDataAsync()
        {
            _ = new SchemaManager(MappingManager, Configuration, Logger, DataModeler, Sherlock, Helper);
            var Session = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, "DELETE FROM AllReferencesAndID_")
                .AddQuery(CommandType.Text,
                @"INSERT INTO [dbo].[AllReferencesAndID_]
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
           ,1234)")
                .AddQuery(CommandType.Text,
                @"INSERT INTO [dbo].[AllReferencesAndID_]
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
           ,1234)")
                .AddQuery(CommandType.Text,
                @"INSERT INTO [dbo].[AllReferencesAndID_]
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
           ,1234)")
                .ExecuteScalarAsync<int>().ConfigureAwait(false);
        }
    }
}