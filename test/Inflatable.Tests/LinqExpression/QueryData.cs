using Inflatable.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Inflatable.Tests.LinqExpression
{
    public class QueryDataTests : TestingFixture
    {
        [Fact]
        public void AddParameter()
        {
            var TestObject = new QueryData<AllReferencesAndID>(new Inflatable.ClassMapper.MappingSource(new List<IMapping>(), new MockDatabaseMappingForMockMapping(), Canister.Builder.Bootstrapper.Resolve<QueryProviderManager>(), null));
            TestObject.AddParameter(1);
            Assert.Equal(1, TestObject.Parameters.Count);
            Assert.Equal(DbType.Int32, TestObject.Parameters[0].DatabaseType);
            Assert.Equal(ParameterDirection.Input, TestObject.Parameters[0].Direction);
            Assert.Equal("MockDatabase0", TestObject.Parameters[0].ID);
            Assert.Equal(1, TestObject.Parameters[0].InternalValue);
            Assert.Equal("@", TestObject.Parameters[0].ParameterStarter);
        }
    }
}