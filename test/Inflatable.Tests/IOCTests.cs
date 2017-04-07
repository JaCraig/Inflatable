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
            var Manager = Canister.Builder.Bootstrapper.Resolve<SchemaManager>();
        }
    }
}