using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.ClassMapper
{
    public class MappingManagerTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TestObject = new MappingManager(new[] {
                new AllReferencesAndIDMappingNoDatabase()
            },
            new IDatabase[]{
                new MockDatabaseMapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, SQLHelperLogger) }, GetLogger<QueryProviderManager>()),
            GetLogger<MappingManager>(),
            ObjectPool);
            var TestSource = TestObject.Sources.First();
            Assert.Single(TestObject.Sources);
            Assert.Single(TestSource.Mappings);
            Assert.Equal(typeof(AllReferencesAndID), TestSource.Mappings.First().Key);
            Assert.IsType<AllReferencesAndIDMappingNoDatabase>(TestSource.Mappings.First().Value);
            Assert.Single(TestSource.TypeGraphs);
            Assert.Equal(typeof(AllReferencesAndID), TestSource.TypeGraphs.First().Key);
            Assert.Equal(TestSource.Mappings.First().Key, TestSource.TypeGraphs.First().Key);
        }
    }
}