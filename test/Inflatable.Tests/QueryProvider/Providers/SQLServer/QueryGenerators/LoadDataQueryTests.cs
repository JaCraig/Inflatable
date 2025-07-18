﻿using Inflatable.ClassMapper;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Enums;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.QueryProvider.Providers.SQLServer.QueryGenerators;
using Inflatable.Schema;
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
using System.Data;
using Xunit;

namespace Inflatable.Tests.QueryProvider.Providers.SQLServer.QueryGenerators
{
    [Collection("Test collection")]
    public class LoadDataQueryTests : TestingFixture
    {
        public LoadDataQueryTests(SetupFixture setupFixture)
            : base(setupFixture) { }

        [Fact]
        public void Creation()
        {
            var Mappings = new MappingSource([
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            ],
                new MockDatabaseMapping(),
                new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
            GetLogger<MappingSource>(),
            ObjectPool);
            var TestObject = new DataLoadQuery<ConcreteClass1>(Mappings, ObjectPool);
            Assert.Equal(typeof(ConcreteClass1), TestObject.AssociatedType);
            Assert.Same(Mappings, TestObject.MappingInformation);
            Assert.Equal(QueryType.LoadData, TestObject.QueryType);
        }

        [Fact]
        public void GenerateDeclarations()
        {
            var Mappings = new MappingSource([
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);
            var TestObject = new DataLoadQuery<ConcreteClass1>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery[] Result = TestObject.GenerateDeclarations();
            Assert.Empty(Result);
        }

        [Fact]
        public void GenerateQuery()
        {
            var Mappings = new MappingSource([
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);
            var TestObject = new DataLoadQuery<ConcreteClass1>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries([new(new { ID = 1 })])[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            _ = Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("SELECT [dbo].[IInterface1_].[ID_] AS [ID],[dbo].[BaseClass1_].[BaseClassValue1_] AS [BaseClassValue1],[dbo].[ConcreteClass1_].[Value1_] AS [Value1]\r\nFROM [dbo].[ConcreteClass1_]\r\nINNER JOIN [dbo].[BaseClass1_] ON [dbo].[ConcreteClass1_].[BaseClass1_ID_]=[dbo].[BaseClass1_].[ID_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nWHERE ([dbo].[IInterface1_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithManyToManyProperties()
        {
            var Mappings = new MappingSource([
                new AllReferencesAndIDMappingWithDatabase(),
                new ManyToManyPropertiesMapping()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);
            Inflatable.ClassMapper.Interfaces.IManyToManyProperty ManyToManyProperty = Mappings.Mappings[typeof(ManyToManyProperties)].ManyToManyProperties[0];
            var TempDataModel = new DataModel(Mappings, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ManyToManyProperty.Setup(Mappings, TempDataModel.SourceSpec);
            var TestObject = new DataLoadQuery<ManyToManyProperties>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries([new(new { ID = 1 })])[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            _ = Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[ManyToManyProperties_].[ID_] AS [ID],[dbo].[ManyToManyProperties_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToManyProperties_]\r\nWHERE ([dbo].[ManyToManyProperties_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithManyToOneManyFromComplexGraphProperties()
        {
            var Mappings = new MappingSource([
                new ManyToOneManyFromComplexClassMapping(),
                new IManyToOneManyMapping(),
                new AllReferencesAndIDMappingWithDatabase()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);

            Inflatable.ClassMapper.Interfaces.IManyToOneProperty ManyToOneManyProperty = Mappings.Mappings[typeof(ManyToOneManyFromComplexClass)].ManyToOneProperties[0];
            var TempDataModel = new DataModel(Mappings, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ManyToOneManyProperty.Setup(Mappings, TempDataModel.SourceSpec);

            var TestObject = new DataLoadQuery<ManyToOneManyFromComplexClass>(Mappings, ObjectPool);
            var TempManyToOneMany = new ManyToOneManyFromComplexClass { ID = 10, BoolValue = true };
            TempManyToOneMany.ManyToOneClass.Add(new AllReferencesAndID { ID = 1 });
            TempManyToOneMany.ManyToOneClass.Add(new AllReferencesAndID { ID = 2 });

            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries([new(new { ID = 10 })])[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            _ = Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IManyToOneMany_].[ID_] AS [ID],[dbo].[IManyToOneMany_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToOneManyFromComplexClass_]\r\nINNER JOIN [dbo].[IManyToOneMany_] ON [dbo].[ManyToOneManyFromComplexClass_].[IManyToOneMany_ID_]=[dbo].[IManyToOneMany_].[ID_]\r\nWHERE ([dbo].[IManyToOneMany_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithManyToOneManyProperties()
        {
            var Mappings = new MappingSource([
                new ManyToOneManyPropertiesMapping(),
                new ManyToOneOnePropertiesMapping()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);

            Inflatable.ClassMapper.Interfaces.IManyToOneProperty ManyToOneManyProperty = Mappings.Mappings[typeof(ManyToOneManyProperties)].ManyToOneProperties[0];
            var TempDataModel = new DataModel(Mappings, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ManyToOneManyProperty.Setup(Mappings, TempDataModel.SourceSpec);

            var TestObject = new DataLoadQuery<ManyToOneManyProperties>(Mappings, ObjectPool);
            var TempManyToOneMany = new ManyToOneManyProperties { ID = 10, BoolValue = true };
            TempManyToOneMany.ManyToOneClass.Add(new ManyToOneOneProperties { ID = 1 });
            TempManyToOneMany.ManyToOneClass.Add(new ManyToOneOneProperties { ID = 2 });

            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries([new(new { ID = 1 })])[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            _ = Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[ManyToOneManyProperties_].[ID_] AS [ID],[dbo].[ManyToOneManyProperties_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToOneManyProperties_]\r\nWHERE ([dbo].[ManyToOneManyProperties_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithManyToOneOneFromComplexGraphProperties()
        {
            var Mappings = new MappingSource([
                new ManyToOneOneFromComplexClassMapping(),
                new IManyToOneOneMapping(),
                new AllReferencesAndIDMappingWithDatabase()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);

            Inflatable.ClassMapper.Interfaces.IManyToOneProperty ManyToOneManyProperty = Mappings.Mappings[typeof(ManyToOneOneFromComplexClass)].ManyToOneProperties[0];
            var TempDataModel = new DataModel(Mappings, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ManyToOneManyProperty.Setup(Mappings, TempDataModel.SourceSpec);

            var TestObject = new DataLoadQuery<ManyToOneOneFromComplexClass>(Mappings, ObjectPool);

            _ = new ManyToOneOneFromComplexClass
            {
                ID = 10,
                BoolValue = true,
                ManyToOneClass = new AllReferencesAndID { ID = 1 }
            };

            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries([new(new { ID = 1 })])[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            _ = Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IManyToOneOne_].[ID_] AS [ID],[dbo].[IManyToOneOne_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToOneOneFromComplexClass_]\r\nINNER JOIN [dbo].[IManyToOneOne_] ON [dbo].[ManyToOneOneFromComplexClass_].[IManyToOneOne_ID_]=[dbo].[IManyToOneOne_].[ID_]\r\nWHERE ([dbo].[IManyToOneOne_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithManyToOneSingleProperties()
        {
            var Mappings = new MappingSource([
                new ManyToOneOnePropertiesMapping(),
                new ManyToOneManyPropertiesMapping()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);

            Inflatable.ClassMapper.Interfaces.IManyToOneProperty ManyToOneOneProperty = Mappings.Mappings[typeof(ManyToOneOneProperties)].ManyToOneProperties[0];
            var TempDataModel = new DataModel(Mappings, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ManyToOneOneProperty.Setup(Mappings, TempDataModel.SourceSpec);

            var TestObject = new DataLoadQuery<ManyToOneOneProperties>(Mappings, ObjectPool);

            _ = new ManyToOneOneProperties
            {
                ID = 10,
                BoolValue = true,
                ManyToOneClass = new ManyToOneManyProperties { ID = 1 }
            };

            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries([new(new { ID = 1 })])[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            _ = Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[ManyToOneOneProperties_].[ID_] AS [ID],[dbo].[ManyToOneOneProperties_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[ManyToOneOneProperties_]\r\nWHERE ([dbo].[ManyToOneOneProperties_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapProperties()
        {
            var Mappings = new MappingSource([
                new AllReferencesAndIDMappingWithDatabase(),
                new MapPropertiesMapping()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);
            Inflatable.ClassMapper.Interfaces.IMapProperty MapProperty = Mappings.Mappings[typeof(MapProperties)].MapProperties[0];
            MapProperty.Setup(Mappings);
            var TestObject = new DataLoadQuery<MapProperties>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries([new(new { ID = 1 })])[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            _ = Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[MapProperties_].[ID_] AS [ID],[dbo].[MapProperties_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[MapProperties_]\r\nWHERE ([dbo].[MapProperties_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapPropertiesComplex()
        {
            var Mappings = new MappingSource([
                new AllReferencesAndIDMappingWithDatabase(),
                new MapPropertiesFromComplexClassMapping(),
                new IMapPropertiesInterfaceMapping()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);
            Inflatable.ClassMapper.Interfaces.IMapProperty MapProperty = Mappings.Mappings[typeof(MapPropertiesFromComplexClass)].MapProperties[0];
            MapProperty.Setup(Mappings);
            var TestObject = new DataLoadQuery<MapPropertiesFromComplexClass>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries([new(new { ID = 1 })])[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            _ = Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IMapPropertiesInterface_].[ID_] AS [ID],[dbo].[IMapPropertiesInterface_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[MapPropertiesFromComplexClass_]\r\nINNER JOIN [dbo].[IMapPropertiesInterface_] ON [dbo].[MapPropertiesFromComplexClass_].[IMapPropertiesInterface_ID_]=[dbo].[IMapPropertiesInterface_].[ID_]\r\nWHERE ([dbo].[IMapPropertiesInterface_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapToSelf()
        {
            var Mappings = new MappingSource([
                new MapPropertyReferencesSelfMapping(),
                new IMapPropertiesInterfaceMapping()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);
            Inflatable.ClassMapper.Interfaces.IMapProperty MapProperty = Mappings.Mappings[typeof(MapPropertyReferencesSelf)].MapProperties[0];
            MapProperty.Setup(Mappings);
            var TestObject = new DataLoadQuery<MapPropertyReferencesSelf>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries([new(new { ID = 1 })])[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            _ = Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IMapPropertiesInterface_].[ID_] AS [ID],[dbo].[IMapPropertiesInterface_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[MapPropertyReferencesSelf_]\r\nINNER JOIN [dbo].[IMapPropertiesInterface_] ON [dbo].[MapPropertyReferencesSelf_].[IMapPropertiesInterface_ID_]=[dbo].[IMapPropertiesInterface_].[ID_]\r\nWHERE ([dbo].[IMapPropertiesInterface_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapToSelfOnInterface()
        {
            var Mappings = new MappingSource([
                new MapPropertiesWithMapOnInterfaceMapping(),
                new IMapPropertiesInterfaceWithMapMapping()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);
            Inflatable.ClassMapper.Interfaces.IMapProperty MapProperty = Mappings.Mappings[typeof(IMapPropertiesInterfaceWithMap)].MapProperties[0];
            MapProperty.Setup(Mappings);
            var TestObject = new DataLoadQuery<MapPropertiesWithMapOnInterface>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries([new(new { ID = 1 })])[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            _ = Assert.Single(Result.Parameters);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID0", Result.Parameters[0].ID);
            Assert.Equal("SELECT [dbo].[IMapPropertiesInterfaceWithMap_].[ID_] AS [ID],[dbo].[MapPropertiesWithMapOnInterface_].[BoolValue_] AS [BoolValue]\r\nFROM [dbo].[MapPropertiesWithMapOnInterface_]\r\nINNER JOIN [dbo].[IMapPropertiesInterfaceWithMap_] ON [dbo].[MapPropertiesWithMapOnInterface_].[IMapPropertiesInterfaceWithMap_ID_]=[dbo].[IMapPropertiesInterfaceWithMap_].[ID_]\r\nWHERE ([dbo].[IMapPropertiesInterfaceWithMap_].[ID_]=@ID0);", Result.QueryString);
            Assert.Equal(QueryType.LoadData, Result.QueryType);
        }
    }
}