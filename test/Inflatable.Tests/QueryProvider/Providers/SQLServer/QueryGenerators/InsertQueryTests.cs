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
using Inflatable.Tests.TestDatabases.SimpleTest;
using Inflatable.Tests.TestDatabases.SimpleTestWithDatabase;
using Serilog;
using System.Data;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.QueryProvider.Providers.SQLServer.QueryGenerators
{
    public class InsertQueryTests : TestingFixture
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
            var TestObject = new InsertQuery<ConcreteClass1>(Mappings);
            Assert.Equal(typeof(ConcreteClass1), TestObject.AssociatedType);
            Assert.Same(Mappings, TestObject.MappingInformation);
            Assert.Equal(QueryType.Insert, TestObject.QueryType);
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
            var TestObject = new InsertQuery<ConcreteClass1>(Mappings);
            var Result = TestObject.GenerateDeclarations();
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Empty(Result.Parameters);
            Assert.Equal("DECLARE @IInterface1_ID_Temp AS INT;\r\n\r\nDECLARE @BaseClass1_ID_Temp AS BIGINT;\r\n\r\nDECLARE @ConcreteClass1_ID_Temp AS BIGINT;\r\n", Result.QueryString);
            Assert.Equal(QueryType.Insert, Result.QueryType);
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
            var TestObject = new InsertQuery<ConcreteClass1>(Mappings);
            var Result = TestObject.GenerateQuery(new ConcreteClass1 { ID = 10, BaseClassValue1 = 1, Value1 = 2 });
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(3, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal(1, Result.Parameters[1].InternalValue);
            Assert.Equal("BaseClassValue1", Result.Parameters[1].ID);
            Assert.Equal(2, Result.Parameters[2].InternalValue);
            Assert.Equal("Value1", Result.Parameters[2].ID);
            Assert.Equal("INSERT INTO [dbo].[IInterface1_] DEFAULT VALUES;\r\nSET @IInterface1_ID_Temp=SCOPE_IDENTITY();\r\nSELECT @IInterface1_ID_Temp AS [ID];\r\n\r\nINSERT INTO [dbo].[BaseClass1_]([dbo].[BaseClass1_].[BaseClassValue1_],[dbo].[BaseClass1_].[IInterface1_ID_]) VALUES (@BaseClassValue1,@IInterface1_ID_Temp);\r\nSET @BaseClass1_ID_Temp=SCOPE_IDENTITY();\r\n\r\nINSERT INTO [dbo].[ConcreteClass1_]([dbo].[ConcreteClass1_].[Value1_],[dbo].[ConcreteClass1_].[BaseClass1_ID_]) VALUES (@Value1,@BaseClass1_ID_Temp);\r\n", Result.QueryString);
            Assert.Equal(QueryType.Insert, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapPropertiesNullValue()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new AllReferencesAndIDMappingWithDatabase(),
                new MapPropertiesMapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>());
            Mappings.Mappings[typeof(MapProperties)].MapProperties.First().Setup(Mappings);
            var TestObject = new InsertQuery<MapProperties>(Mappings);
            var Result = TestObject.GenerateQuery(new MapProperties { ID = 10, BoolValue = true });
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(3, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal(true, Result.Parameters[1].InternalValue);
            Assert.Equal("BoolValue", Result.Parameters[1].ID);
            Assert.Equal(null, Result.Parameters[2].InternalValue);
            Assert.Equal("MappedClass_AllReferencesAndID_ID_", Result.Parameters[2].ID);
            Assert.Equal("INSERT INTO [dbo].[MapProperties_]([dbo].[MapProperties_].[BoolValue_],[dbo].[MapProperties_].[MappedClass_AllReferencesAndID_ID_]) VALUES (@BoolValue,@MappedClass_AllReferencesAndID_ID_);\r\nSET @MapProperties_ID_Temp=SCOPE_IDENTITY();\r\nSELECT @MapProperties_ID_Temp AS [ID];\r\n", Result.QueryString);
            Assert.Equal(QueryType.Insert, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapPropertiesWithValue()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new AllReferencesAndIDMappingWithDatabase(),
                new MapPropertiesMapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>());
            Mappings.Mappings[typeof(MapProperties)].MapProperties.First().Setup(Mappings);
            var TestObject = new InsertQuery<MapProperties>(Mappings);
            var Result = TestObject.GenerateQuery(new MapProperties { ID = 10, BoolValue = true, MappedClass = new AllReferencesAndID { ID = 1 } });
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(3, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal(true, Result.Parameters[1].InternalValue);
            Assert.Equal("BoolValue", Result.Parameters[1].ID);
            Assert.Equal(1, Result.Parameters[2].InternalValue);
            Assert.Equal("MappedClass_AllReferencesAndID_ID_", Result.Parameters[2].ID);
            Assert.Equal("INSERT INTO [dbo].[MapProperties_]([dbo].[MapProperties_].[BoolValue_],[dbo].[MapProperties_].[MappedClass_AllReferencesAndID_ID_]) VALUES (@BoolValue,@MappedClass_AllReferencesAndID_ID_);\r\nSET @MapProperties_ID_Temp=SCOPE_IDENTITY();\r\nSELECT @MapProperties_ID_Temp AS [ID];\r\n", Result.QueryString);
            Assert.Equal(QueryType.Insert, Result.QueryType);
        }
    }
}