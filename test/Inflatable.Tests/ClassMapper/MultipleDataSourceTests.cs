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
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>()) }, GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>());

            Assert.Equal(2, TestObject.Sources.Count());
            var Source1 = TestObject.Sources.First(x => x.Source.GetType() == typeof(MockDatabaseMapping));
            var Source2 = TestObject.Sources.First(x => x.Source.GetType() == typeof(SecondMockDatabaseMapping));
            Assert.Single(Source1.Mappings);
            Assert.Single(Source2.Mappings);
            var Source1Mapping = Source1.Mappings.First().Value;
            var Source2Mapping = Source2.Mappings.First().Value;
            Assert.Single(Source1Mapping.IDProperties);
            Assert.Single(Source2Mapping.IDProperties);
            Assert.Single(Source1Mapping.ReferenceProperties);
            Assert.Single(Source2Mapping.ReferenceProperties);
        }
    }
}