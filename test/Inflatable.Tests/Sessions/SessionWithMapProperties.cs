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
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, SQLHelperLogger) }, GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>());
            InternalSchemaManager = new SchemaManager(InternalMappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());

            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool, SQLHelperLogger);
            InternalQueryProviderManager = new QueryProviderManager(new[] { TempQueryProvider }, GetLogger<QueryProviderManager>());

            CacheManager = Canister.Builder.Bootstrapper.Resolve<Cache>();
            CacheManager.GetOrAddCache("Inflatable").Compact(1);
        }

        public static Aspectus.Aspectus AOPManager => Canister.Builder.Bootstrapper.Resolve<Aspectus.Aspectus>();
        public Cache CacheManager { get; set; }
        public MappingManager InternalMappingManager { get; set; }

        public QueryProviderManager InternalQueryProviderManager { get; set; }
        public SchemaManager InternalSchemaManager { get; set; }

        [Fact]
        public async Task AllNoParametersWithDataInDatabase()
        {
            _ = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Results = DbContext<MapProperties>.CreateQuery().ToArray();
            Assert.Equal(3, Results.Length);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<MapProperties>("SELECT TOP 2 ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
            var Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(6, Results2.Count());
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabaseAndCascade()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT TOP 2 ID_ as [ID] FROM MapPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapPropertiesWithCascade>("SELECT ID_ as [ID] FROM MapPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
            var Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.NotEmpty(Results2);
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<MapProperties>("SELECT TOP 1 ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(2, Results.Count());
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            await DeleteData().ConfigureAwait(false);
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            var Result = await TestObject.ExecuteAsync<MapProperties>("SELECT TOP 1 ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<MapProperties>("SELECT ID_ as [ID] FROM MapProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Empty(Results);
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
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
            _ = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            var Result = DbContext<MapProperties>.CreateQuery().First();
            Assert.NotNull(Result.MappedClass);
        }

        [Fact]
        public async Task UpdateMultipleCascadeWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
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
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
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
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
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
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            Assert.Equal(0, await TestObject.Save<MapProperties>(null).ExecuteAsync().ConfigureAwait(false));
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            await DeleteData().ConfigureAwait(false);
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
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

        private static async Task DeleteData()
        {
            await Helper
                            .CreateBatch()
                            .AddQuery(CommandType.Text, "DELETE FROM AllReferencesAndID_")
                            .AddQuery(CommandType.Text, "DELETE FROM MapProperties_")
                            .AddQuery(CommandType.Text, "DELETE FROM MapPropertiesWithCascade_")
                            .ExecuteScalarAsync<int>().ConfigureAwait(false);
        }

        private async Task SetupDataAsync()
        {
            var TestObject = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            var Session = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, "DELETE FROM AllReferencesAndID_")
                .AddQuery(CommandType.Text, "DELETE FROM MapProperties_")
                .AddQuery(CommandType.Text, "DELETE FROM MapPropertiesWithCascade_")
                .ExecuteScalarAsync<int>().ConfigureAwait(false);
            var InitialData = new MapProperties[]
            {
                new MapProperties
                {
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
                new MapProperties
                {
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
                new MapProperties
                {
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
                new MapPropertiesWithCascade
                {
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
                new MapPropertiesWithCascade
                {
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
                new MapPropertiesWithCascade
                {
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

            await Session.Save(InitialData.Select(x => x.MappedClass).ToArray()).ExecuteAsync().ConfigureAwait(false);
            await Session.Save(InitialData2.Select(x => x.MappedClass).ToArray()).ExecuteAsync().ConfigureAwait(false);
            await Session.Save(InitialData).ExecuteAsync().ConfigureAwait(false);
            await Session.Save(InitialData2).ExecuteAsync().ConfigureAwait(false);
        }
    }
}