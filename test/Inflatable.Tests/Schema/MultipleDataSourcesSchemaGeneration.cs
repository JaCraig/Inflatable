using Inflatable.ClassMapper;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.MultipleDataSources.Mappings;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Schema
{
    [Collection("Test collection")]
    public class MultipleDataSourcesSchemaGeneration : TestingFixture
    {
        public MultipleDataSourcesSchemaGeneration(SetupFixture setupFixture)
            : base(setupFixture)
        {
            Mappings = new MappingManager([
                new SimpleClassDataSource1MappingWithDatabase(),
                new SimpleClassDataSource2MappingWithDatabase()
            ],
            [
                new TestDatabaseMapping(),
                new TestDatabase2Mapping()
            ],
            new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>());
        }

        private MappingManager Mappings { get; }

        [Fact]
        public async Task Creation()
        {
            try
            {
                await TestDatabaseManager.ResetKnownDatabasesAsync();
            }
            catch { }
            var TestObject = new SchemaManager(Mappings, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            Assert.Equal(2, TestObject.Models.Count());
            DataModel TestModel = TestObject.Models.First(x => x.SourceSpec.Name == "TestDatabase");
            Assert.Equal("TestDatabase", TestModel.SourceSpec.Name);
            Assert.NotNull(TestModel.SourceSpec);
            Assert.Empty(TestModel.SourceSpec.Functions);
            Assert.Empty(TestModel.SourceSpec.StoredProcedures);
            _ = Assert.Single(TestModel.SourceSpec.Tables);
            Assert.Equal("SimpleClass_", TestModel.SourceSpec.Tables[0].Name);
            Assert.Empty(TestModel.SourceSpec.Views);
            _ = Assert.Single(TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[SimpleClass_]([ID_] Int NOT NULL PRIMARY KEY,[DataSource1Value_] Int NOT NULL)", TestModel.GeneratedSchemaChanges);

            TestModel = TestObject.Models.First(x => x.SourceSpec.Name == "TestDatabase2");
            Assert.Equal("TestDatabase2", TestModel.SourceSpec.Name);
            Assert.NotNull(TestModel.SourceSpec);
            Assert.Empty(TestModel.SourceSpec.Functions);
            Assert.Empty(TestModel.SourceSpec.StoredProcedures);
            _ = Assert.Single(TestModel.SourceSpec.Tables);
            Assert.Equal("SimpleClass_", TestModel.SourceSpec.Tables[0].Name);
            Assert.Empty(TestModel.SourceSpec.Views);
            _ = Assert.Single(TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[SimpleClass_]([ID_] Int NOT NULL PRIMARY KEY,[DataSource2Value_] Int NOT NULL)", TestModel.GeneratedSchemaChanges);
        }
    }
}