using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.MultipleDataSources.Mappings;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.ClassMapper
{
    [Collection("Test collection")]
    public class MultipleDataSourceTests : TestingFixture
    {
        public MultipleDataSourceTests(SetupFixture setupFixture)
            : base(setupFixture) { }

        [Fact]
        public void Creation()
        {
            var TestObject = new MappingManager([
                new SimpleClassDataSource1Mapping(),
                new SimpleClassDataSource2Mapping()
            ],
            [
                new MockDatabaseMapping(),
                new SecondMockDatabaseMapping()
            ],
            new QueryProviderManager([new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>())], GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>());

            Assert.Equal(2, TestObject.Sources.Length);
            IMappingSource Source1 = TestObject.Sources.First(x => x.Source.GetType() == typeof(MockDatabaseMapping));
            IMappingSource Source2 = TestObject.Sources.First(x => x.Source.GetType() == typeof(SecondMockDatabaseMapping));
            _ = Assert.Single(Source1.Mappings);
            _ = Assert.Single(Source2.Mappings);
            IMapping Source1Mapping = Source1.Mappings.First().Value;
            IMapping Source2Mapping = Source2.Mappings.First().Value;
            _ = Assert.Single(Source1Mapping.IDProperties);
            _ = Assert.Single(Source2Mapping.IDProperties);
            _ = Assert.Single(Source1Mapping.ReferenceProperties);
            _ = Assert.Single(Source2Mapping.ReferenceProperties);
        }
    }
}