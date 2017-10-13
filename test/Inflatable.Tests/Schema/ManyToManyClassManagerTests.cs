using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Schema;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.ManyToManyProperties;
using Inflatable.Tests.TestDatabases.SimpleTestWithDatabase;
using Serilog;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.Schema
{
    public class ManyToManyClassManagerTests : TestingFixture
    {
        public ManyToManyClassManagerTests()
        {
            Mappings = new MappingManager(new IMapping[] {
                new AllReferencesAndIDMappingWithDatabase(),
                new ManyToManyPropertiesMapping()
            },
            new IDatabase[]{
                new TestDatabaseMapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration) }, Logger),
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
        }

        private MappingManager Mappings { get; }

        [Fact]
        public void Creation()
        {
            SchemaManager TestObject = new SchemaManager(Mappings, Configuration, Logger);
            Assert.Equal(Mappings, TestObject.Mappings);
            Assert.Single(TestObject.Models);
            var TestModel = TestObject.Models.First();
            Assert.Equal("Default", TestModel.Source.Source.Name);
            Assert.Equal("TestDatabase", TestModel.SourceSpec.Name);
            Assert.Equal(2, TestModel.Source.Mappings.Count);
            Assert.NotNull(TestModel.SourceSpec);
            Assert.Equal(0, TestModel.SourceSpec.Functions.Count);
            Assert.Equal(0, TestModel.SourceSpec.StoredProcedures.Count);
            Assert.Equal(3, TestModel.SourceSpec.Tables.Count);
            Assert.Contains(TestModel.SourceSpec.Tables, x => x.Name == "AllReferencesAndID_");
            Assert.Contains(TestModel.SourceSpec.Tables, x => x.Name == "ManyToManyProperties_");
            Assert.Contains(TestModel.SourceSpec.Tables, x => x.Name == "AllReferencesAndID_ManyToManyProperties");
            Assert.Equal(0, TestModel.SourceSpec.Views.Count);
            Assert.Equal(6, TestModel.GeneratedSchemaChanges.Count());
            Assert.Contains("CREATE DATABASE [TestDatabase]", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[AllReferencesAndID_]([ID_] Int NOT NULL PRIMARY KEY IDENTITY,[BoolValue_] Bit NOT NULL,[ByteArrayValue_] VarBinary(100),[ByteValue_] TinyInt NOT NULL,[CharValue_] NChar NOT NULL,[DateTimeValue_] DateTime2 NOT NULL,[DecimalValue_] Decimal(18,0) NOT NULL,[DoubleValue_] Float NOT NULL,[FloatValue_] Real NOT NULL,[GuidValue_] UniqueIdentifier NOT NULL,[IntValue_] Int NOT NULL,[LongValue_] BigInt NOT NULL,[NullableBoolValue_] Bit,[NullableByteValue_] TinyInt,[NullableCharValue_] NChar,[NullableDateTimeValue_] DateTime2,[NullableDecimalValue_] Decimal(18,0),[NullableDoubleValue_] Float,[NullableFloatValue_] Real,[NullableGuidValue_] UniqueIdentifier,[NullableIntValue_] Int,[NullableLongValue_] BigInt,[NullableSByteValue_] TinyInt,[NullableShortValue_] SmallInt,[NullableTimeSpanValue_] DateTime,[NullableUIntValue_] Int,[NullableULongValue_] BigInt,[NullableUShortValue_] SmallInt,[SByteValue_] TinyInt NOT NULL,[ShortValue_] SmallInt NOT NULL,[StringValue1_] NVarChar(20),[StringValue2_] NVarChar(MAX),[TimeSpanValue_] DateTime NOT NULL,[UIntValue_] Int NOT NULL,[ULongValue_] BigInt NOT NULL,[UShortValue_] SmallInt NOT NULL)", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [dbo].[ManyToManyProperties_]([ID_] Int NOT NULL PRIMARY KEY IDENTITY,[BoolValue_] Bit NOT NULL)", TestModel.GeneratedSchemaChanges);
            Assert.Contains("ALTER TABLE [dbo].[AllReferencesAndID_ManyToManyProperties] ADD FOREIGN KEY ([ManyToManyProperties_ID_]) REFERENCES [dbo].[ManyToManyProperties_]([ID_]) ON DELETE CASCADE ON UPDATE CASCADE", TestModel.GeneratedSchemaChanges);
            Assert.Contains("ALTER TABLE [dbo].[AllReferencesAndID_ManyToManyProperties] ADD FOREIGN KEY ([AllReferencesAndID_ID_]) REFERENCES [dbo].[AllReferencesAndID_]([ID_]) ON DELETE CASCADE ON UPDATE CASCADE", TestModel.GeneratedSchemaChanges);
        }
    }
}