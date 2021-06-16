using Inflatable.ClassMapper;
using Inflatable.QueryProvider;
using Inflatable.Schema;
using Inflatable.Tests.BaseClasses;
using Xunit;

namespace Inflatable.Tests
{
    public class IOCTests : TestingFixture
    {
        [Fact]
        public void RegistrationTests()
        {
            var TempMappingManager = Resolve<MappingManager>();
            Assert.NotNull(TempMappingManager);
            var SchemaManager = Resolve<SchemaManager>();
            Assert.NotNull(SchemaManager);
            var QueryManager = Resolve<QueryProviderManager>();
            Assert.NotNull(QueryManager);
        }
    }
}