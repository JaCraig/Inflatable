using BigBook;
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
using Inflatable.Tests.TestDatabases.ManyToManyProperties;
using Inflatable.Tests.TestDatabases.ManyToOneProperties;
using Inflatable.Tests.TestDatabases.ManyToOneProperties.Mappings;
using Inflatable.Tests.TestDatabases.MapProperties;
using Inflatable.Tests.TestDatabases.MapProperties.Mappings;
using Inflatable.Tests.TestDatabases.SimpleTest;
using Inflatable.Tests.TestDatabases.SimpleTestWithDatabase;
using Serilog;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Inflatable.Tests.QueryProvider.Providers.SQLServer.QueryGenerators
{
    public class LoadDataQueryTests : TestingFixture
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
                new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
            Canister.Builder.Bootstrapper.Resolve<ILogger>(),
            ObjectPool);
            var TestObject = new DataLoadQuery<ConcreteClass1>(Mappings, ObjectPool);
            Assert.Equal(typeof(ConcreteClass1), TestObject.AssociatedType);
            Assert.Same(Mappings, TestObject.MappingInformation);
            Assert.Equal(QueryType.LoadData, TestObject.QueryType);
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
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);
            var TestObject = new DataLoadQuery<ConcreteClass1>(Mappings, ObjectPool);
            var Result = TestObject.GenerateDeclarations();
            Assert.Equal(CommandType.Text, Result[0].DatabaseCommandType);
            Assert.Empty(Result[0].Parameters);
            Assert.Equal("", Result[0].QueryString);
            Assert.Equal(QueryType.LoadData, Result[0].QueryType);
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
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);
            var TestObject = new DataLoadQuery<ConcreteClass1>(Mappings, ObjectPool);
            var Result = TestObject.GenerateQueries(new List<Dynamo>() { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Empty(Result.Parameters);
            Assert.Equal("", Result.QueryString);
            Assert.Equal(QueryType.LoadProperty, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithManyToManyProperties()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new AllReferencesAndIDMappingWithDatabase(),
                new ManyToManyPropertiesMapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);
            var ManyToManyProperty = Mappings.Mappings[typeof(ManyToManyProperties)].ManyToManyProperties[0];
            var TempDataModel = new Inflatable.Schema.DataModel(Mappings, Configuration, Logger, DataModeler, Sherlock, Helper);
            ManyToManyProperty.Setup(Mappings, TempDataModel.SourceSpec);
            var TestObject = new DataLoadQuery<ManyToManyProperties>(Mappings, ObjectPool);
            var Result = TestObject.GenerateQueries(new List<Dynamo>() { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[AllReferencesAndID_].[ID_] AS [ID],[dbo].[AllReferencesAndID_].[BoolValue_] AS [BoolValue],[dbo].[AllReferencesAndID_].[ByteArrayValue_] AS [ByteArrayValue],[dbo].[AllReferencesAndID_].[ByteValue_] AS [ByteValue],[dbo].[AllReferencesAndID_].[CharValue_] AS [CharValue],[dbo].[AllReferencesAndID_].[DateTimeValue_] AS [DateTimeValue],[dbo].[AllReferencesAndID_].[DecimalValue_] AS [DecimalValue],[dbo].[AllReferencesAndID_].[DoubleValue_] AS [DoubleValue],[dbo].[AllReferencesAndID_].[FloatValue_] AS [FloatValue],[dbo].[AllReferencesAndID_].[GuidValue_] AS [GuidValue],[dbo].[AllReferencesAndID_].[IntValue_] AS [IntValue],[dbo].[AllReferencesAndID_].[LongValue_] AS [LongValue],[dbo].[AllReferencesAndID_].[NullableBoolValue_] AS [NullableBoolValue],[dbo].[AllReferencesAndID_].[NullableByteValue_] AS [NullableByteValue],[dbo].[AllReferencesAndID_].[NullableCharValue_] AS [NullableCharValue],[dbo].[AllReferencesAndID_].[NullableDateTimeValue_] AS [NullableDateTimeValue],[dbo].[AllReferencesAndID_].[NullableDecimalValue_] AS [NullableDecimalValue],[dbo].[AllReferencesAndID_].[NullableDoubleValue_] AS [NullableDoubleValue],[dbo].[AllReferencesAndID_].[NullableFloatValue_] AS [NullableFloatValue],[dbo].[AllReferencesAndID_].[NullableGuidValue_] AS [NullableGuidValue],[dbo].[AllReferencesAndID_].[NullableIntValue_] AS [NullableIntValue],[dbo].[AllReferencesAndID_].[NullableLongValue_] AS [NullableLongValue],[dbo].[AllReferencesAndID_].[NullableSByteValue_] AS [NullableSByteValue],[dbo].[AllReferencesAndID_].[NullableShortValue_] AS [NullableShortValue],[dbo].[AllReferencesAndID_].[NullableTimeSpanValue_] AS [NullableTimeSpanValue],[dbo].[AllReferencesAndID_].[NullableUIntValue_] AS [NullableUIntValue],[dbo].[AllReferencesAndID_].[NullableULongValue_] AS [NullableULongValue],[dbo].[AllReferencesAndID_].[NullableUShortValue_] AS [NullableUShortValue],[dbo].[AllReferencesAndID_].[SByteValue_] AS [SByteValue],[dbo].[AllReferencesAndID_].[ShortValue_] AS [ShortValue],[dbo].[AllReferencesAndID_].[StringValue1_] AS [StringValue1],[dbo].[AllReferencesAndID_].[StringValue2_] AS [StringValue2],[dbo].[AllReferencesAndID_].[TimeSpanValue_] AS [TimeSpanValue],[dbo].[AllReferencesAndID_].[UIntValue_] AS [UIntValue],[dbo].[AllReferencesAndID_].[ULongValue_] AS [ULongValue],[dbo].[AllReferencesAndID_].[UShortValue_] AS [UShortValue],[dbo].[AllReferencesAndID_].[UriValue_] AS [UriValue]\r\nFROM [dbo].[AllReferencesAndID_]\r\nINNER JOIN [dbo].[AllReferencesAndID_ManyToManyProperties] ON [dbo].[AllReferencesAndID_ManyToManyProperties].[AllReferencesAndID_ID_]=[dbo].[AllReferencesAndID_].[ID_]\r\nWHERE [dbo].[AllReferencesAndID_ManyToManyProperties].[ManyToManyProperties_ID_]=@ID;", Result.QueryString);
            Assert.Equal(QueryType.LoadProperty, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithManyToOneManyFromComplexGraphProperties()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new ManyToOneManyFromComplexClassMapping(),
                new IManyToOneManyMapping(),
                new AllReferencesAndIDMappingWithDatabase()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);

            var ManyToOneManyProperty = Mappings.Mappings[typeof(ManyToOneManyFromComplexClass)].ManyToOneProperties[0];
            var TempDataModel = new Inflatable.Schema.DataModel(Mappings, Configuration, Logger, DataModeler, Sherlock, Helper);
            ManyToOneManyProperty.Setup(Mappings, TempDataModel.SourceSpec);

            var TestObject = new DataLoadQuery<ManyToOneManyFromComplexClass>(Mappings, ObjectPool);
            var TempManyToOneMany = new ManyToOneManyFromComplexClass { ID = 10, BoolValue = true };
            TempManyToOneMany.ManyToOneClass.Add(new AllReferencesAndID { ID = 1 });
            TempManyToOneMany.ManyToOneClass.Add(new AllReferencesAndID { ID = 2 });

            var Result = TestObject.GenerateQueries(new List<Dynamo>() { new Dynamo(new { ID = 10 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[AllReferencesAndID_].[ID_] AS [ID],[dbo].[AllReferencesAndID_].[BoolValue_] AS [BoolValue],[dbo].[AllReferencesAndID_].[ByteArrayValue_] AS [ByteArrayValue],[dbo].[AllReferencesAndID_].[ByteValue_] AS [ByteValue],[dbo].[AllReferencesAndID_].[CharValue_] AS [CharValue],[dbo].[AllReferencesAndID_].[DateTimeValue_] AS [DateTimeValue],[dbo].[AllReferencesAndID_].[DecimalValue_] AS [DecimalValue],[dbo].[AllReferencesAndID_].[DoubleValue_] AS [DoubleValue],[dbo].[AllReferencesAndID_].[FloatValue_] AS [FloatValue],[dbo].[AllReferencesAndID_].[GuidValue_] AS [GuidValue],[dbo].[AllReferencesAndID_].[IntValue_] AS [IntValue],[dbo].[AllReferencesAndID_].[LongValue_] AS [LongValue],[dbo].[AllReferencesAndID_].[NullableBoolValue_] AS [NullableBoolValue],[dbo].[AllReferencesAndID_].[NullableByteValue_] AS [NullableByteValue],[dbo].[AllReferencesAndID_].[NullableCharValue_] AS [NullableCharValue],[dbo].[AllReferencesAndID_].[NullableDateTimeValue_] AS [NullableDateTimeValue],[dbo].[AllReferencesAndID_].[NullableDecimalValue_] AS [NullableDecimalValue],[dbo].[AllReferencesAndID_].[NullableDoubleValue_] AS [NullableDoubleValue],[dbo].[AllReferencesAndID_].[NullableFloatValue_] AS [NullableFloatValue],[dbo].[AllReferencesAndID_].[NullableGuidValue_] AS [NullableGuidValue],[dbo].[AllReferencesAndID_].[NullableIntValue_] AS [NullableIntValue],[dbo].[AllReferencesAndID_].[NullableLongValue_] AS [NullableLongValue],[dbo].[AllReferencesAndID_].[NullableSByteValue_] AS [NullableSByteValue],[dbo].[AllReferencesAndID_].[NullableShortValue_] AS [NullableShortValue],[dbo].[AllReferencesAndID_].[NullableTimeSpanValue_] AS [NullableTimeSpanValue],[dbo].[AllReferencesAndID_].[NullableUIntValue_] AS [NullableUIntValue],[dbo].[AllReferencesAndID_].[NullableULongValue_] AS [NullableULongValue],[dbo].[AllReferencesAndID_].[NullableUShortValue_] AS [NullableUShortValue],[dbo].[AllReferencesAndID_].[SByteValue_] AS [SByteValue],[dbo].[AllReferencesAndID_].[ShortValue_] AS [ShortValue],[dbo].[AllReferencesAndID_].[StringValue1_] AS [StringValue1],[dbo].[AllReferencesAndID_].[StringValue2_] AS [StringValue2],[dbo].[AllReferencesAndID_].[TimeSpanValue_] AS [TimeSpanValue],[dbo].[AllReferencesAndID_].[UIntValue_] AS [UIntValue],[dbo].[AllReferencesAndID_].[ULongValue_] AS [ULongValue],[dbo].[AllReferencesAndID_].[UShortValue_] AS [UShortValue],[dbo].[AllReferencesAndID_].[UriValue_] AS [UriValue]\r\nFROM [dbo].[AllReferencesAndID_]\r\nWHERE [dbo].[AllReferencesAndID_].[IManyToOneMany_ID_]=@ID;", Result.QueryString);
            Assert.Equal(QueryType.LoadProperty, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithManyToOneManyProperties()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new ManyToOneManyPropertiesMapping(),
                new ManyToOneOnePropertiesMapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);

            var ManyToOneManyProperty = Mappings.Mappings[typeof(ManyToOneManyProperties)].ManyToOneProperties[0];
            var TempDataModel = new Inflatable.Schema.DataModel(Mappings, Configuration, Logger, DataModeler, Sherlock, Helper);
            ManyToOneManyProperty.Setup(Mappings, TempDataModel.SourceSpec);

            var TestObject = new DataLoadQuery<ManyToOneManyProperties>(Mappings, ObjectPool);
            var TempManyToOneMany = new ManyToOneManyProperties { ID = 10, BoolValue = true };
            TempManyToOneMany.ManyToOneClass.Add(new ManyToOneOneProperties { ID = 1 });
            TempManyToOneMany.ManyToOneClass.Add(new ManyToOneOneProperties { ID = 2 });

            var Result = TestObject.GenerateQueries(new List<Dynamo>() { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[ManyToOneOneProperties_].[ID_] AS [ID],[dbo].[ManyToOneOneProperties_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToOneOneProperties_]\r\nWHERE [dbo].[ManyToOneOneProperties_].[ManyToOneManyProperties_ID_]=@ID;", Result.QueryString);
            Assert.Equal(QueryType.LoadProperty, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithManyToOneOneFromComplexGraphProperties()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new ManyToOneOneFromComplexClassMapping(),
                new IManyToOneOneMapping(),
                new AllReferencesAndIDMappingWithDatabase()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);

            var ManyToOneManyProperty = Mappings.Mappings[typeof(ManyToOneOneFromComplexClass)].ManyToOneProperties[0];
            var TempDataModel = new Inflatable.Schema.DataModel(Mappings, Configuration, Logger, DataModeler, Sherlock, Helper);
            ManyToOneManyProperty.Setup(Mappings, TempDataModel.SourceSpec);

            var TestObject = new DataLoadQuery<ManyToOneOneFromComplexClass>(Mappings, ObjectPool);
            var TempManyToOneMany = new ManyToOneOneFromComplexClass { ID = 10, BoolValue = true };
            TempManyToOneMany.ManyToOneClass = new AllReferencesAndID { ID = 1 };

            var Result = TestObject.GenerateQueries(new List<Dynamo>() { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[AllReferencesAndID_].[ID_] AS [ID],[dbo].[AllReferencesAndID_].[BoolValue_] AS [BoolValue],[dbo].[AllReferencesAndID_].[ByteArrayValue_] AS [ByteArrayValue],[dbo].[AllReferencesAndID_].[ByteValue_] AS [ByteValue],[dbo].[AllReferencesAndID_].[CharValue_] AS [CharValue],[dbo].[AllReferencesAndID_].[DateTimeValue_] AS [DateTimeValue],[dbo].[AllReferencesAndID_].[DecimalValue_] AS [DecimalValue],[dbo].[AllReferencesAndID_].[DoubleValue_] AS [DoubleValue],[dbo].[AllReferencesAndID_].[FloatValue_] AS [FloatValue],[dbo].[AllReferencesAndID_].[GuidValue_] AS [GuidValue],[dbo].[AllReferencesAndID_].[IntValue_] AS [IntValue],[dbo].[AllReferencesAndID_].[LongValue_] AS [LongValue],[dbo].[AllReferencesAndID_].[NullableBoolValue_] AS [NullableBoolValue],[dbo].[AllReferencesAndID_].[NullableByteValue_] AS [NullableByteValue],[dbo].[AllReferencesAndID_].[NullableCharValue_] AS [NullableCharValue],[dbo].[AllReferencesAndID_].[NullableDateTimeValue_] AS [NullableDateTimeValue],[dbo].[AllReferencesAndID_].[NullableDecimalValue_] AS [NullableDecimalValue],[dbo].[AllReferencesAndID_].[NullableDoubleValue_] AS [NullableDoubleValue],[dbo].[AllReferencesAndID_].[NullableFloatValue_] AS [NullableFloatValue],[dbo].[AllReferencesAndID_].[NullableGuidValue_] AS [NullableGuidValue],[dbo].[AllReferencesAndID_].[NullableIntValue_] AS [NullableIntValue],[dbo].[AllReferencesAndID_].[NullableLongValue_] AS [NullableLongValue],[dbo].[AllReferencesAndID_].[NullableSByteValue_] AS [NullableSByteValue],[dbo].[AllReferencesAndID_].[NullableShortValue_] AS [NullableShortValue],[dbo].[AllReferencesAndID_].[NullableTimeSpanValue_] AS [NullableTimeSpanValue],[dbo].[AllReferencesAndID_].[NullableUIntValue_] AS [NullableUIntValue],[dbo].[AllReferencesAndID_].[NullableULongValue_] AS [NullableULongValue],[dbo].[AllReferencesAndID_].[NullableUShortValue_] AS [NullableUShortValue],[dbo].[AllReferencesAndID_].[SByteValue_] AS [SByteValue],[dbo].[AllReferencesAndID_].[ShortValue_] AS [ShortValue],[dbo].[AllReferencesAndID_].[StringValue1_] AS [StringValue1],[dbo].[AllReferencesAndID_].[StringValue2_] AS [StringValue2],[dbo].[AllReferencesAndID_].[TimeSpanValue_] AS [TimeSpanValue],[dbo].[AllReferencesAndID_].[UIntValue_] AS [UIntValue],[dbo].[AllReferencesAndID_].[ULongValue_] AS [ULongValue],[dbo].[AllReferencesAndID_].[UShortValue_] AS [UShortValue],[dbo].[AllReferencesAndID_].[UriValue_] AS [UriValue]\r\nFROM [dbo].[AllReferencesAndID_]\r\nINNER JOIN [dbo].[IManyToOneOne_] as [IManyToOneOne_2] ON [IManyToOneOne_2].[AllReferencesAndID_ID_]=[dbo].[AllReferencesAndID_].[ID_]\r\nWHERE [IManyToOneOne_2].[ID_]=@ID;", Result.QueryString);
            Assert.Equal(QueryType.LoadProperty, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithManyToOneSingleProperties()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new ManyToOneOnePropertiesMapping(),
                new ManyToOneManyPropertiesMapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);

            var ManyToOneOneProperty = Mappings.Mappings[typeof(ManyToOneOneProperties)].ManyToOneProperties[0];
            var TempDataModel = new Inflatable.Schema.DataModel(Mappings, Configuration, Logger, DataModeler, Sherlock, Helper);
            ManyToOneOneProperty.Setup(Mappings, TempDataModel.SourceSpec);

            var TestObject = new DataLoadQuery<ManyToOneOneProperties>(Mappings, ObjectPool);
            var TempManyToOneOne = new ManyToOneOneProperties { ID = 10, BoolValue = true };
            TempManyToOneOne.ManyToOneClass = new ManyToOneManyProperties { ID = 1 };

            var Result = TestObject.GenerateQueries(new List<Dynamo>() { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[ManyToOneManyProperties_].[ID_] AS [ID],[dbo].[ManyToOneManyProperties_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToOneManyProperties_]\r\nINNER JOIN [dbo].[ManyToOneOneProperties_] as [ManyToOneOneProperties_2] ON [ManyToOneOneProperties_2].[ManyToOneManyProperties_ID_]=[dbo].[ManyToOneManyProperties_].[ID_]\r\nWHERE [ManyToOneOneProperties_2].[ID_]=@ID;", Result.QueryString);
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
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);
            var MapProperty = Mappings.Mappings[typeof(MapProperties)].MapProperties[0];
            MapProperty.Setup(Mappings);
            var TestObject = new DataLoadQuery<MapProperties>(Mappings, ObjectPool);
            var Result = TestObject.GenerateQueries(new List<Dynamo>() { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[AllReferencesAndID_].[ID_] AS [ID],[dbo].[AllReferencesAndID_].[BoolValue_] AS [BoolValue],[dbo].[AllReferencesAndID_].[ByteArrayValue_] AS [ByteArrayValue],[dbo].[AllReferencesAndID_].[ByteValue_] AS [ByteValue],[dbo].[AllReferencesAndID_].[CharValue_] AS [CharValue],[dbo].[AllReferencesAndID_].[DateTimeValue_] AS [DateTimeValue],[dbo].[AllReferencesAndID_].[DecimalValue_] AS [DecimalValue],[dbo].[AllReferencesAndID_].[DoubleValue_] AS [DoubleValue],[dbo].[AllReferencesAndID_].[FloatValue_] AS [FloatValue],[dbo].[AllReferencesAndID_].[GuidValue_] AS [GuidValue],[dbo].[AllReferencesAndID_].[IntValue_] AS [IntValue],[dbo].[AllReferencesAndID_].[LongValue_] AS [LongValue],[dbo].[AllReferencesAndID_].[NullableBoolValue_] AS [NullableBoolValue],[dbo].[AllReferencesAndID_].[NullableByteValue_] AS [NullableByteValue],[dbo].[AllReferencesAndID_].[NullableCharValue_] AS [NullableCharValue],[dbo].[AllReferencesAndID_].[NullableDateTimeValue_] AS [NullableDateTimeValue],[dbo].[AllReferencesAndID_].[NullableDecimalValue_] AS [NullableDecimalValue],[dbo].[AllReferencesAndID_].[NullableDoubleValue_] AS [NullableDoubleValue],[dbo].[AllReferencesAndID_].[NullableFloatValue_] AS [NullableFloatValue],[dbo].[AllReferencesAndID_].[NullableGuidValue_] AS [NullableGuidValue],[dbo].[AllReferencesAndID_].[NullableIntValue_] AS [NullableIntValue],[dbo].[AllReferencesAndID_].[NullableLongValue_] AS [NullableLongValue],[dbo].[AllReferencesAndID_].[NullableSByteValue_] AS [NullableSByteValue],[dbo].[AllReferencesAndID_].[NullableShortValue_] AS [NullableShortValue],[dbo].[AllReferencesAndID_].[NullableTimeSpanValue_] AS [NullableTimeSpanValue],[dbo].[AllReferencesAndID_].[NullableUIntValue_] AS [NullableUIntValue],[dbo].[AllReferencesAndID_].[NullableULongValue_] AS [NullableULongValue],[dbo].[AllReferencesAndID_].[NullableUShortValue_] AS [NullableUShortValue],[dbo].[AllReferencesAndID_].[SByteValue_] AS [SByteValue],[dbo].[AllReferencesAndID_].[ShortValue_] AS [ShortValue],[dbo].[AllReferencesAndID_].[StringValue1_] AS [StringValue1],[dbo].[AllReferencesAndID_].[StringValue2_] AS [StringValue2],[dbo].[AllReferencesAndID_].[TimeSpanValue_] AS [TimeSpanValue],[dbo].[AllReferencesAndID_].[UIntValue_] AS [UIntValue],[dbo].[AllReferencesAndID_].[ULongValue_] AS [ULongValue],[dbo].[AllReferencesAndID_].[UShortValue_] AS [UShortValue],[dbo].[AllReferencesAndID_].[UriValue_] AS [UriValue]\r\nFROM [dbo].[AllReferencesAndID_]\r\nINNER JOIN [dbo].[MapProperties_] as [MapProperties_2] ON [MapProperties_2].[AllReferencesAndID_MappedClass_ID_]=[dbo].[AllReferencesAndID_].[ID_]\r\nWHERE [MapProperties_2].[ID_]=@ID;", Result.QueryString);
            Assert.Equal(QueryType.LoadProperty, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapPropertiesComplex()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new AllReferencesAndIDMappingWithDatabase(),
                new MapPropertiesFromComplexClassMapping(),
                new IMapPropertiesInterfaceMapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);
            var MapProperty = Mappings.Mappings[typeof(MapPropertiesFromComplexClass)].MapProperties[0];
            MapProperty.Setup(Mappings);
            var TestObject = new DataLoadQuery<MapPropertiesFromComplexClass>(Mappings, ObjectPool);
            var Result = TestObject.GenerateQueries(new List<Dynamo>() { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[AllReferencesAndID_].[ID_] AS [ID],[dbo].[AllReferencesAndID_].[BoolValue_] AS [BoolValue],[dbo].[AllReferencesAndID_].[ByteArrayValue_] AS [ByteArrayValue],[dbo].[AllReferencesAndID_].[ByteValue_] AS [ByteValue],[dbo].[AllReferencesAndID_].[CharValue_] AS [CharValue],[dbo].[AllReferencesAndID_].[DateTimeValue_] AS [DateTimeValue],[dbo].[AllReferencesAndID_].[DecimalValue_] AS [DecimalValue],[dbo].[AllReferencesAndID_].[DoubleValue_] AS [DoubleValue],[dbo].[AllReferencesAndID_].[FloatValue_] AS [FloatValue],[dbo].[AllReferencesAndID_].[GuidValue_] AS [GuidValue],[dbo].[AllReferencesAndID_].[IntValue_] AS [IntValue],[dbo].[AllReferencesAndID_].[LongValue_] AS [LongValue],[dbo].[AllReferencesAndID_].[NullableBoolValue_] AS [NullableBoolValue],[dbo].[AllReferencesAndID_].[NullableByteValue_] AS [NullableByteValue],[dbo].[AllReferencesAndID_].[NullableCharValue_] AS [NullableCharValue],[dbo].[AllReferencesAndID_].[NullableDateTimeValue_] AS [NullableDateTimeValue],[dbo].[AllReferencesAndID_].[NullableDecimalValue_] AS [NullableDecimalValue],[dbo].[AllReferencesAndID_].[NullableDoubleValue_] AS [NullableDoubleValue],[dbo].[AllReferencesAndID_].[NullableFloatValue_] AS [NullableFloatValue],[dbo].[AllReferencesAndID_].[NullableGuidValue_] AS [NullableGuidValue],[dbo].[AllReferencesAndID_].[NullableIntValue_] AS [NullableIntValue],[dbo].[AllReferencesAndID_].[NullableLongValue_] AS [NullableLongValue],[dbo].[AllReferencesAndID_].[NullableSByteValue_] AS [NullableSByteValue],[dbo].[AllReferencesAndID_].[NullableShortValue_] AS [NullableShortValue],[dbo].[AllReferencesAndID_].[NullableTimeSpanValue_] AS [NullableTimeSpanValue],[dbo].[AllReferencesAndID_].[NullableUIntValue_] AS [NullableUIntValue],[dbo].[AllReferencesAndID_].[NullableULongValue_] AS [NullableULongValue],[dbo].[AllReferencesAndID_].[NullableUShortValue_] AS [NullableUShortValue],[dbo].[AllReferencesAndID_].[SByteValue_] AS [SByteValue],[dbo].[AllReferencesAndID_].[ShortValue_] AS [ShortValue],[dbo].[AllReferencesAndID_].[StringValue1_] AS [StringValue1],[dbo].[AllReferencesAndID_].[StringValue2_] AS [StringValue2],[dbo].[AllReferencesAndID_].[TimeSpanValue_] AS [TimeSpanValue],[dbo].[AllReferencesAndID_].[UIntValue_] AS [UIntValue],[dbo].[AllReferencesAndID_].[ULongValue_] AS [ULongValue],[dbo].[AllReferencesAndID_].[UShortValue_] AS [UShortValue],[dbo].[AllReferencesAndID_].[UriValue_] AS [UriValue]\r\nFROM [dbo].[AllReferencesAndID_]\r\nINNER JOIN [dbo].[MapPropertiesFromComplexClass_] as [MapPropertiesFromComplexClass_2] ON [MapPropertiesFromComplexClass_2].[AllReferencesAndID_MappedClass_ID_]=[dbo].[AllReferencesAndID_].[ID_]\r\nINNER JOIN [dbo].[IMapPropertiesInterface_] AS [IMapPropertiesInterface_2] ON [MapPropertiesFromComplexClass_2].[IMapPropertiesInterface_ID_]=[IMapPropertiesInterface_2].[ID_]\r\nWHERE [IMapPropertiesInterface_2].[ID_]=@ID;", Result.QueryString);
            Assert.Equal(QueryType.LoadProperty, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapToSelf()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new MapPropertyReferencesSelfMapping(),
                new IMapPropertiesInterfaceMapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);
            var MapProperty = Mappings.Mappings[typeof(MapPropertyReferencesSelf)].MapProperties[0];
            MapProperty.Setup(Mappings);
            var TestObject = new DataLoadQuery<MapPropertyReferencesSelf>(Mappings, ObjectPool);
            var Result = TestObject.GenerateQueries(new List<Dynamo>() { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IMapPropertiesInterface_].[ID_] AS [ID],[dbo].[IMapPropertiesInterface_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[MapPropertyReferencesSelf_]\r\nINNER JOIN [dbo].[IMapPropertiesInterface_] ON [dbo].[MapPropertyReferencesSelf_].[IMapPropertiesInterface_ID_]=[dbo].[IMapPropertiesInterface_].[ID_]\r\nINNER JOIN [dbo].[MapPropertyReferencesSelf_] as [MapPropertyReferencesSelf_2] ON [MapPropertyReferencesSelf_2].[IMapPropertiesInterface_MappedClass_ID_]=[dbo].[IMapPropertiesInterface_].[ID_]\r\nINNER JOIN [dbo].[IMapPropertiesInterface_] AS [IMapPropertiesInterface_2] ON [MapPropertyReferencesSelf_2].[IMapPropertiesInterface_ID_]=[IMapPropertiesInterface_2].[ID_]\r\nWHERE [IMapPropertiesInterface_2].[ID_]=@ID;", Result.QueryString);
            Assert.Equal(QueryType.LoadProperty, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapToSelfOnInterface()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new MapPropertiesWithMapOnInterfaceMapping(),
                new IMapPropertiesInterfaceWithMapMapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);
            var MapProperty = Mappings.Mappings[typeof(IMapPropertiesInterfaceWithMap)].MapProperties[0];
            MapProperty.Setup(Mappings);
            var TestObject = new DataLoadQuery<MapPropertiesWithMapOnInterface>(Mappings, ObjectPool);
            var Result = TestObject.GenerateQueries(new List<Dynamo>() { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IMapPropertiesInterfaceWithMap_].[ID_] AS [ID],[dbo].[MapPropertiesWithMapOnInterface_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[MapPropertiesWithMapOnInterface_]\r\nINNER JOIN [dbo].[IMapPropertiesInterfaceWithMap_] ON [dbo].[MapPropertiesWithMapOnInterface_].[IMapPropertiesInterfaceWithMap_ID_]=[dbo].[IMapPropertiesInterfaceWithMap_].[ID_]\r\nINNER JOIN [dbo].[IMapPropertiesInterfaceWithMap_] as [IMapPropertiesInterfaceWithMap_2] ON [IMapPropertiesInterfaceWithMap_2].[IMapPropertiesInterfaceWithMap_MappedClass_ID_]=[dbo].[IMapPropertiesInterfaceWithMap_].[ID_]\r\nWHERE [IMapPropertiesInterfaceWithMap_2].[ID_]=@ID;", Result.QueryString);
            Assert.Equal(QueryType.LoadProperty, Result.QueryType);
        }
    }
}