using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using Serilog;
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
            Canister.Builder.Bootstrapper.Resolve<ILogger>());
            var TestSource = TestObject.Sources.First();
            Assert.Equal(1, TestObject.Sources.Count());
            Assert.Equal(1, TestSource.Mappings.Count);
            Assert.Equal(typeof(AllReferencesAndID), TestSource.Mappings.First().Key);
            Assert.IsType<AllReferencesAndIDMappingNoDatabase>(TestSource.Mappings.First().Value);
            Assert.Equal(1, TestSource.TypeGraphs.Count());
            Assert.Equal(typeof(AllReferencesAndID), TestSource.TypeGraphs.First().Key);
            Assert.Equal(TestSource.Mappings.First().Key, TestSource.TypeGraphs.First().Key);
        }
    }
}