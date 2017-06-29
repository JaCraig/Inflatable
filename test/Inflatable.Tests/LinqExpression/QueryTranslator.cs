using Inflatable.ClassMapper;
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using Xunit;

namespace Inflatable.Tests.LinqExpression
{
    public partial class QueryTranslatorTests : TestingFixture
    {
        public QueryTranslatorTests()
        {
            Mappings = Canister.Builder.Bootstrapper.Resolve<MappingManager>();
            QueryProviders = Canister.Builder.Bootstrapper.Resolve<QueryProviderManager>();
        }

        private MappingManager Mappings { get; }

        private QueryProviderManager QueryProviders { get; }

        [Fact]
        public void Creation()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            Assert.NotNull(TestObject.MappingManager);
            Assert.NotNull(TestObject.QueryProviderManager);
        }
    }
}