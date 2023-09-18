using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.Fixtures;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.MultipleDataSources.Mappings;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Schema
{
    public class MultipleDataSourcesSchemaGeneration : TestingFixture
    {
        public MultipleDataSourcesSchemaGeneration(SetupFixture setupFixture)
            : base(setupFixture)
        {
            Mappings = new MappingManager(new IMapping[] {
                new SimpleClassDataSource1MappingWithDatabase(),
                new SimpleClassDataSource2MappingWithDatabase()
            },
            new IDatabase[]{
                new TestDatabaseMapping(),
                new TestDatabase2Mapping()
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
                await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase SET ONLINE\r\nDROP DATABASE TestDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE TestDatabase2 SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE TestDatabase2 SET ONLINE\r\nDROP DATABASE TestDatabase2")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabase SET ONLINE\r\nDROP DATABASE MockDatabase")
                    .AddQuery(CommandType.Text, "ALTER DATABASE MockDatabaseForMockMapping SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE MockDatabaseForMockMapping SET ONLINE\r\nDROP DATABASE MockDatabaseForMockMapping")
                    .ExecuteScalarAsync<int>();
            }
            catch { }
            var TestObject = new SchemaManager(Mappings, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            Assert.Equal(2, TestObject.Models.Count());
            var TestModel = TestObject.Models.First(x => x.SourceSpec.Name == "TestDatabase");
            Assert.Equal("TestDatabase", TestModel.SourceSpec.Name);
            Assert.NotNull(TestModel.SourceSpec);
            Assert.Empty(TestModel.SourceSpec.Functions);
            Assert.Empty(TestModel.SourceSpec.StoredProcedures);
            Assert.Single(TestModel.SourceSpec.Tables);
            Assert.Equal("SimpleClass_", TestModel.SourceSpec.Tables[0].Name);
            Assert.Empty(TestModel.SourceSpec.Views);
            Assert.Equal(2, TestModel.GeneratedSchemaChanges.Count());
            Assert.Contains("CREATE DATABASE [TestDatabase]", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[SimpleClass_]([ID_] Int NOT NULL PRIMARY KEY,[DataSource1Value_] Int NOT NULL)", TestModel.GeneratedSchemaChanges);

            TestModel = TestObject.Models.First(x => x.SourceSpec.Name == "TestDatabase2");
            Assert.Equal("TestDatabase2", TestModel.SourceSpec.Name);
            Assert.NotNull(TestModel.SourceSpec);
            Assert.Empty(TestModel.SourceSpec.Functions);
            Assert.Empty(TestModel.SourceSpec.StoredProcedures);
            Assert.Single(TestModel.SourceSpec.Tables);
            Assert.Equal("SimpleClass_", TestModel.SourceSpec.Tables[0].Name);
            Assert.Empty(TestModel.SourceSpec.Views);
            Assert.Equal(2, TestModel.GeneratedSchemaChanges.Count());
            Assert.Contains("CREATE DATABASE [TestDatabase2]", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[SimpleClass_]([ID_] Int NOT NULL PRIMARY KEY,[DataSource2Value_] Int NOT NULL)", TestModel.GeneratedSchemaChanges);
        }
    }
}