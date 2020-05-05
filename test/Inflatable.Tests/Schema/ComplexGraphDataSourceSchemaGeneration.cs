using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph.Mappings;
using Inflatable.Tests.TestDatabases.Databases;
using Serilog;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.Schema
{
    public class ComplexGraphDataSourceSchemaGeneration : TestingFixture
    {
        public ComplexGraphDataSourceSchemaGeneration()
        {
            Mappings = new MappingManager(new IMapping[] {
                new BaseClass1MappingWithDatabase(),
                new ConcreteClass1MappingWithDatabase(),
                new ConcreteClass2MappingWithDatabase(),
                new ConcreteClass3MappingWithDatabase(),
                new IInterface1MappingWithDatabase(),
                new IInterface2MappingWithDatabase()
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
        public void Creation()
        {
            var TestObject = new SchemaManager(Mappings, Configuration, Logger, DataModeler, Sherlock, Helper);
            Assert.Single(TestObject.Models);
            var TestModel = TestObject.Models.First();
            Assert.Equal("TestDatabase", TestModel.SourceSpec.Name);
            Assert.NotNull(TestModel.SourceSpec);
            Assert.Empty(TestModel.SourceSpec.Functions);
            Assert.Empty(TestModel.SourceSpec.StoredProcedures);
            Assert.Equal(5, TestModel.SourceSpec.Tables.Count);
            Assert.Contains("BaseClass1_", TestModel.SourceSpec.Tables.Select(x => x.Name));
            Assert.Contains("ConcreteClass1_", TestModel.SourceSpec.Tables.Select(x => x.Name));
            Assert.Contains("ConcreteClass2_", TestModel.SourceSpec.Tables.Select(x => x.Name));
            Assert.Contains("ConcreteClass3_", TestModel.SourceSpec.Tables.Select(x => x.Name));
            Assert.Contains("IInterface1_", TestModel.SourceSpec.Tables.Select(x => x.Name));
            Assert.Empty(TestModel.SourceSpec.Views);
            Assert.Equal(12, TestModel.GeneratedSchemaChanges.Count());
            Assert.Contains("CREATE DATABASE [TestDatabase]", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[ConcreteClass2_]([ID_] BigInt PRIMARY KEY IDENTITY,[InterfaceValue_] Int NOT NULL,[BaseClass1_ID_] BigInt NOT NULL UNIQUE)", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[IInterface1_]([ID_] Int NOT NULL PRIMARY KEY IDENTITY)", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[ConcreteClass3_]([ID_] BigInt PRIMARY KEY IDENTITY,[MyUniqueProperty_] Int NOT NULL,[IInterface1_ID_] Int NOT NULL UNIQUE)", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE UNIQUE INDEX [Index_IInterface1_ID_1] ON [dbo].[ConcreteClass3_]([IInterface1_ID_])", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[ConcreteClass1_]([ID_] BigInt PRIMARY KEY IDENTITY,[Value1_] Int NOT NULL,[BaseClass1_ID_] BigInt NOT NULL UNIQUE)", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[BaseClass1_]([ID_] BigInt PRIMARY KEY IDENTITY,[BaseClassValue1_] Int NOT NULL,[IInterface1_ID_] Int NOT NULL UNIQUE)", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE UNIQUE INDEX [Index_IInterface1_ID_1] ON [dbo].[BaseClass1_]([IInterface1_ID_])", TestModel.GeneratedSchemaChanges);
            Assert.Contains("ALTER TABLE [dbo].[ConcreteClass2_] ADD FOREIGN KEY ([BaseClass1_ID_]) REFERENCES [dbo].[BaseClass1_]([ID_]) ON DELETE CASCADE ON UPDATE CASCADE", TestModel.GeneratedSchemaChanges);
            Assert.Contains("ALTER TABLE [dbo].[ConcreteClass3_] ADD FOREIGN KEY ([IInterface1_ID_]) REFERENCES [dbo].[IInterface1_]([ID_]) ON DELETE CASCADE ON UPDATE CASCADE", TestModel.GeneratedSchemaChanges);
            Assert.Contains("ALTER TABLE [dbo].[ConcreteClass1_] ADD FOREIGN KEY ([BaseClass1_ID_]) REFERENCES [dbo].[BaseClass1_]([ID_]) ON DELETE CASCADE ON UPDATE CASCADE", TestModel.GeneratedSchemaChanges);
            Assert.Contains("ALTER TABLE [dbo].[BaseClass1_] ADD FOREIGN KEY ([IInterface1_ID_]) REFERENCES [dbo].[IInterface1_]([ID_]) ON DELETE CASCADE ON UPDATE CASCADE", TestModel.GeneratedSchemaChanges);
        }
    }
}