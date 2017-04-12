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
            var TempMappingManager = Canister.Builder.Bootstrapper.Resolve<MappingManager>();
            Assert.NotNull(TempMappingManager);
            var SchemaManager = Canister.Builder.Bootstrapper.Resolve<SchemaManager>();
            Assert.NotNull(SchemaManager);
            var QueryManager = Canister.Builder.Bootstrapper.Resolve<QueryProviderManager>();
            Assert.NotNull(QueryManager);
        }
    }
}