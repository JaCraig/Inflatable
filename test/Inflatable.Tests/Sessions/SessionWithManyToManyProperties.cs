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
using Inflatable.Tests.TestDatabases.SimpleTest;
using Inflatable.Tests.TestDatabases.SimpleTestWithDatabase;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    [Collection("Test collection")]
    public class SessionWithManyToManyProperties : TestingFixture
    {
        public SessionWithManyToManyProperties(SetupFixture setupFixture)
            : base(setupFixture)
        {
            InternalMappingManager = new MappingManager([
                new AllReferencesAndIDMappingWithDatabase(),
                new ManyToManyPropertiesWithCascadeMapping(),
                new ManyToManyPropertiesMapping(),
            ],
            [
                new TestDatabaseMapping()
            ],
            new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>());
            InternalSchemaManager = new SchemaManager(InternalMappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());

            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>());
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
            ISession TempSession = Resolve<ISession>();
            _ = await TempSession.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            await SetupDataAsync();
            ManyToManyPropertiesWithCascade[] Results = [.. DbContext<ManyToManyPropertiesWithCascade>.CreateQuery()];
            Assert.Equal(3, Results.Length);
            _ = await TempSession.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            await SetupDataAsync();
            IEnumerable<ManyToManyProperties> Result = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToManyProperties> Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            _ = Assert.Single(Results);
            IEnumerable<AllReferencesAndID> Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Equal(6, Results2.Count());
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabaseAndCascade()
        {
            ISession TestObject = Resolve<ISession>();
            _ = await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            await SetupDataAsync();
            IEnumerable<ManyToManyPropertiesWithCascade> Result = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToManyPropertiesWithCascade> Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default");
            _ = Assert.Single(Results);
            IEnumerable<AllReferencesAndID> Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Equal(4, Results2.Count());
            _ = await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            await SetupDataAsync();
            IEnumerable<ManyToManyProperties> Result = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToManyProperties> Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            Assert.Equal(2, Results.Count());
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            IEnumerable<ManyToManyProperties> Result = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            IEnumerable<ManyToManyProperties> Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            Assert.Empty(Results);
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            ISession TestObject = Resolve<ISession>();
            _ = await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            await SetupDataAsync();
            var Result1 = new ManyToManyPropertiesWithCascade
            {
                BoolValue = false
            };
            Result1.ManyToManyClass.Add(new AllReferencesAndID
            {
                ByteArrayValue = [1, 2, 3, 4],
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
                ByteArrayValue = [5, 6, 7, 8],
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
                ByteArrayValue = [9, 10, 11, 12],
                ByteValue = 34,
                CharValue = 'c',
                DateTimeValue = new DateTime(2000, 1, 1)
            });
            _ = await TestObject.Save(Result1, Result2, Result3).ExecuteAsync();
            IEnumerable<ManyToManyPropertiesWithCascade> Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default");
            Assert.Equal(6, Results.Count());
            Assert.Contains(Results, x => x.ID == Result1.ID
            && !x.BoolValue
            && x.ManyToManyClass.Count == 1
            && x.ManyToManyClass.Any(y => y.ByteValue == 34
            && y.CharValue == 'a'
            && y.DateTimeValue == new DateTime(2000, 1, 1)));
            Assert.Contains(Results, x => x.ID == Result2.ID
            && !x.BoolValue
            && x.ManyToManyClass.Count == 1
            && x.ManyToManyClass.Any(y => y.ByteValue == 34
            && y.CharValue == 'b'
            && y.DateTimeValue == new DateTime(2000, 1, 1)));
            Assert.Contains(Results, x => x.ID == Result3.ID
            && !x.BoolValue
            && x.ManyToManyClass.Count == 1
            && x.ManyToManyClass.Any(y => y.ByteValue == 34
            && y.CharValue == 'c'
            && y.DateTimeValue == new DateTime(2000, 1, 1)));
            _ = await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }

        [Fact]
        public async Task LoadMapPropertyWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            await SetupDataAsync();
            ManyToManyProperties Result = DbContext<ManyToManyProperties>.CreateQuery().First();
            Assert.NotNull(Result.ManyToManyClass);
            _ = Assert.Single(Result.ManyToManyClass);
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }

        [Fact]
        public async Task UpdateMultipleCascadeWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            _ = await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            await SetupDataAsync();
            IEnumerable<ManyToManyPropertiesWithCascade> Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default");
            ManyToManyPropertiesWithCascade[] UpdatedResults = [.. Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToManyClass.Add(new AllReferencesAndID
                {
                    ByteArrayValue = [9, 10, 11, 12],
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                });
            })];
            _ = await TestObject.Save(UpdatedResults).ExecuteAsync();
            Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.Equal(2, Results.Max(x => x.ManyToManyClass.Count));
            _ = await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            await SetupDataAsync();
            IEnumerable<ManyToManyProperties> Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default");
            ManyToManyProperties[] UpdatedResults = [.. Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToManyClass.Add(new AllReferencesAndID
                {
                    ByteArrayValue = [9, 10, 11, 12],
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                });
                var Result = AsyncHelper.RunSync(async () => await TestObject.Save(x.ManyToManyClass).ExecuteAsync());
            })];
            _ = await Assert.ThrowsAsync<SqlException>(() => TestObject.Save(UpdatedResults).ExecuteAsync());
            Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.Equal(1, Results.Max(x => x.ManyToManyClass.Count));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            ISession TestObject = Resolve<ISession>();
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            await SetupDataAsync();
            IEnumerable<ManyToManyProperties> Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default");
            ManyToManyProperties[] UpdatedResults = [.. Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToManyClass.Clear();
            })];
            Assert.Equal(6, await TestObject.Save(UpdatedResults).ExecuteAsync());
            Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.ManyToManyClass.Count == 0));
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            await SetupDataAsync();
            Assert.Equal(0, await TestObject.Save<ManyToManyProperties>(null).ExecuteAsync());
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            _ = await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            _ = await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            var Result = new ManyToManyProperties
            {
                BoolValue = false
            };
            Result.ManyToManyClass.Add(new AllReferencesAndID
            {
                ByteArrayValue = [9, 10, 11, 12],
                ByteValue = 34,
                CharValue = 'c',
                DateTimeValue = new DateTime(2000, 1, 1)
            });
            _ = await Assert.ThrowsAsync<SqlException>(() => TestObject.Save(Result).ExecuteAsync());
            IEnumerable<ManyToManyProperties> Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default");
            _ = Assert.Single(Results);
        }

        private async Task SetupDataAsync()
        {
            var TestObject = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession Session = Resolve<ISession>();
            _ = await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, "DELETE FROM ManyToManyProperties_")
                .AddQuery(CommandType.Text, "DELETE FROM ManyToManyPropertiesWithCascade_")
                .AddQuery(CommandType.Text, "DELETE FROM AllReferencesAndID_")
                .ExecuteScalarAsync<int>().ConfigureAwait(false);
            var InitialData = new ManyToManyProperties[]
            {
                new() {
                    BoolValue=true,
                    ManyToManyClass=
                    [
                        new() {
                            BoolValue=true,
                            ByteValue=1,
                            NullableBoolValue=true,
                            CharValue='a',
                            DateTimeValue=new DateTime(2008,1,1),
                            DecimalValue=13.2m,
                            DoubleValue=423.12341234,
                            FloatValue=1243.1f,
                            GuidValue=Guid.Parse("ad0d39ad-6889-4ab3-965d-3d4042344ee6"),
                            IntValue=12,
                            LongValue=2,
                            NullableByteValue=1,
                            SByteValue=2,
                            ShortValue=1,
                            StringValue1="asdfvzxcv",
                            StringValue2="qwerertyizjgposgj",
                            ULongValue=12,
                            UIntValue=5342,
                            UShortValue=1234
                        }
                    ]
                },
                new() {
                    BoolValue=true,
                    ManyToManyClass=
                    [
                        new() {
                            BoolValue=true,
                            ByteValue=1,
                            NullableBoolValue=true,
                            CharValue='a',
                            DateTimeValue=new DateTime(2008,1,1),
                            DecimalValue=13.2m,
                            DoubleValue=423.12341234,
                            FloatValue=1243.1f,
                            GuidValue=Guid.Parse("ad0d39ad-6889-4ab3-965d-3d4042344ee6"),
                            IntValue=12,
                            LongValue=2,
                            NullableByteValue=1,
                            SByteValue=2,
                            ShortValue=1,
                            StringValue1="asdfvzxcv",
                            StringValue2="qwerertyizjgposgj",
                            ULongValue=12,
                            UIntValue=5342,
                            UShortValue=1234
                        }
                    ]
                },
                new() {
                    BoolValue=true,
                    ManyToManyClass=
                    [
                        new() {
                            BoolValue=true,
                            ByteValue=1,
                            NullableBoolValue=true,
                            CharValue='a',
                            DateTimeValue=new DateTime(2008,1,1),
                            DecimalValue=13.2m,
                            DoubleValue=423.12341234,
                            FloatValue=1243.1f,
                            GuidValue=Guid.Parse("ad0d39ad-6889-4ab3-965d-3d4042344ee6"),
                            IntValue=12,
                            LongValue=2,
                            NullableByteValue=1,
                            SByteValue=2,
                            ShortValue=1,
                            StringValue1="asdfvzxcv",
                            StringValue2="qwerertyizjgposgj",
                            ULongValue=12,
                            UIntValue=5342,
                            UShortValue=1234
                        }
                    ]
                },
            };
            var InitialData2 = new ManyToManyPropertiesWithCascade[]
            {
                new() {
                    BoolValue=true,
                    ManyToManyClass=
                    [
                        new() {
                            BoolValue=true,
                            ByteValue=1,
                            NullableBoolValue=true,
                            CharValue='a',
                            DateTimeValue=new DateTime(2008,1,1),
                            DecimalValue=13.2m,
                            DoubleValue=423.12341234,
                            FloatValue=1243.1f,
                            GuidValue=Guid.Parse("ad0d39ad-6889-4ab3-965d-3d4042344ee6"),
                            IntValue=12,
                            LongValue=2,
                            NullableByteValue=1,
                            SByteValue=2,
                            ShortValue=1,
                            StringValue1="asdfvzxcv",
                            StringValue2="qwerertyizjgposgj",
                            ULongValue=12,
                            UIntValue=5342,
                            UShortValue=1234
                        }
                    ]
                },
                new() {
                    BoolValue=true,
                    ManyToManyClass=
                    [
                        new() {
                            BoolValue=true,
                            ByteValue=1,
                            NullableBoolValue=true,
                            CharValue='a',
                            DateTimeValue=new DateTime(2008,1,1),
                            DecimalValue=13.2m,
                            DoubleValue=423.12341234,
                            FloatValue=1243.1f,
                            GuidValue=Guid.Parse("ad0d39ad-6889-4ab3-965d-3d4042344ee6"),
                            IntValue=12,
                            LongValue=2,
                            NullableByteValue=1,
                            SByteValue=2,
                            ShortValue=1,
                            StringValue1="asdfvzxcv",
                            StringValue2="qwerertyizjgposgj",
                            ULongValue=12,
                            UIntValue=5342,
                            UShortValue=1234
                        }
                    ]
                },
                new() {
                    BoolValue=true,
                    ManyToManyClass=
                    [
                        new() {
                            BoolValue=true,
                            ByteValue=1,
                            NullableBoolValue=true,
                            CharValue='a',
                            DateTimeValue=new DateTime(2008,1,1),
                            DecimalValue=13.2m,
                            DoubleValue=423.12341234,
                            FloatValue=1243.1f,
                            GuidValue=Guid.Parse("ad0d39ad-6889-4ab3-965d-3d4042344ee6"),
                            IntValue=12,
                            LongValue=2,
                            NullableByteValue=1,
                            SByteValue=2,
                            ShortValue=1,
                            StringValue1="asdfvzxcv",
                            StringValue2="qwerertyizjgposgj",
                            ULongValue=12,
                            UIntValue=5342,
                            UShortValue=1234
                        }
                    ]
                },
            };
            _ = await Session.Save(InitialData.SelectMany(x => x.ManyToManyClass).ToArray()).ExecuteAsync().ConfigureAwait(false);
            _ = await Session.Save(InitialData).ExecuteAsync().ConfigureAwait(false);
            _ = await Session.Save(InitialData2).ExecuteAsync().ConfigureAwait(false);
        }
    }
}