using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.ManyToOneProperties.Mappings;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Schema
{
    public class ManyToOneClassMappingTests : TestingFixture
    {
        public ManyToOneClassMappingTests()
        {
            Mappings = new MappingManager(new IMapping[] {
                new ManyToOneOnePropertiesMapping(),
                new ManyToOneManyPropertiesMapping()
            },
            new IDatabase[]{
                new TestDatabaseMapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
            Canister.Builder.Bootstrapper.Resolve<ILogger>(),
            ObjectPool);
        }

        private MappingManager Mappings { get; }

        [Fact]
        public async Task Creation()
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
            var TestObject = new SchemaManager(Mappings, Configuration, Logger, DataModeler, Sherlock, Helper);
            Assert.Single(TestObject.Models);
            var TestModel = TestObject.Models.First();
            Assert.Equal("TestDatabase", TestModel.SourceSpec.Name);
            Assert.NotNull(TestModel.SourceSpec);
            Assert.Empty(TestModel.SourceSpec.Functions);
            Assert.Empty(TestModel.SourceSpec.StoredProcedures);
            Assert.Equal(2, TestModel.SourceSpec.Tables.Count);
            Assert.Contains(TestModel.SourceSpec.Tables, x => x.Name == "ManyToOneManyProperties_");
            Assert.Contains(TestModel.SourceSpec.Tables, x => x.Name == "ManyToOneOneProperties_");
            Assert.Empty(TestModel.SourceSpec.Views);
            Assert.Equal(4, TestModel.GeneratedSchemaChanges.Count());
            Assert.Contains("CREATE DATABASE [TestDatabase]", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[ManyToOneOneProperties_]([ID_] Int NOT NULL PRIMARY KEY IDENTITY,[BoolValue_] Bit NOT NULL,[ManyToOneManyProperties_ID_] Int)", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[ManyToOneManyProperties_]([ID_] Int NOT NULL PRIMARY KEY IDENTITY,[BoolValue_] Bit NOT NULL)", TestModel.GeneratedSchemaChanges);
            Assert.Contains("ALTER TABLE [dbo].[ManyToOneOneProperties_] ADD FOREIGN KEY ([ManyToOneManyProperties_ID_]) REFERENCES [dbo].[ManyToOneManyProperties_]([ID_]) ON DELETE SET NULL", TestModel.GeneratedSchemaChanges);
        }
    }
}