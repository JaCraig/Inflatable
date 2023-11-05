using Inflatable.ClassMapper;
using Inflatable.QueryProvider;
using Inflatable.Schema;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.Fixtures;
using Xunit;

namespace Inflatable.Tests
{
    [Collection("Test collection")]
    public class IOCTests : TestingFixture
    {
        public IOCTests(SetupFixture setupFixture)
            : base(setupFixture) { }

        [Fact]
        public void RegistrationTests()
        {
            MappingManager TempMappingManager = Resolve<MappingManager>();
            Assert.NotNull(TempMappingManager);
            SchemaManager SchemaManager = Resolve<SchemaManager>();
            Assert.NotNull(SchemaManager);
            QueryProviderManager QueryManager = Resolve<QueryProviderManager>();
            Assert.NotNull(QueryManager);
        }
    }
}