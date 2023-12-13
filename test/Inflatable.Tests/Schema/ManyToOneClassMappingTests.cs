using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.ManyToOneProperties.Mappings;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Schema
{
    [Collection("Test collection")]
    public class ManyToOneClassMappingTests : TestingFixture
    {
        public ManyToOneClassMappingTests(SetupFixture setupFixture)
            : base(setupFixture)
        {
            Mappings = new MappingManager(new IMapping[] {
                new ManyToOneOnePropertiesMapping(),
                new ManyToOneManyPropertiesMapping()
            },
            new IDatabase[]{
                new TestDatabaseMapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>()) }, GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>());
        }

        private MappingManager Mappings { get; }

        [Fact]
        public async Task Creation()
        {
            try
            {
                _ = await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>();
            }
            catch { }
            var TestObject = new SchemaManager(Mappings, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            _ = Assert.Single(TestObject.Models);
            DataModel TestModel = TestObject.Models.First();
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