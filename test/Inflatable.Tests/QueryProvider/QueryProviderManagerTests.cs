using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph;
using Inflatable.Tests.TestDatabases.ComplexGraph.Mappings;
using Serilog;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.QueryProvider
{
    public class QueryProviderManagerTests : TestingFixture
    {
        [Fact]
        public void CreateBatch()
        {
            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool);
            var TestObject = new QueryProviderManager(new[] { TempQueryProvider }, Logger);
            var Result = TestObject.CreateBatch(new MockDatabaseMapping(), DynamoFactory);
            Assert.NotNull(Result);
        }

        [Fact]
        public void CreateGenerator()
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
            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool);
            var TestObject = new QueryProviderManager(new[] { TempQueryProvider }, Logger);
            var Result = TestObject.CreateGenerator<ConcreteClass1>(Mappings);
            Assert.NotNull(Result);
            Assert.Equal(typeof(ConcreteClass1), Result.AssociatedType);
        }

        [Fact]
        public void Creation()
        {
            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool);
            var TestObject = new QueryProviderManager(new[] { TempQueryProvider }, Logger);
            Assert.Equal(SqlClientFactory.Instance, TestObject.Providers.Keys.First());
            Assert.Equal(TempQueryProvider, TestObject.Providers.Values.First());
        }
    }
}