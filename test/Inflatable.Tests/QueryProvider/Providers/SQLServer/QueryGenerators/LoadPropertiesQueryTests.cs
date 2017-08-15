using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph;
using Inflatable.Tests.TestDatabases.ComplexGraph.Mappings;
using Inflatable.Tests.TestDatabases.MapProperties;
using Inflatable.Tests.TestDatabases.SimpleTestWithDatabase;
using Serilog;
using System.Data;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.QueryProvider.Providers.SQLServer.QueryGenerators
{
    public class LoadPropertiesQueryTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            },
                new MockDatabaseMapping(),
                new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration) }, Logger),
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
            var TestObject = new LoadPropertiesQuery<ConcreteClass1>(Mappings);
            Assert.Equal(typeof(ConcreteClass1), TestObject.AssociatedType);
            Assert.Same(Mappings, TestObject.MappingInformation);
            Assert.Equal(QueryType.LoadProperty, TestObject.QueryType);
        }

        [Fact]
        public void GenerateDeclarations()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>());
            var TestObject = new LoadPropertiesQuery<ConcreteClass1>(Mappings);
            var Result = TestObject.GenerateDeclarations();
            Assert.Equal(CommandType.Text, Result[0].DatabaseCommandType);
            Assert.Empty(Result[0].Parameters);
            Assert.Equal("", Result[0].QueryString);
            Assert.Equal(QueryType.LoadProperty, Result[0].QueryType);
        }

        [Fact]
        public void GenerateQuery()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>());
            var TestObject = new LoadPropertiesQuery<ConcreteClass1>(Mappings);
            var Result = TestObject.GenerateQueries(new ConcreteClass1 { ID = 10, BaseClassValue1 = 1, Value1 = 2 })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(0, Result.Parameters.Length);
            Assert.Equal("", Result.QueryString);
            Assert.Equal(QueryType.LoadProperty, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapProperties()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new AllReferencesAndIDMappingWithDatabase(),
                new MapPropertiesMapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>());
            Mappings.Mappings[typeof(MapProperties)].MapProperties.First().Setup(Mappings);
            var TestObject = new LoadPropertiesQuery<MapProperties>(Mappings);
            var Result = TestObject.GenerateQueries(new MapProperties { ID = 10, BoolValue = true }, "MappedClass")[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(1, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[AllReferencesAndID_].[ID_] AS [ID],[dbo].[AllReferencesAndID_].[BoolValue_] AS [BoolValue],[dbo].[AllReferencesAndID_].[ByteArrayValue_] AS [ByteArrayValue],[dbo].[AllReferencesAndID_].[ByteValue_] AS [ByteValue],[dbo].[AllReferencesAndID_].[CharValue_] AS [CharValue],[dbo].[AllReferencesAndID_].[DateTimeValue_] AS [DateTimeValue],[dbo].[AllReferencesAndID_].[DecimalValue_] AS [DecimalValue],[dbo].[AllReferencesAndID_].[DoubleValue_] AS [DoubleValue],[dbo].[AllReferencesAndID_].[FloatValue_] AS [FloatValue],[dbo].[AllReferencesAndID_].[GuidValue_] AS [GuidValue],[dbo].[AllReferencesAndID_].[IntValue_] AS [IntValue],[dbo].[AllReferencesAndID_].[LongValue_] AS [LongValue],[dbo].[AllReferencesAndID_].[NullableBoolValue_] AS [NullableBoolValue],[dbo].[AllReferencesAndID_].[NullableByteValue_] AS [NullableByteValue],[dbo].[AllReferencesAndID_].[NullableCharValue_] AS [NullableCharValue],[dbo].[AllReferencesAndID_].[NullableDateTimeValue_] AS [NullableDateTimeValue],[dbo].[AllReferencesAndID_].[NullableDecimalValue_] AS [NullableDecimalValue],[dbo].[AllReferencesAndID_].[NullableDoubleValue_] AS [NullableDoubleValue],[dbo].[AllReferencesAndID_].[NullableFloatValue_] AS [NullableFloatValue],[dbo].[AllReferencesAndID_].[NullableGuidValue_] AS [NullableGuidValue],[dbo].[AllReferencesAndID_].[NullableIntValue_] AS [NullableIntValue],[dbo].[AllReferencesAndID_].[NullableLongValue_] AS [NullableLongValue],[dbo].[AllReferencesAndID_].[NullableSByteValue_] AS [NullableSByteValue],[dbo].[AllReferencesAndID_].[NullableShortValue_] AS [NullableShortValue],[dbo].[AllReferencesAndID_].[NullableTimeSpanValue_] AS [NullableTimeSpanValue],[dbo].[AllReferencesAndID_].[NullableUIntValue_] AS [NullableUIntValue],[dbo].[AllReferencesAndID_].[NullableULongValue_] AS [NullableULongValue],[dbo].[AllReferencesAndID_].[NullableUShortValue_] AS [NullableUShortValue],[dbo].[AllReferencesAndID_].[SByteValue_] AS [SByteValue],[dbo].[AllReferencesAndID_].[ShortValue_] AS [ShortValue],[dbo].[AllReferencesAndID_].[StringValue1_] AS [StringValue1],[dbo].[AllReferencesAndID_].[StringValue2_] AS [StringValue2],[dbo].[AllReferencesAndID_].[TimeSpanValue_] AS [TimeSpanValue],[dbo].[AllReferencesAndID_].[UIntValue_] AS [UIntValue],[dbo].[AllReferencesAndID_].[ULongValue_] AS [ULongValue],[dbo].[AllReferencesAndID_].[UShortValue_] AS [UShortValue]\r\nFROM [dbo].[AllReferencesAndID_]\r\nINNER JOIN [dbo].[MapProperties_] ON [dbo].[MapProperties_].[AllReferencesAndID_MappedClass_ID_]=[dbo].[AllReferencesAndID_].[ID_]\r\nWHERE [dbo].[MapProperties_].[ID_]=@ID;", Result.QueryString);
            Assert.Equal(QueryType.LoadProperty, Result.QueryType);
        }
    }
}