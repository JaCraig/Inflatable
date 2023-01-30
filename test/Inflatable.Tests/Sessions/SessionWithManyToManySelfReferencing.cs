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
using Inflatable.Tests.TestDatabases.ManyToManyProperties;
using Inflatable.Tests.TestDatabases.ManyToManyProperties.Mappings;
using Microsoft.Data.SqlClient;
using SQLHelperDB;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    public class SessionWithManyToManyPropertiesSelfReferencing : TestingFixture
    {
        public SessionWithManyToManyPropertiesSelfReferencing()
        {
            InternalMappingManager = new MappingManager(new IMapping[] {
                new ManyToManyPropertySelfReferencingMapping()
            },
            new IDatabase[]{
                new TestDatabaseMapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelper>()) }, GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>());
            InternalSchemaManager = new SchemaManager(InternalMappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());

            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>());
            InternalQueryProviderManager = new QueryProviderManager(new[] { TempQueryProvider }, GetLogger<QueryProviderManager>());

            CacheManager = Resolve<Cache>();
            CacheManager.GetOrAddCache("Inflatable").Compact(1);
        }

        public static Aspectus.Aspectus AOPManager => Resolve<Aspectus.Aspectus>();
        public Cache CacheManager { get; set; }
        public MappingManager InternalMappingManager { get; set; }

        public QueryProviderManager InternalQueryProviderManager { get; set; }
        public SchemaManager InternalSchemaManager { get; set; }

        [Fact]
        public async Task AllNoParametersWithDataInDatabase()
        {
            _ = Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Results = DbContext<ManyToManyPropertySelfReferencing>.CreateQuery().ToArray();
            Assert.Equal(6, Results.Length);
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabase()
        {
            var TestObject = Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(4, Results.Count());
        }

        [Fact]
        public async Task DeleteMultipleWithDataInDatabaseAndCascade()
        {
            var TestObject = Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 2 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(4, Results.Count());
        }

        [Fact]
        public async Task DeleteWithDataInDatabase()
        {
            var TestObject = Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(5, Results.Count());
        }

        [Fact]
        public async Task DeleteWithNoDataInDatabase()
        {
            try
            {
                await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false);
            }
            catch { }
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            var TestObject = Resolve<ISession>();

            var Result = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT TOP 1 ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            await TestObject.Delete(Result.ToArray()).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Empty(Results);
        }

        [Fact]
        public async Task InsertMultipleObjectsWithCascade()
        {
            var TestObject = Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Result1 = new ManyToManyPropertySelfReferencing
            {
                BoolValue = false
            };
            Result1.Children.Add(new ManyToManyPropertySelfReferencing
            {
                BoolValue = true
            });
            var Result2 = new ManyToManyPropertySelfReferencing
            {
                BoolValue = false
            };
            Result2.Children.Add(new ManyToManyPropertySelfReferencing
            {
                BoolValue = true
            });
            var Result3 = new ManyToManyPropertySelfReferencing
            {
                BoolValue = false
            };
            Result3.Children.Add(new ManyToManyPropertySelfReferencing
            {
                BoolValue = true
            });
            await TestObject.Save(Result1, Result2, Result3).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID], BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(12, Results.Count());
            Assert.Contains(Results, x => x.ID == Result1.ID
            && !x.BoolValue
            && x.Children.Count == 1
            && x.Children.All(y => y.BoolValue));
            Assert.Contains(Results, x => x.ID == Result2.ID
            && !x.BoolValue
            && x.Children.Count == 1
            && x.Children.All(y => y.BoolValue));
            Assert.Contains(Results, x => x.ID == Result3.ID
            && !x.BoolValue
            && x.Children.Count == 1
            && x.Children.All(y => y.BoolValue));
        }

        [Fact]
        public async Task LoadMapPropertyWithDataInDatabase()
        {
            _ = Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Result = DbContext<ManyToManyPropertySelfReferencing>.CreateQuery().Skip(1).Take(1).First();
            Assert.NotNull(Result.Children);
            Assert.Single(Result.Children);
        }

        [Fact]
        public async Task UpdateMultipleCascadeWithDataInDatabase()
        {
            var TestObject = Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.Children.ForEach(y => y.BoolValue = false);
                x.Children.Add(new ManyToManyPropertySelfReferencing
                {
                    BoolValue = false
                });
            }).ToArray();
            await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false);
            Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Results = Results.Where(x => x.Children.Count > 0);
            Assert.Equal(6, Results.Count());
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabase()
        {
            var TestObject = Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.Children.Add(new ManyToManyPropertySelfReferencing
                {
                    BoolValue = true
                });
                var Result = TestObject.Save(x.Children).ExecuteAsync().GetAwaiter().GetResult();
            }).ToArray();
            await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false);
            Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.False(Results.All(x => !x.BoolValue));
            Results = Results.Where(x => x.Children.Count > 0);
            Assert.Equal(6, Results.Count());
        }

        [Fact]
        public async Task UpdateMultipleWithDataInDatabaseToNull()
        {
            var TestObject = Resolve<ISession>();

            await SetupDataAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            var UpdatedResults = Results.ForEach(x =>
            {
                x.BoolValue = false;
                x.Children.Clear();
            }).ToArray();
            Assert.Equal(9, await TestObject.Save(UpdatedResults).ExecuteAsync().ConfigureAwait(false));
            Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID],BoolValue_ as [BoolValue] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.True(Results.All(x => !x.BoolValue));
            Assert.True(Results.All(x => x.Children.Count == 0));
        }

        [Fact]
        public async Task UpdateNullWithDataInDatabase()
        {
            var TestObject = Resolve<ISession>();
            await SetupDataAsync().ConfigureAwait(false);
            Assert.Equal(0, await TestObject.Save<ManyToManyPropertySelfReferencing>(null).ExecuteAsync().ConfigureAwait(false));
        }

        [Fact]
        public async Task UpdateWithNoDataInDatabase()
        {
            try
            {
                await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false);
            }
            catch { }
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            var TestObject = Resolve<ISession>();
            var Result = new ManyToManyPropertySelfReferencing
            {
                BoolValue = false
            };
            Result.Children.Add(new ManyToManyPropertySelfReferencing
            {
                BoolValue = true
            });
            await TestObject.Save(Result).ExecuteAsync().ConfigureAwait(false);
            var Results = await TestObject.ExecuteAsync<ManyToManyPropertySelfReferencing>("SELECT ID_ as [ID] FROM ManyToManyPropertySelfReferencing_", CommandType.Text, "Default").ConfigureAwait(false);
            Assert.Equal(2, Results.Count());
        }

        private async Task SetupDataAsync()
        {
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            var Session = Resolve<ISession>();
            await Helper
                .CreateBatch()
                .AddQuery(CommandType.Text, "DELETE FROM Parent_Child")
                .AddQuery(CommandType.Text, "DELETE FROM ManyToManyPropertySelfReferencing_")
                .ExecuteAsync()
                .ConfigureAwait(false);
            var InitialData = new ManyToManyPropertySelfReferencing[]
            {
                new ManyToManyPropertySelfReferencing
                {
                    BoolValue=true,
                    Children=new List<ManyToManyPropertySelfReferencing>
                    {
                        new ManyToManyPropertySelfReferencing
                        {
                            BoolValue=true
                        }
                    }
                },
                new ManyToManyPropertySelfReferencing
                {
                    BoolValue=true,
                    Children=new List<ManyToManyPropertySelfReferencing>
                    {
                        new ManyToManyPropertySelfReferencing
                        {
                            BoolValue=true
                        }
                    }
                },
                new ManyToManyPropertySelfReferencing
                {
                    BoolValue=true,
                    Children=new List<ManyToManyPropertySelfReferencing>
                    {
                        new ManyToManyPropertySelfReferencing
                        {
                            BoolValue=true
                        }
                    }
                }
            };
            await Session.Save(InitialData).ExecuteAsync().ConfigureAwait(false);
        }
    }
}