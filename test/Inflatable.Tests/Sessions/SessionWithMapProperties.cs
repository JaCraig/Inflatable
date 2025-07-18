﻿using BigBook;
using DragonHoard.Core;
using Inflatable.ClassMapper;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.MapProperties;
using Inflatable.Tests.TestDatabases.SimpleTest;
using Inflatable.Tests.TestDatabases.SimpleTestWithDatabase;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    [Collection("Test collection")]
    public class SessionWithMapProperties : TestingFixture
    {
        public SessionWithMapProperties(SetupFixture setupFixture)
            : base(setupFixture)
        {
            InternalMappingManager = new MappingManager([
                new AllReferencesAndIDMappingWithDatabase(),
                new MapPropertiesMapping(),
                new MappedPropertiesWithCascadeMapping(),
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
            _ = Resolve<ISession>();
            await SetupDataAsync();
            MapProperties[] Results = [.. DbContext<MapProperties>.CreateQuery()];
            Assert.Equal(3, Results.Length);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            System.Collections.Generic.IEnumerable<MapProperties> Result = await TestObject.ExecuteAsync<MapProperties>("SELECT TOP 2 ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            System.Collections.Generic.IEnumerable<MapProperties> Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default");
            _ = Assert.Single(Results);
            System.Collections.Generic.IEnumerable<AllReferencesAndID> Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.Equal(6, Results2.Count());
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabaseAndCascade()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            System.Collections.Generic.IEnumerable<MapPropertiesWithCascade> Result = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT TOP 2 ID_ as [ID] FROM MapPropertiesWithCascade_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            System.Collections.Generic.IEnumerable<MapPropertiesWithCascade> Results = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT ID_ as [ID] FROM MapPropertiesWithCascade_", CommandType.Text, "Default");
            _ = Assert.Single(Results);
            System.Collections.Generic.IEnumerable<AllReferencesAndID> Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default");
            Assert.NotEmpty(Results2);
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            System.Collections.Generic.IEnumerable<MapProperties> Result = await TestObject.ExecuteAsync<MapProperties>("SELECT TOP 1 ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            System.Collections.Generic.IEnumerable<MapProperties> Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default");
            Assert.Equal(2, Results.Count());
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            await DeleteData();
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession TestObject = Resolve<ISession>();
            System.Collections.Generic.IEnumerable<MapProperties> Result = await TestObject.ExecuteAsync<MapProperties>("SELECT TOP 1 ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default");
            _ = await TestObject.Delete(Result.ToArray()).ExecuteAsync();
            System.Collections.Generic.IEnumerable<MapProperties> Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default");
            Assert.Empty(Results);
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            var Result1 = new MapPropertiesWithCascade
            {
                BoolValue = false,
                MappedClass = new AllReferencesAndID
                {
                    ByteArrayValue = [1, 2, 3, 4],
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
                    ByteArrayValue = [5, 6, 7, 8],
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
                    ByteArrayValue = [9, 10, 11, 12],
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                }
            };
            _ = await TestObject.Save(Result1, Result2, Result3).ExecuteAsync();
            System.Collections.Generic.IEnumerable<MapPropertiesWithCascade> Results = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM MapPropertiesWithCascade_", CommandType.Text, "Default");
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
            _ = Resolve<ISession>();
            await SetupDataAsync();
            MapProperties Result = DbContext<MapProperties>.CreateQuery().First();
            Assert.NotNull(Result.MappedClass);
        }

        [Fact]
        public async Task UpdateMultipleCascadeWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            System.Collections.Generic.IEnumerable<MapPropertiesWithCascade> Results = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithCascade_", CommandType.Text, "Default");
            MapPropertiesWithCascade[] UpdatedResults = [.. Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.MappedClass = new AllReferencesAndID
                {
                    ByteArrayValue = [9, 10, 11, 12],
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                };
            })];
            _ = await TestObject.Save(UpdatedResults).ExecuteAsync();
            Results = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapPropertiesWithCascade_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.MappedClass.ID > 3));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            System.Collections.Generic.IEnumerable<MapProperties> Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapProperties_", CommandType.Text, "Default");
            MapProperties[] UpdatedResults = [.. Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.MappedClass = new AllReferencesAndID
                {
                    ByteArrayValue = [9, 10, 11, 12],
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                };
                var Result = AsyncHelper.RunSync(async () => await TestObject.Save(x.MappedClass).ExecuteAsync());
            })];
            Assert.Equal(6, await TestObject.Save(UpdatedResults).ExecuteAsync());
            Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapProperties_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.MappedClass.ID > 3));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            System.Collections.Generic.IEnumerable<MapProperties> Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapProperties_", CommandType.Text, "Default");
            MapProperties[] UpdatedResults = [.. Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.MappedClass = null;
            })];
            Assert.Equal(6, await TestObject.Save(UpdatedResults).ExecuteAsync());
            Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM MapProperties_", CommandType.Text, "Default");
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.MappedClass == null));
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            ISession TestObject = Resolve<ISession>();
            await SetupDataAsync();
            Assert.Equal(0, await TestObject.Save<MapProperties>(null).ExecuteAsync());
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            await DeleteData();
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession TestObject = Resolve<ISession>();
            var Result = new MapProperties
            {
                BoolValue = false,
                MappedClass = new AllReferencesAndID
                {
                    ByteArrayValue = [9, 10, 11, 12],
                    ByteValue = 34,
                    CharValue = 'c',
                    DateTimeValue = new DateTime(2000, 1, 1)
                }
            };
            _ = await TestObject.Save(Result).ExecuteAsync();
            System.Collections.Generic.IEnumerable<MapProperties> Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default");
            _ = Assert.Single(Results);
        }

        private async Task DeleteData()
        {
            _ = await Helper
                            .CreateBatch()
                            .AddQuery(CommandType.Text, "DELETE FROM AllReferencesAndID_")
                            .AddQuery(CommandType.Text, "DELETE FROM MapProperties_")
                            .AddQuery(CommandType.Text, "DELETE FROM MapPropertiesWithCascade_")
                            .ExecuteScalarAsync<int>().ConfigureAwait(false);
        }

        private async Task SetupDataAsync()
        {
            var TestObject = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession Session = Resolve<ISession>();
            _ = await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, "DELETE FROM AllReferencesAndID_")
                .AddQuery(CommandType.Text, "DELETE FROM MapProperties_")
                .AddQuery(CommandType.Text, "DELETE FROM MapPropertiesWithCascade_")
                .ExecuteScalarAsync<int>().ConfigureAwait(false);
            var InitialData = new MapProperties[]
            {
                new() {
                    BoolValue=true,
                    MappedClass = new AllReferencesAndID
                        {
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
                },
                new() {
                    BoolValue=true,
                    MappedClass = new AllReferencesAndID
                        {
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
                },
                new() {
                    BoolValue=true,
                    MappedClass = new AllReferencesAndID
                        {
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
                },
            };
            var InitialData2 = new MapPropertiesWithCascade[]
            {
                new() {
                    BoolValue=true,
                    MappedClass = new AllReferencesAndID
                        {
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
                },
                new() {
                    BoolValue=true,
                    MappedClass = new AllReferencesAndID
                        {
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
                },
                new() {
                    BoolValue=true,
                    MappedClass = new AllReferencesAndID
                        {
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
                },
            };

            _ = await Session.Save(InitialData.Select(x => x.MappedClass).ToArray()).ExecuteAsync().ConfigureAwait(false);
            _ = await Session.Save(InitialData2.Select(x => x.MappedClass).ToArray()).ExecuteAsync().ConfigureAwait(false);
            _ = await Session.Save(InitialData).ExecuteAsync().ConfigureAwait(false);
            _ = await Session.Save(InitialData2).ExecuteAsync().ConfigureAwait(false);
        }
    }
}