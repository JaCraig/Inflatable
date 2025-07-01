using Inflatable.ClassMapper;
using Inflatable.Interfaces;
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
using System.Linq;
using Xunit;

namespace Inflatable.Tests.QueryProvider.Providers.SQLServer.QueryGenerators
{
    [Collection("Test collection")]
    public class SavePropertiesQueryTests : TestingFixture
    {
        public SavePropertiesQueryTests(SetupFixture setupFixture)
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
            var TestObject = new SavePropertiesQuery<ConcreteClass1>(Mappings, ObjectPool);
            Assert.Equal(typeof(ConcreteClass1), TestObject.AssociatedType);
            Assert.Same(Mappings, TestObject.MappingInformation);
            Assert.Equal(QueryType.JoinsSave, TestObject.QueryType);
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
            var TestObject = new SavePropertiesQuery<ConcreteClass1>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery[] Result = TestObject.GenerateDeclarations();
            Assert.Equal(CommandType.Text, Result[0].DatabaseCommandType);
            Assert.Empty(Result[0].Parameters);
            Assert.Equal("", Result[0].QueryString);
            Assert.Equal(QueryType.JoinsSave, Result[0].QueryType);
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
            var TestObject = new SavePropertiesQuery<ConcreteClass1>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(new ConcreteClass1 { ID = 10, BaseClassValue1 = 1, Value1 = 2 })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Empty(Result.Parameters);
            Assert.Equal("", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
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

            var TestObject = new SavePropertiesQuery<ManyToManyProperties>(Mappings, ObjectPool);
            var TempManyToMany = new ManyToManyProperties { ID = 10, BoolValue = true };
            TempManyToMany.ManyToManyClass.Add(new AllReferencesAndID { ID = 1 });
            TempManyToMany.ManyToManyClass.Add(new AllReferencesAndID { ID = 2 });

            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(TempManyToMany, ManyToManyProperty)[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal(1, Result.Parameters[1].InternalValue);
            Assert.Equal("AllReferencesAndID_ID_", Result.Parameters[1].ID);
            Assert.Equal("ManyToManyProperties_ID_", Result.Parameters[0].ID);
            Assert.Equal("INSERT INTO [dbo].[AllReferencesAndID_ManyToManyProperties]([dbo].[AllReferencesAndID_ManyToManyProperties].[AllReferencesAndID_ID_], [dbo].[AllReferencesAndID_ManyToManyProperties].[ManyToManyProperties_ID_]) VALUES (@AllReferencesAndID_ID_, @ManyToManyProperties_ID_);", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
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
            ManyToOneManyProperty.SetColumnInfo(Mappings);

            var TestObject = new SavePropertiesQuery<ManyToOneManyFromComplexClass>(Mappings, ObjectPool);
            var TempManyToOneMany = new ManyToOneManyFromComplexClass { ID = 10, BoolValue = true };
            TempManyToOneMany.ManyToOneClass.Add(new AllReferencesAndID { ID = 1 });
            TempManyToOneMany.ManyToOneClass.Add(new AllReferencesAndID { ID = 2 });

            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(TempManyToOneMany, ManyToOneManyProperty)[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal(1, Result.Parameters[1].InternalValue);
            Assert.Equal("IManyToOneMany_ID_", Result.Parameters[0].ID);
            Assert.Equal("ID_", Result.Parameters[1].ID);
            Assert.Equal("UPDATE [dbo].[AllReferencesAndID_] SET [dbo].[AllReferencesAndID_].[IManyToOneMany_ID_] = @IManyToOneMany_ID_ FROM [dbo].[AllReferencesAndID_] WHERE [dbo].[AllReferencesAndID_].[ID_] = @ID_;", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
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
            ManyToOneManyProperty.SetColumnInfo(Mappings);

            var TestObject = new SavePropertiesQuery<ManyToOneManyProperties>(Mappings, ObjectPool);
            var TempManyToOneMany = new ManyToOneManyProperties { ID = 10, BoolValue = true };
            TempManyToOneMany.ManyToOneClass.Add(new ManyToOneOneProperties { ID = 1 });
            TempManyToOneMany.ManyToOneClass.Add(new ManyToOneOneProperties { ID = 2 });

            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(TempManyToOneMany, ManyToOneManyProperty)[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal(1, Result.Parameters[1].InternalValue);
            Assert.Equal("ManyToOneManyProperties_ID_", Result.Parameters[0].ID);
            Assert.Equal("ID_", Result.Parameters[1].ID);
            Assert.Equal("UPDATE [dbo].[ManyToOneOneProperties_] SET [dbo].[ManyToOneOneProperties_].[ManyToOneManyProperties_ID_] = @ManyToOneManyProperties_ID_ FROM [dbo].[ManyToOneOneProperties_] WHERE [dbo].[ManyToOneOneProperties_].[ID_] = @ID_;", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
            Result = TestObject.GenerateQueries(TempManyToOneMany, ManyToOneManyProperty)[1];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal(2, Result.Parameters[1].InternalValue);
            Assert.Equal("ManyToOneManyProperties_ID_", Result.Parameters[0].ID);
            Assert.Equal("ID_", Result.Parameters[1].ID);
            Assert.Equal("UPDATE [dbo].[ManyToOneOneProperties_] SET [dbo].[ManyToOneOneProperties_].[ManyToOneManyProperties_ID_] = @ManyToOneManyProperties_ID_ FROM [dbo].[ManyToOneOneProperties_] WHERE [dbo].[ManyToOneOneProperties_].[ID_] = @ID_;", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
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
            ManyToOneManyProperty.SetColumnInfo(Mappings);

            var TestObject = new SavePropertiesQuery<ManyToOneOneFromComplexClass>(Mappings, ObjectPool);
            var TempManyToOneMany = new ManyToOneOneFromComplexClass
            {
                ID = 10,
                BoolValue = true,
                ManyToOneClass = new AllReferencesAndID { ID = 1 }
            };

            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(TempManyToOneMany, ManyToOneManyProperty)[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal(10, Result.Parameters[1].InternalValue);
            Assert.Equal("AllReferencesAndID_ID_", Result.Parameters[0].ID);
            Assert.Equal("ID", Result.Parameters[1].ID);
            Assert.Equal("UPDATE [dbo].[IManyToOneOne_] SET [dbo].[IManyToOneOne_].[AllReferencesAndID_ID_] = @AllReferencesAndID_ID_ FROM [dbo].[IManyToOneOne_] WHERE [dbo].[IManyToOneOne_].[ID_] = @ID;", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
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

            var TestObject = new SavePropertiesQuery<ManyToOneOneProperties>(Mappings, ObjectPool);
            var TempManyToOneOne = new ManyToOneOneProperties
            {
                ID = 10,
                BoolValue = true,
                ManyToOneClass = new ManyToOneManyProperties { ID = 1 }
            };

            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(TempManyToOneOne, ManyToOneOneProperty)[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal(10, Result.Parameters[1].InternalValue);
            Assert.Equal("ManyToOneManyProperties_ID_", Result.Parameters[0].ID);
            Assert.Equal("ID", Result.Parameters[1].ID);
            Assert.Equal("UPDATE [dbo].[ManyToOneOneProperties_] SET [dbo].[ManyToOneOneProperties_].[ManyToOneManyProperties_ID_] = @ManyToOneManyProperties_ID_ FROM [dbo].[ManyToOneOneProperties_] WHERE [dbo].[ManyToOneOneProperties_].[ID_] = @ID;", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
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
            MapProperty.SetColumnInfo(Mappings);
            var TestObject = new SavePropertiesQuery<MapPropertiesFromComplexClass>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(new MapPropertiesFromComplexClass { ID = 10, BoolValue = true, MappedClass = new AllReferencesAndID { ID = 1 } }, MapProperty)[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[1].InternalValue);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[1].ID);
            Assert.Equal("AllReferencesAndID_MappedClass_ID_", Result.Parameters[0].ID);
            Assert.Equal("UPDATE [dbo].[MapPropertiesFromComplexClass_] SET [dbo].[MapPropertiesFromComplexClass_].[AllReferencesAndID_MappedClass_ID_] = @AllReferencesAndID_MappedClass_ID_ FROM [dbo].[MapPropertiesFromComplexClass_] INNER JOIN [dbo].[IMapPropertiesInterface_] ON [dbo].[MapPropertiesFromComplexClass_].[IMapPropertiesInterface_ID_]=[dbo].[IMapPropertiesInterface_].[ID_] WHERE [dbo].[IMapPropertiesInterface_].[ID_] = @ID;", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapPropertiesNullValue()
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
            MapProperty.SetColumnInfo(Mappings);
            var TestObject = new SavePropertiesQuery<MapProperties>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(new MapProperties { ID = 10, BoolValue = true }, MapProperty)[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[1].InternalValue);
            Assert.Null(Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[1].ID);
            Assert.Equal("AllReferencesAndID_MappedClass_ID_", Result.Parameters[0].ID);
            Assert.Equal("UPDATE [dbo].[MapProperties_] SET [dbo].[MapProperties_].[AllReferencesAndID_MappedClass_ID_] = @AllReferencesAndID_MappedClass_ID_ FROM [dbo].[MapProperties_] WHERE [dbo].[MapProperties_].[ID_] = @ID;", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapPropertiesWithValue()
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
            MapProperty.SetColumnInfo(Mappings);
            var TestObject = new SavePropertiesQuery<MapProperties>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(new MapProperties { ID = 10, BoolValue = true, MappedClass = new AllReferencesAndID { ID = 1 } }, MapProperty)[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[1].InternalValue);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[1].ID);
            Assert.Equal("AllReferencesAndID_MappedClass_ID_", Result.Parameters[0].ID);
            Assert.Equal("UPDATE [dbo].[MapProperties_] SET [dbo].[MapProperties_].[AllReferencesAndID_MappedClass_ID_] = @AllReferencesAndID_MappedClass_ID_ FROM [dbo].[MapProperties_] WHERE [dbo].[MapProperties_].[ID_] = @ID;", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
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
            MapProperty.SetColumnInfo(Mappings);
            var TestObject = new SavePropertiesQuery<MapPropertyReferencesSelf>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(new MapPropertyReferencesSelf { ID = 10, BoolValue = true, MappedClass = new MapPropertyReferencesSelf { ID = 1 } }, MapProperty)[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[1].InternalValue);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[1].ID);
            Assert.Equal("IMapPropertiesInterface_MappedClass_ID_", Result.Parameters[0].ID);
            Assert.Equal("UPDATE [dbo].[MapPropertyReferencesSelf_] SET [dbo].[MapPropertyReferencesSelf_].[IMapPropertiesInterface_MappedClass_ID_] = @IMapPropertiesInterface_MappedClass_ID_ FROM [dbo].[MapPropertyReferencesSelf_] INNER JOIN [dbo].[IMapPropertiesInterface_] ON [dbo].[MapPropertyReferencesSelf_].[IMapPropertiesInterface_ID_]=[dbo].[IMapPropertiesInterface_].[ID_] WHERE [dbo].[IMapPropertiesInterface_].[ID_] = @ID;", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
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
            MapProperty.SetColumnInfo(Mappings);
            var TestObject = new SavePropertiesQuery<MapPropertiesWithMapOnInterface>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(new MapPropertiesWithMapOnInterface { ID = 10, BoolValue = true, MappedClass = new MapPropertiesWithMapOnInterface { ID = 1 } }, MapProperty)[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[1].InternalValue);
            Assert.Equal(1, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[1].ID);
            Assert.Equal("IMapPropertiesInterfaceWithMap_MappedClass_ID_", Result.Parameters[0].ID);
            Assert.Equal("UPDATE [dbo].[IMapPropertiesInterfaceWithMap_] SET [dbo].[IMapPropertiesInterfaceWithMap_].[IMapPropertiesInterfaceWithMap_MappedClass_ID_] = @IMapPropertiesInterfaceWithMap_MappedClass_ID_ FROM [dbo].[IMapPropertiesInterfaceWithMap_] WHERE [dbo].[IMapPropertiesInterfaceWithMap_].[ID_] = @ID;", Result.QueryString);
            Assert.Equal(QueryType.JoinsSave, Result.QueryType);
        }
    }
}