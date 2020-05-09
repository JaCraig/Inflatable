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
    public class SessionWithMapProperties : TestingFixture
    {
        public SessionWithMapProperties()
        {
            InternalMappingManager = new MappingManager(new IMapping[] {
                new AllReferencesAndIDMappingWithDatabase(),
                new MapPropertiesMapping(),
                new MappedPropertiesWithCascadeMapping(),
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
            _ = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            await SetupDataAsync().ConfigureAwait(false);
            var Results = DbContext<MapProperties>.CreateQuery().ToArray();
            Assert.Equal(3, Results.Length);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<MapProperties>("SELECT TOP 2 ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
            var Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(3, Results2.Count());
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabaseAndCascade()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT TOP 2 ID_ as [ID] FROM MapPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT ID_ as [ID] FROM MapPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
            var Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results2);
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<MapProperties>("SELECT TOP 1 ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
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
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            var Result = await TestObject.ExecuteAsync<MapProperties>("SELECT TOP 1 ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Empty(Results);
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            await SetupDataAsync().ConfigureAwait(false);
            var Result1 = new MapPropertiesWithCascade
            {
                BoolValue = false,
                MappedClass = new AllReferencesAndID
                {
                    ByteArrayValue = new byte[] { 1, 2, 3, 4 },
                    ByteValue = 34,
                    CharValue = 'a',
                    DateTimeValue = new DateTime(2000, 1, 1)
                }
            };
            var Result2 = new MapPropertiesWithCascade
            {
                BoolValue = false,
                MappedClass = new AllReferencesAndID
                {
                    ByteArrayValue = new byte[] { 5, 6, 7, 8 },
                    ByteValue = 34,
                    CharValue = 'b',
                    DateTimeValue = new DateTime(2000, 1, 1)
                }
            };
            var Result3 = new MapPropertiesWithCascade
            {
                BoolValue = false,
                MappedClass = new AllReferencesAndID
                {
                    ByteArrayValue = new byte[] { 9, 10, 11, 12 },
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                }
            };
            await TestObject.Save(Result1, Result2, Result3).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM MapPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(6, Results.Count());
            Assert.Contains(Results, x => x.ID == Result1.ID
            && !x.BoolValue
            && x.MappedClass.ByteValue == 34
            && x.MappedClass.CharValue == 'a'
            && x.MappedClass.DateTimeValue == new DateTime(2000, 1, 1));
            Assert.Contains(Results, x => x.ID == Result2.ID
            && !x.BoolValue
            && x.MappedClass.ByteValue == 34
            && x.MappedClass.CharValue == 'b'
            && x.MappedClass.DateTimeValue == new DateTime(2000, 1, 1));
            Assert.Contains(Results, x => x.ID == Result3.ID
            && !x.BoolValue
            && x.MappedClass.ByteValue == 34
            && x.MappedClass.CharValue == 'c'
            && x.MappedClass.DateTimeValue == new DateTime(2000, 1, 1));
        }

        [Fact]
        public async Task LoadMapPropertyWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            await SetupDataAsync().ConfigureAwait(false);
            var Result = DbContext<MapProperties>.CreateQuery().Where(x => x.ID == 1).First();
            Assert.NotNull(Result.MappedClass);
            Assert.Equal(1, Result.MappedClass.ID);
        }

        [Fact]
        public async Task UpdateMultipleCascadeWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.MappedClass = new AllReferencesAndID
                {
                    ByteArrayValue = new byte[] { 9, 10, 11, 12 },
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                };
            }).ToArray();
            await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false);
            Results = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.MappedClass.ID > 3));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.MappedClass = new AllReferencesAndID
                {
                    ByteArrayValue = new byte[] { 9, 10, 11, 12 },
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                };
                var Result = TestObject.Save(x.MappedClass).ExecuteAsync().Result;
            }).ToArray();
            Assert.Equal(6, await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false));
            Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.MappedClass.ID > 3));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.MappedClass = null;
            }).ToArray();
            Assert.Equal(6, await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false));
            Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.MappedClass == null));
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            await SetupDataAsync().ConfigureAwait(false);
            Assert.Equal(0, await TestObject.Save<MapProperties>(null).ExecuteAsync().ConfigureAwait(false));
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
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, Logger, CacheManager, DynamoFactory);
            var Result = new MapProperties
            {
                BoolValue = false,
                MappedClass = new AllReferencesAndID
                {
                    ByteArrayValue = new byte[] { 9, 10, 11, 12 },
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                }
            };
            await TestObject.Save(Result).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
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
            _ = new SchemaManager(MappingManager, Configuration, Logger, DataModeler, Sherlock, Helper);
            await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, @"INSERT INTO [dbo].[AllReferencesAndID_]
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
                .AddQuery(CommandType.Text, @"INSERT INTO [dbo].[AllReferencesAndID_]
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
                .AddQuery(CommandType.Text, @"INSERT INTO [dbo].[AllReferencesAndID_]
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
           .AddQuery(CommandType.Text, @"INSERT INTO [dbo].[MapProperties_]
           ([BoolValue_],
           [AllReferencesAndID_MappedClass_ID_])
     VALUES
           (1
           ,1)")
           .AddQuery(CommandType.Text, @"INSERT INTO [dbo].[MapProperties_]
           ([BoolValue_],
           [AllReferencesAndID_MappedClass_ID_])
     VALUES
           (0
           ,2)")
           .AddQuery(CommandType.Text, @"INSERT INTO [dbo].[MapProperties_]
           ([BoolValue_],
           [AllReferencesAndID_MappedClass_ID_])
     VALUES
           (1
           ,3)")

           .AddQuery(CommandType.Text, @"INSERT INTO [dbo].[MapPropertiesWithCascade_]
           ([BoolValue_],
           [AllReferencesAndID_MappedClass_ID_])
     VALUES
           (1
           ,1)")
           .AddQuery(CommandType.Text, @"INSERT INTO [dbo].[MapPropertiesWithCascade_]
           ([BoolValue_],
           [AllReferencesAndID_MappedClass_ID_])
     VALUES
           (0
           ,2)")
           .AddQuery(CommandType.Text, @"INSERT INTO [dbo].[MapPropertiesWithCascade_]
           ([BoolValue_],
           [AllReferencesAndID_MappedClass_ID_])
     VALUES
           (1
           ,3)")
                .ExecuteScalarAsync<int>().ConfigureAwait(false);
        }
    }
}