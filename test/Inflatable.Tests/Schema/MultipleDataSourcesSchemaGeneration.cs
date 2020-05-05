using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.MultipleDataSources.Mappings;
using Serilog;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.Schema
{
    public class MultipleDataSourcesSchemaGeneration : TestingFixture
    {
        public MultipleDataSourcesSchemaGeneration()
        {
            Mappings = new MappingManager(new IMapping[] {
                new SimpleClassDataSource1MappingWithDatabase(),
                new SimpleClassDataSource2MappingWithDatabase()
            },
            new IDatabase[]{
                new TestDatabaseMapping(),
                new TestDatabase2Mapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
            Canister.Builder.Bootstrapper.Resolve<ILogger>(),
            ObjectPool);
        }

        private MappingManager Mappings { get; }

        [Fact]
        public void Creation()
        {
            var TestObject = new SchemaManager(Mappings, Configuration, Logger, DataModeler, Sherlock, Helper);
            Assert.Equal(2, TestObject.Models.Count());
            var TestModel = TestObject.Models.Last();
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

            TestModel = TestObject.Models.First();
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