using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.Schema;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.SimpleTestWithDatabase;
using Serilog;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.Schema
{
    public class ModelManagerTests : TestingFixture
    {
        public ModelManagerTests()
        {
            Mappings = new MappingManager(new[] {
                new AllReferencesAndIDMappingWithDatabase()
            },
            new IDatabase[]{
                new TestDatabaseMapping()
            },
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
        }

        private MappingManager Mappings { get; }

        [Fact]
        public void Creation()
        {
            SchemaManager TestObject = new SchemaManager(Mappings, Configuration, Logger);
            Assert.Equal(Mappings, TestObject.Mappings);
            Assert.Equal(1, TestObject.Models.Count());
            var TestModel = TestObject.Models.First();
            Assert.Equal("Default", TestModel.Source.Source.Name);
            Assert.Equal("TestDatabase", TestModel.SourceSpec.Name);
            Assert.Equal(1, TestModel.Source.Mappings.Count);
            Assert.NotNull(TestModel.SourceSpec);
            Assert.Equal(0, TestModel.SourceSpec.Functions.Count);
            Assert.Equal(0, TestModel.SourceSpec.StoredProcedures.Count);
            Assert.Equal(1, TestModel.SourceSpec.Tables.Count);
            Assert.Equal("AllReferencesAndID_", TestModel.SourceSpec.Tables.First().Name);
            Assert.Equal(0, TestModel.SourceSpec.Views.Count);
            Assert.Equal(2, TestModel.GeneratedSchemaChanges.Count());
            Assert.Contains("CREATE DATABASE [TestDatabase]", TestModel.GeneratedSchemaChanges);
            Assert.Contains("CREATE TABLE [AllReferencesAndID_]([ID_] Int NOT NULL PRIMARY KEY IDENTITY,[BoolValue_] Bit NOT NULL,[ByteArrayValue_] VarBinary(100),[ByteValue_] TinyInt NOT NULL,[CharValue_] NChar NOT NULL,[DateTimeValue_] DateTime2 NOT NULL,[DecimalValue_] Decimal(18,0) NOT NULL,[DoubleValue_] Float NOT NULL,[FloatValue_] Real NOT NULL,[GuidValue_] UniqueIdentifier NOT NULL,[IntValue_] Int NOT NULL,[LongValue_] BigInt NOT NULL,[NullableBoolValue_] Bit,[NullableByteValue_] TinyInt,[NullableCharValue_] NChar,[NullableDateTimeValue_] DateTime2,[NullableDecimalValue_] Decimal(18,0),[NullableDoubleValue_] Float,[NullableFloatValue_] Real,[NullableGuidValue_] UniqueIdentifier,[NullableIntValue_] Int,[NullableLongValue_] BigInt,[NullableSByteValue_] Int,[NullableShortValue_] SmallInt,[NullableTimeSpanValue_] DateTime,[NullableUIntValue_] Int,[NullableULongValue_] Int,[NullableUShortValue_] Int,[SByteValue_] Int NOT NULL,[ShortValue_] SmallInt NOT NULL,[StringValue1_] NVarChar(20),[StringValue2_] NVarChar(MAX),[TimeSpanValue_] DateTime NOT NULL,[UIntValue_] Int NOT NULL,[ULongValue_] Int NOT NULL,[UShortValue_] Int NOT NULL)", TestModel.GeneratedSchemaChanges);
        }
    }
}