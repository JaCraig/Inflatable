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
using Inflatable.Tests.TestDatabases.SimpleTestWithDatabase;
using System.Data;
using Xunit;

namespace Inflatable.Tests.QueryProvider.Providers.SQLServer.QueryGenerators
{
    [Collection("Test collection")]
    public class DeletePropertiesQueryTests : TestingFixture
    {
        public DeletePropertiesQueryTests(SetupFixture setupFixture)
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
            var TestObject = new DeletePropertiesQuery<ConcreteClass1>(Mappings, ObjectPool);
            Assert.Equal(typeof(ConcreteClass1), TestObject.AssociatedType);
            Assert.Same(Mappings, TestObject.MappingInformation);
            Assert.Equal(QueryType.JoinsDelete, TestObject.QueryType);
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
            var TestObject = new DeletePropertiesQuery<ConcreteClass1>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery[] Result = TestObject.GenerateDeclarations();
            Assert.Equal(CommandType.Text, Result[0].DatabaseCommandType);
            Assert.Empty(Result[0].Parameters);
            Assert.Equal("", Result[0].QueryString);
            Assert.Equal(QueryType.JoinsDelete, Result[0].QueryType);
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
            var TestObject = new DeletePropertiesQuery<ConcreteClass1>(Mappings, ObjectPool);
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(new ConcreteClass1 { ID = 10, BaseClassValue1 = 1, Value1 = 2 })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Empty(Result.Parameters);
            Assert.Equal("", Result.QueryString);
            Assert.Equal(QueryType.JoinsDelete, Result.QueryType);
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
            var TestObject = new DeletePropertiesQuery<ManyToManyProperties>(Mappings, ObjectPool);
            var TempManyToMany = new ManyToManyProperties { ID = 10, BoolValue = true };
            TempManyToMany.ManyToManyClass.Add(new TestDatabases.SimpleTest.AllReferencesAndID { ID = 1 });
            TempManyToMany.ManyToManyClass.Add(new TestDatabases.SimpleTest.AllReferencesAndID { ID = 2 });
            Inflatable.QueryProvider.Interfaces.IQuery Result = TestObject.GenerateQueries(TempManyToMany, ManyToManyProperty)[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            _ = Assert.Single(Result.Parameters);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ManyToManyProperties_ID_", Result.Parameters[0].ID);
            Assert.Equal("DELETE FROM [dbo].[AllReferencesAndID_ManyToManyProperties] WHERE ([dbo].[AllReferencesAndID_ManyToManyProperties].[ManyToManyProperties_ID_] = @ManyToManyProperties_ID_);", Result.QueryString);
            Assert.Equal(QueryType.JoinsDelete, Result.QueryType);
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
            Inflatable.ClassMapper.Interfaces.IManyToOneProperty ManyToOneProperty = Mappings.Mappings[typeof(ManyToOneManyProperties)].ManyToOneProperties[0];
            var TempDataModel = new DataModel(Mappings, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ManyToOneProperty.Setup(Mappings, TempDataModel.SourceSpec);
            var TestObject = new DeletePropertiesQuery<ManyToOneManyProperties>(Mappings, ObjectPool);
            var TempManyToOne = new ManyToOneManyProperties { ID = 10, BoolValue = true };
            TempManyToOne.ManyToOneClass.Add(new ManyToOneOneProperties { ID = 1 });
            TempManyToOne.ManyToOneClass.Add(new ManyToOneOneProperties { ID = 2 });
            Inflatable.QueryProvider.Interfaces.IQuery[] Result = TestObject.GenerateQueries(TempManyToOne, ManyToOneProperty);
            Assert.Empty(Result);
        }

        [Fact]
        public void GenerateQueryWithManyToOneSingleProperties()
        {
            var Mappings = new MappingSource([
                new ManyToOneManyPropertiesMapping(),
                new ManyToOneOnePropertiesMapping()
            ],
                   new MockDatabaseMapping(),
                   new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
               GetLogger<MappingSource>(),
               ObjectPool);
            Inflatable.ClassMapper.Interfaces.IManyToOneProperty ManyToOneProperty = Mappings.Mappings[typeof(ManyToOneOneProperties)].ManyToOneProperties[0];
            var TempDataModel = new DataModel(Mappings, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ManyToOneProperty.Setup(Mappings, TempDataModel.SourceSpec);
            var TestObject = new DeletePropertiesQuery<ManyToOneOneProperties>(Mappings, ObjectPool);
            var TempManyToOne = new ManyToOneOneProperties
            {
                ID = 10,
                BoolValue = true,
                ManyToOneClass = new ManyToOneManyProperties { ID = 1 }
            };
            Inflatable.QueryProvider.Interfaces.IQuery[] Result = TestObject.GenerateQueries(TempManyToOne, ManyToOneProperty);
            Assert.Empty(Result);
        }
    }
}