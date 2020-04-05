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
    public class UpdateQueryTests : TestingFixture
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
            var TestObject = new UpdateQuery<ConcreteClass1>(Mappings);
            Assert.Equal(typeof(ConcreteClass1), TestObject.AssociatedType);
            Assert.Same(Mappings, TestObject.MappingInformation);
            Assert.Equal(QueryType.Update, TestObject.QueryType);
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
            var TestObject = new UpdateQuery<ConcreteClass1>(Mappings);
            var Result = TestObject.GenerateDeclarations();
            Assert.Equal(CommandType.Text, Result[0].DatabaseCommandType);
            Assert.Empty(Result[0].Parameters);
            Assert.Equal("", Result[0].QueryString);
            Assert.Equal(QueryType.Update, Result[0].QueryType);
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
            var TestObject = new UpdateQuery<ConcreteClass1>(Mappings);
            var Result = TestObject.GenerateQueries(new ConcreteClass1 { ID = 10, BaseClassValue1 = 1, Value1 = 2 })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(3, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.Equal(1, Result.Parameters[1].InternalValue);
            Assert.Equal("BaseClassValue1", Result.Parameters[1].ID);
            Assert.Equal(2, Result.Parameters[2].InternalValue);
            Assert.Equal("Value1", Result.Parameters[2].ID);
            Assert.Equal("UPDATE [dbo].[BaseClass1_]\r\nSET [dbo].[BaseClass1_].[BaseClassValue1_]=@BaseClassValue1\r\nFROM [dbo].[BaseClass1_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nWHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n\r\nUPDATE [dbo].[ConcreteClass1_]\r\nSET [dbo].[ConcreteClass1_].[Value1_]=@Value1\r\nFROM [dbo].[ConcreteClass1_]\r\nINNER JOIN [dbo].[BaseClass1_] ON [dbo].[ConcreteClass1_].[BaseClass1_ID_]=[dbo].[BaseClass1_].[ID_]\r\nINNER JOIN [dbo].[IInterface1_] ON [dbo].[BaseClass1_].[IInterface1_ID_]=[dbo].[IInterface1_].[ID_]\r\nWHERE [dbo].[IInterface1_].[ID_]=@ID;\r\n", Result.QueryString);
            Assert.Equal(QueryType.Update, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapPropertiesNullValue()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new AllReferencesAndIDMappingWithDatabase(),
                new MapPropertiesMapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);
            Mappings.Mappings[typeof(MapProperties)].MapProperties.First().Setup(Mappings);
            Mappings.Mappings[typeof(MapProperties)].MapProperties.First().SetColumnInfo(Mappings);
            var TestObject = new UpdateQuery<MapProperties>(Mappings);
            var Result = TestObject.GenerateQueries(new MapProperties { ID = 10, BoolValue = true })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.True((bool)Result.Parameters[1].InternalValue);
            Assert.Equal("BoolValue", Result.Parameters[1].ID);
            Assert.Equal("UPDATE [dbo].[MapProperties_]\r\nSET [dbo].[MapProperties_].[BoolValue_]=@BoolValue\r\nFROM [dbo].[MapProperties_]\r\nWHERE [dbo].[MapProperties_].[ID_]=@ID;\r\n", Result.QueryString);
            Assert.Equal(QueryType.Update, Result.QueryType);
        }

        [Fact]
        public void GenerateQueryWithMapPropertiesWithValue()
        {
            var Mappings = new MappingSource(new IMapping[] {
                new AllReferencesAndIDMappingWithDatabase(),
                new MapPropertiesMapping()
            },
                   new MockDatabaseMapping(),
                   new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
               Canister.Builder.Bootstrapper.Resolve<ILogger>(),
               ObjectPool);
            Mappings.Mappings[typeof(MapProperties)].MapProperties.First().Setup(Mappings);
            Mappings.Mappings[typeof(MapProperties)].MapProperties.First().SetColumnInfo(Mappings);
            var TestObject = new UpdateQuery<MapProperties>(Mappings);
            var Result = TestObject.GenerateQueries(new MapProperties { ID = 10, BoolValue = true, MappedClass = new AllReferencesAndID { ID = 1 } })[0];
            Assert.Equal(CommandType.Text, Result.DatabaseCommandType);
            Assert.Equal(2, Result.Parameters.Length);
            Assert.Equal(10, Result.Parameters[0].InternalValue);
            Assert.Equal("ID", Result.Parameters[0].ID);
            Assert.True((bool)Result.Parameters[1].InternalValue);
            Assert.Equal("BoolValue", Result.Parameters[1].ID);
            Assert.Equal("UPDATE [dbo].[MapProperties_]\r\nSET [dbo].[MapProperties_].[BoolValue_]=@BoolValue\r\nFROM [dbo].[MapProperties_]\r\nWHERE [dbo].[MapProperties_].[ID_]=@ID;\r\n", Result.QueryString);
            Assert.Equal(QueryType.Update, Result.QueryType);
        }
    }
}