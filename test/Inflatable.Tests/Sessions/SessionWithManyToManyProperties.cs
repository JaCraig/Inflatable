﻿using BigBook;
using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.ManyToManyProperties;
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
    public class SessionWithManyToManyProperties : TestingFixture
    {
        public SessionWithManyToManyProperties()
        {
            InternalMappingManager = new MappingManager(new IMapping[] {
                new AllReferencesAndIDMappingWithDatabase(),
                new ManyToManyPropertiesWithCascadeMapping(),
                new ManyToManyPropertiesMapping(),
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
            var Results = DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToArray();
            Assert.Equal(3, Results.Count());
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            Assert.Equal(1, Results.Count());
            var Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Equal(3, Results2.Count());
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabaseAndCascade()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default");
            await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default");
            Assert.Equal(1, Results.Count());
            var Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Equal(1, Results2.Count());
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            Assert.Equal(2, Results.Count());
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            var Result = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            Assert.Empty(Results);
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result1 = new ManyToManyPropertiesWithCascade
            {
                BoolValue = false
            };
            Result1.ManyToManyClass.Add(new AllReferencesAndID
            {
                ByteArrayValue = new byte[] { 1, 2, 3, 4 },
                ByteValue = 34,
                CharValue = 'a',
                DateTimeValue = new DateTime(2000, 1, 1)
            });
            var Result2 = new ManyToManyPropertiesWithCascade
            {
                BoolValue = false
            };
            Result2.ManyToManyClass.Add(new AllReferencesAndID
            {
                ByteArrayValue = new byte[] { 5, 6, 7, 8 },
                ByteValue = 34,
                CharValue = 'b',
                DateTimeValue = new DateTime(2000, 1, 1)
            });
            var Result3 = new ManyToManyPropertiesWithCascade
            {
                BoolValue = false
            };
            Result3.ManyToManyClass.Add(new AllReferencesAndID
            {
                ByteArrayValue = new byte[] { 9, 10, 11, 12 },
                ByteValue = 34,
                CharValue = 'c',
                DateTimeValue = new DateTime(2000, 1, 1)
            });
            await TestObject.Save(Result1, Result2, Result3).ExecuteAsync();
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default");
            Assert.Equal(6, Results.Count());
            Assert.True(Results.Any(x => x.ID == Result1.ID
            && !x.BoolValue
            && x.ManyToManyClass.Count == 1
            && x.ManyToManyClass.Any(y => y.ByteValue == 34
            && y.CharValue == 'a'
            && y.DateTimeValue == new DateTime(2000, 1, 1))));
            Assert.True(Results.Any(x => x.ID == Result2.ID
            && !x.BoolValue
            && x.ManyToManyClass.Count == 1
            && x.ManyToManyClass.Any(y => y.ByteValue == 34
            && y.CharValue == 'b'
            && y.DateTimeValue == new DateTime(2000, 1, 1))));
            Assert.True(Results.Any(x => x.ID == Result3.ID
            && !x.BoolValue
            && x.ManyToManyClass.Count == 1
            && x.ManyToManyClass.Any(y => y.ByteValue == 34
            && y.CharValue == 'c'
            && y.DateTimeValue == new DateTime(2000, 1, 1))));
        }

        [Fact]
        public void LoadMapPropertyWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Result = DbContext<ManyToManyProperties>.CreateQuery().Where(x => x.ID == 1).First();
            Assert.NotNull(Result.ManyToManyClass);
            Assert.Equal(1, Result.ManyToManyClass[0].ID);
        }

        [Fact]
        public async Task UpdateMultipleCascadeWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default");
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToManyClass.Add(new AllReferencesAndID
                {
                    ByteArrayValue = new byte[] { 9, 10, 11, 12 },
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                });
            }).ToArray();
            await TestObject.Save(UpdatedResults).ExecuteAsync();
            Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.Equal(6, Results.Max(x => x.ManyToManyClass.Max(y => y.ID)));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default");
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToManyClass.Add(new AllReferencesAndID
                {
                    ByteArrayValue = new byte[] { 9, 10, 11, 12 },
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                });
                var Result = TestObject.Save(x.ManyToManyClass).ExecuteAsync().GetAwaiter().GetResult();
            }).ToArray();
            await Assert.ThrowsAsync<SqlException>(async () => await TestObject.Save(UpdatedResults).ExecuteAsync());
            Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.Equal(3, Results.Max(x => x.ManyToManyClass.Max(y => y.ID)));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default");
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToManyClass.Clear();
            }).ToArray();
            Assert.Equal(6, await TestObject.Save(UpdatedResults).ExecuteAsync());
            Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => !x.ManyToManyClass.Any()));
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            SetupData();
            Assert.Equal(0, await TestObject.Save<ManyToManyProperties>(null).ExecuteAsync());
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            var TestObject = new Session(InternalMappingManager, InternalSchemaManager, InternalQueryProviderManager, AOPManager);
            var Result = new ManyToManyProperties
            {
                BoolValue = false
            };
            Result.ManyToManyClass.Add(new AllReferencesAndID
            {
                ByteArrayValue = new byte[] { 9, 10, 11, 12 },
                ByteValue = 34,
                CharValue = 'c',
                DateTimeValue = new DateTime(2000, 1, 1)
            });
            await Assert.ThrowsAsync<SqlException>(async () => await TestObject.Save(Result).ExecuteAsync());
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            Assert.Equal(1, Results.Count());
        }

        private void SetupData()
        {
            new SQLHelper.SQLHelper(Configuration, SqlClientFactory.Instance)
                .CreateBatch()
                .AddQuery(@"INSERT INTO [dbo].[ManyToManyProperties_]([BoolValue_]) VALUES (1)
INSERT INTO [dbo].[ManyToManyProperties_]([BoolValue_]) VALUES (1)
INSERT INTO [dbo].[ManyToManyProperties_]([BoolValue_]) VALUES (1)", CommandType.Text)
                .AddQuery(@"INSERT INTO [dbo].[ManyToManyPropertiesWithCascade_]([BoolValue_]) VALUES (1)
INSERT INTO [dbo].[ManyToManyPropertiesWithCascade_]([BoolValue_]) VALUES (1)
INSERT INTO [dbo].[ManyToManyPropertiesWithCascade_]([BoolValue_]) VALUES (1)", CommandType.Text)
                .AddQuery(@"INSERT INTO [dbo].[AllReferencesAndID_]([BoolValue_],[ByteArrayValue_],[ByteValue_],[CharValue_],[DateTimeValue_],[DecimalValue_],[DoubleValue_],[FloatValue_],[GuidValue_],[IntValue_],[LongValue_],[SByteValue_],[ShortValue_],[StringValue1_],[StringValue2_],[TimeSpanValue_],[UIntValue_],[ULongValue_],[UShortValue_]) VALUES (1,1,1,'a','1/1/2008',13.2,423.12341234,1243.1,'ad0d39ad-6889-4ab3-965d-3d4042344ee6',12,2,1,2,'asdfvzxcv','qwerertyizjgposgj','January 1, 1900 00:00:00.100',12,5342,1234)", CommandType.Text)
                .AddQuery(@"INSERT INTO [dbo].[AllReferencesAndID_]([BoolValue_],[ByteArrayValue_],[ByteValue_],[CharValue_],[DateTimeValue_],[DecimalValue_],[DoubleValue_],[FloatValue_],[GuidValue_],[IntValue_],[LongValue_],[SByteValue_],[ShortValue_],[StringValue1_],[StringValue2_],[TimeSpanValue_],[UIntValue_],[ULongValue_],[UShortValue_]) VALUES (1,1,1,'a','1/1/2008',13.2,423.12341234,1243.1,'ad0d39ad-6889-4ab3-965d-3d4042344ee6',12,2,1,2,'asdfvzxcv','qwerertyizjgposgj','January 1, 1900 00:00:00.100',12,5342,1234)", CommandType.Text)
                .AddQuery(@"INSERT INTO [dbo].[AllReferencesAndID_]([BoolValue_],[ByteArrayValue_],[ByteValue_],[CharValue_],[DateTimeValue_],[DecimalValue_],[DoubleValue_],[FloatValue_],[GuidValue_],[IntValue_],[LongValue_],[SByteValue_],[ShortValue_],[StringValue1_],[StringValue2_],[TimeSpanValue_],[UIntValue_],[ULongValue_],[UShortValue_]) VALUES (1,1,1,'a','1/1/2008',13.2,423.12341234,1243.1,'ad0d39ad-6889-4ab3-965d-3d4042344ee6',12,2,1,2,'asdfvzxcv','qwerertyizjgposgj','January 1, 1900 00:00:00.100',12,5342,1234)", CommandType.Text)
                .AddQuery(@"INSERT INTO [dbo].[AllReferencesAndID_ManyToManyProperties]([ManyToManyProperties_ID_],[AllReferencesAndID_ID_]) VALUES (1,1)
INSERT INTO [dbo].[AllReferencesAndID_ManyToManyProperties]([ManyToManyProperties_ID_],[AllReferencesAndID_ID_]) VALUES (2,2)
INSERT INTO [dbo].[AllReferencesAndID_ManyToManyProperties]([ManyToManyProperties_ID_],[AllReferencesAndID_ID_]) VALUES (3,3)", CommandType.Text)
                .AddQuery(@"INSERT INTO [dbo].[AllReferencesAndID_ManyToManyPropertiesWithCascade]([ManyToManyPropertiesWithCascade_ID_],[AllReferencesAndID_ID_]) VALUES (1,1)
INSERT INTO [dbo].[AllReferencesAndID_ManyToManyPropertiesWithCascade]([ManyToManyPropertiesWithCascade_ID_],[AllReferencesAndID_ID_]) VALUES (2,2)
INSERT INTO [dbo].[AllReferencesAndID_ManyToManyPropertiesWithCascade]([ManyToManyPropertiesWithCascade_ID_],[AllReferencesAndID_ID_]) VALUES (3,3)", CommandType.Text)
                .ExecuteScalar<int>();
        }
    }
}