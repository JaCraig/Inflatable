using Inflatable.ClassMapper;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.ClassMapper
{
    [Collection("Test collection")]
    public class MappingManagerTests : TestingFixture
    {
        public MappingManagerTests(SetupFixture setupFixture)
            : base(setupFixture) { }

        [Fact]
        public void Creation()
        {
            var TestObject = new MappingManager([
                new AllReferencesAndIDMappingNoDatabase()
            ],
            [
                new MockDatabaseMapping()
            ],
            new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>());
            IMappingSource TestSource = TestObject.Sources[0];
            _ = Assert.Single(TestObject.Sources);
            _ = Assert.Single(TestSource.Mappings);
            Assert.Equal(typeof(AllReferencesAndID), TestSource.Mappings.First().Key);
            _ = Assert.IsType<AllReferencesAndIDMappingNoDatabase>(TestSource.Mappings.First().Value);
            _ = Assert.Single(TestSource.TypeGraphs);
            Assert.Equal(typeof(AllReferencesAndID), TestSource.TypeGraphs.First().Key);
            Assert.Equal(TestSource.Mappings.First().Key, TestSource.TypeGraphs.First().Key);
        }
    }
}