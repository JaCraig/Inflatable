using BigBook;
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
using System.Collections.Generic;
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
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TempSession.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await SetupDataAsync().ConfigureAwait(false);
            var Results = DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToArray();
            Assert.Equal(3, Results.Length);
            await TempSession.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
            var Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(6, Results2.Count());
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabaseAndCascade()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
            var Results2 = await TestObject.ExecuteAsync<AllReferencesAndID>("SELECT ID_ as [ID] FROM AllReferencesAndID_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(4, Results2.Count());
            await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(2, Results.Count());
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Empty(Results);
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await SetupDataAsync().ConfigureAwait(false);
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
            await TestObject.Save(Result1, Result2, Result3).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
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
            await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task LoadMapPropertyWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await SetupDataAsync().ConfigureAwait(false);
            var Result = DbContext<ManyToManyProperties>.CreateQuery().First();
            Assert.NotNull(Result.ManyToManyClass);
            Assert.Single(Result.ManyToManyClass);
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task UpdateMultipleCascadeWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
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
            await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false);
            Results = await TestObject.ExecuteAsync<ManyToManyPropertiesWithCascade>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertiesWithCascade_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.Equal(2, Results.Max(x => x.ManyToManyClass.Count));
            await TestObject.Delete(DbContext<ManyToManyPropertiesWithCascade>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
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
            await Assert.ThrowsAsync<SqlException>(async () => await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false)).ConfigureAwait(false);
            Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.Equal(1, Results.Max(x => x.ManyToManyClass.Count));
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.ManyToManyClass.Clear();
            }).ToArray();
            Assert.Equal(6, await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false));
            Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.ManyToManyClass.Count == 0));
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await SetupDataAsync().ConfigureAwait(false);
            Assert.Equal(0, await TestObject.Save<ManyToManyProperties>(null).ExecuteAsync().ConfigureAwait(false));
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            var TestObject = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TestObject.Delete(DbContext<ManyToManyProperties>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            await TestObject.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
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
            await Assert.ThrowsAsync<SqlException>(async () => await TestObject.Save(Result).ExecuteAsync().ConfigureAwait(false)).ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyProperties>("SELECT ID_ as [ID] FROM ManyToManyProperties_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Single(Results);
        }

        private async Task SetupDataAsync()
        {
            var TestObject = new SchemaManager(MappingManager, Configuration, Logger, DataModeler, Sherlock, Helper);
            var Session = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, "DELETE FROM ManyToManyProperties_")
                .AddQuery(CommandType.Text, "DELETE FROM ManyToManyPropertiesWithCascade_")
                .AddQuery(CommandType.Text, "DELETE FROM AllReferencesAndID_")
                .ExecuteScalarAsync<int>().ConfigureAwait(false);
            var InitialData = new ManyToManyProperties[]
            {
                new ManyToManyProperties
                {
                    BoolValue=true,
                    ManyToManyClass=new List<AllReferencesAndID>()
                    {
                        new AllReferencesAndID
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
                    }
                },
                new ManyToManyProperties
                {
                    BoolValue=true,
                    ManyToManyClass=new List<AllReferencesAndID>()
                    {
                        new AllReferencesAndID
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
                    }
                },
                new ManyToManyProperties
                {
                    BoolValue=true,
                    ManyToManyClass=new List<AllReferencesAndID>()
                    {
                        new AllReferencesAndID
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
                    }
                },
            };
            var InitialData2 = new ManyToManyPropertiesWithCascade[]
            {
                new ManyToManyPropertiesWithCascade
                {
                    BoolValue=true,
                    ManyToManyClass=new List<AllReferencesAndID>()
                    {
                        new AllReferencesAndID
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
                    }
                },
                new ManyToManyPropertiesWithCascade
                {
                    BoolValue=true,
                    ManyToManyClass=new List<AllReferencesAndID>()
                    {
                        new AllReferencesAndID
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
                    }
                },
                new ManyToManyPropertiesWithCascade
                {
                    BoolValue=true,
                    ManyToManyClass=new List<AllReferencesAndID>()
                    {
                        new AllReferencesAndID
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
                    }
                },
            };
            await Session.Save(InitialData.SelectMany(x => x.ManyToManyClass).ToArray()).ExecuteAsync().ConfigureAwait(false);
            await Session.Save(InitialData).ExecuteAsync().ConfigureAwait(false);
            await Session.Save(InitialData2).ExecuteAsync().ConfigureAwait(false);
        }
    }
}