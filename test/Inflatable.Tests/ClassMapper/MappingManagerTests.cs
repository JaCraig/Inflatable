using Inflatable.ClassMapper;
using Inflatable.Tests.BaseClasses;
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
            var TestObject = new MappingManager(new[] { new AllReferencesAndIDMappingNoDatabase() });
            Assert.Equal(1, TestObject.Mappings.Count);
            Assert.Equal(typeof(AllReferencesAndID), TestObject.Mappings.First().Key);
            Assert.IsType<AllReferencesAndIDMappingNoDatabase>(TestObject.Mappings.First().Value);
            Assert.Equal(1, TestObject.TypeGraphs.Count());
            Assert.Equal(typeof(AllReferencesAndID), TestObject.TypeGraphs.First().Key);
            Assert.Equal(TestObject.Mappings.First().Key, TestObject.TypeGraphs.First().Key);
        }
    }
}