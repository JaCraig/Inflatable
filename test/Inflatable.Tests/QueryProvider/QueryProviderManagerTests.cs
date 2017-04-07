using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
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
            var Mappings = new MappingSource(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            },
                new MockDatabaseMapping(),
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
            var QueryProvider = new SQLServerQueryProvider(Configuration);
            var TestObject = new QueryProviderManager(new[] { QueryProvider }, Mappings);
            var Result = TestObject.CreateBatch(new MockDatabaseMapping());
            Assert.NotNull(Result);
        }

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
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
            var QueryProvider = new SQLServerQueryProvider(Configuration);
            var TestObject = new QueryProviderManager(new[] { QueryProvider }, Mappings);
            Assert.Equal(Mappings, TestObject.MappingInfo);
            Assert.Equal(SqlClientFactory.Instance, TestObject.Providers.Keys.First());
            Assert.Equal(QueryProvider, TestObject.Providers.Values.First());
        }
    }
}