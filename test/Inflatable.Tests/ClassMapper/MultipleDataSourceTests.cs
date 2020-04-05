using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.MultipleDataSources.Mappings;
using Serilog;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.ClassMapper
{
    public class MultipleDataSourceTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TestObject = new MappingManager(new IMapping[] {
                new SimpleClassDataSource1Mapping(),
                new SimpleClassDataSource2Mapping()
            },
            new IDatabase[]{
                new MockDatabaseMapping(),
                new SecondMockDatabaseMapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool) }, Logger),
            Canister.Builder.Bootstrapper.Resolve<ILogger>(),
            ObjectPool);

            Assert.Equal(2, TestObject.Sources.Count());
            var Source1 = TestObject.Sources.First(x => x.Source.GetType() == typeof(MockDatabaseMapping));
            var Source2 = TestObject.Sources.First(x => x.Source.GetType() == typeof(SecondMockDatabaseMapping));
            Assert.Equal(1, Source1.Mappings.Count);
            Assert.Equal(1, Source2.Mappings.Count);
            var Source1Mapping = Source1.Mappings.First().Value;
            var Source2Mapping = Source2.Mappings.First().Value;
            Assert.Equal(1, Source1Mapping.IDProperties.Count);
            Assert.Equal(1, Source2Mapping.IDProperties.Count);
            Assert.Equal(1, Source1Mapping.ReferenceProperties.Count);
            Assert.Equal(1, Source2Mapping.ReferenceProperties.Count);
        }
    }
}