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
            var Result = TestObject.GenerateQueries(new Dynamo[] { DynamoFactory.Create(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("SELECT [dbo].[IInterface1_].[ID_] AS [ID],[dbo].[BaseClass1_].[BaseClassValue1_] AS [BaseClassValue1],[dbo].[ConcreteClass1_].[Value1_] AS [Value1]\r\nFROM [dbo].[ConcreteClass1_]\r\nINNER JOIN [dbo].[BaseClass1_] ON [dbo].[ConcreteClass1_].[BaseClass1_ID_]=[dbo].[BaseClass1_].[ID_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nWHERE ([dbo].[IInterface1_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
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
            var Result = TestObject.GenerateQueries(new Dynamo[] { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[ManyToManyProperties_].[ID_] AS [ID],[dbo].[ManyToManyProperties_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToManyProperties_]\r\nWHERE ([dbo].[ManyToManyProperties_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
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

            var Result = TestObject.GenerateQueries(new Dynamo[] { new Dynamo(new { ID = 10 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IManyToOneMany_].[ID_] AS [ID],[dbo].[IManyToOneMany_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToOneManyFromComplexClass_]\r\nINNER JOIN [dbo].[IManyToOneMany_] ON [dbo].[ManyToOneManyFromComplexClass_].[IManyToOneMany_ID_]=[dbo].[IManyToOneMany_].[ID_]\r\nWHERE ([dbo].[IManyToOneMany_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
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

            var Result = TestObject.GenerateQueries(new Dynamo[] { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[ManyToOneManyProperties_].[ID_] AS [ID],[dbo].[ManyToOneManyProperties_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToOneManyProperties_]\r\nWHERE ([dbo].[ManyToOneManyProperties_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
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

            var Result = TestObject.GenerateQueries(new Dynamo[] { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IManyToOneOne_].[ID_] AS [ID],[dbo].[IManyToOneOne_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToOneOneFromComplexClass_]\r\nINNER JOIN [dbo].[IManyToOneOne_] ON [dbo].[ManyToOneOneFromComplexClass_].[IManyToOneOne_ID_]=[dbo].[IManyToOneOne_].[ID_]\r\nWHERE ([dbo].[IManyToOneOne_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
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

            var Result = TestObject.GenerateQueries(new Dynamo[] { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[ManyToOneOneProperties_].[ID_] AS [ID],[dbo].[ManyToOneOneProperties_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToOneOneProperties_]\r\nWHERE ([dbo].[ManyToOneOneProperties_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
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
            var Result = TestObject.GenerateQueries(new Dynamo[] { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[MapProperties_].[ID_] AS [ID],[dbo].[MapProperties_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[MapProperties_]\r\nWHERE ([dbo].[MapProperties_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
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
            var Result = TestObject.GenerateQueries(new Dynamo[] { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IMapPropertiesInterface_].[ID_] AS [ID],[dbo].[IMapPropertiesInterface_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[MapPropertiesFromComplexClass_]\r\nINNER JOIN [dbo].[IMapPropertiesInterface_] ON [dbo].[MapPropertiesFromComplexClass_].[IMapPropertiesInterface_ID_]=[dbo].[IMapPropertiesInterface_].[ID_]\r\nWHERE ([dbo].[IMapPropertiesInterface_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
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
            var Result = TestObject.GenerateQueries(new Dynamo[] { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IMapPropertiesInterface_].[ID_] AS [ID],[dbo].[IMapPropertiesInterface_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[MapPropertyReferencesSelf_]\r\nINNER JOIN [dbo].[IMapPropertiesInterface_] ON [dbo].[MapPropertyReferencesSelf_].[IMapPropertiesInterface_ID_]=[dbo].[IMapPropertiesInterface_].[ID_]\r\nWHERE ([dbo].[IMapPropertiesInterface_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
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
            var Result = TestObject.GenerateQueries(new Dynamo[] { new Dynamo(new { ID = 1 }) })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IMapPropertiesInterfaceWithMap_].[ID_] AS [ID],[dbo].[MapPropertiesWithMapOnInterface_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[MapPropertiesWithMapOnInterface_]\r\nINNER JOIN [dbo].[IMapPropertiesInterfaceWithMap_] ON [dbo].[MapPropertiesWithMapOnInterface_].[IMapPropertiesInterfaceWithMap_ID_]=[dbo].[IMapPropertiesInterfaceWithMap_].[ID_]\r\nWHERE ([dbo].[IMapPropertiesInterfaceWithMap_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
        }
    }
}