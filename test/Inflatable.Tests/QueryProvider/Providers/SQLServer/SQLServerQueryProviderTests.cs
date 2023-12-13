using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph;
using Inflatable.Tests.TestDatabases.ComplexGraph.Mappings;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Inflatable.Tests.QueryProvider.Providers.SQLServer
{
    [Collection("Test collection")]
    public class SQLServerQueryProviderTests : TestingFixture
    {
        public SQLServerQueryProviderTests(SetupFixture setupFixture)
            : base(setupFixture) { }

        [Fact]
        public void Batch()
        {
            var TestObject = new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>());
            SQLHelperDB.SQLHelper Result = TestObject.Batch(new TestDatabases.Databases.TestDatabase2Mapping());
            Assert.NotNull(Result);
        }

        [Fact]
        public void CreateGenerator()
        {
            var TestObject = new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>());
            var Mappings = new MappingSource(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            },
                new MockDatabaseMapping(),
                new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>()) }, GetLogger<QueryProviderManager>()),
                GetLogger<MappingSource>(),
               ObjectPool);
            Inflatable.QueryProvider.Interfaces.IGenerator<ConcreteClass1> Result = TestObject.CreateGenerator<ConcreteClass1>(Mappings);
            Assert.Equal(typeof(ConcreteClass1), Result.AssociatedType);
        }

        [Fact]
        public void Creation()
        {
            var TestObject = new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>());
            Assert.Equal(Configuration, TestObject.Configuration);
            Assert.Contains(SqlClientFactory.Instance, TestObject.Providers);
        }
    }
}