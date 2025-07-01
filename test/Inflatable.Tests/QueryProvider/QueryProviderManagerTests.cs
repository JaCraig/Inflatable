using Inflatable.ClassMapper;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph;
using Inflatable.Tests.TestDatabases.ComplexGraph.Mappings;
using Microsoft.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.QueryProvider
{
    [Collection("Test collection")]
    public class QueryProviderManagerTests : TestingFixture
    {
        public QueryProviderManagerTests(SetupFixture setupFixture)
            : base(setupFixture) { }

        [Fact]
        public void CreateBatch()
        {
            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>());
            var TestObject = new QueryProviderManager([TempQueryProvider], GetLogger<QueryProviderManager>());
            SQLHelperDB.SQLHelper Result = TestObject.CreateBatch(new MockDatabaseMapping());
            Assert.NotNull(Result);
        }

        [Fact]
        public void CreateGenerator()
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
            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>());
            var TestObject = new QueryProviderManager([TempQueryProvider], GetLogger<QueryProviderManager>());
            Inflatable.QueryProvider.Interfaces.IGenerator<ConcreteClass1> Result = TestObject.CreateGenerator<ConcreteClass1>(Mappings);
            Assert.NotNull(Result);
            Assert.Equal(typeof(ConcreteClass1), Result.AssociatedType);
        }

        [Fact]
        public void Creation()
        {
            var TempQueryProvider = new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>());
            var TestObject = new QueryProviderManager([TempQueryProvider], GetLogger<QueryProviderManager>());
            Assert.Equal(SqlClientFactory.Instance, TestObject.Providers.Keys.First());
            Assert.Equal(TempQueryProvider, TestObject.Providers.Values.First());
        }
    }
}