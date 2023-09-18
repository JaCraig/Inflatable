using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.LinqExpression;
using Inflatable.QueryProvider;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.Fixtures;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using SQLHelperDB.HelperClasses;
using System.Collections.Generic;
using System.Data;
using Xunit;

namespace Inflatable.Tests.LinqExpression
{
    public class QueryDataTests : TestingFixture
    {
        public QueryDataTests(SetupFixture setupFixture)
            : base(setupFixture) { }

        [Fact]
        public void AddParameter()
        {
            var TestObject = new QueryData<AllReferencesAndID>(new MappingSource(new List<IMapping>(), new MockDatabaseMappingForMockMapping(), Resolve<QueryProviderManager>(), GetLogger<MappingSource>(), ObjectPool));
            TestObject.Parameters.Add(new Parameter<int>("0", 1));
            Assert.Single(TestObject.Parameters);
            Assert.Equal(DbType.Int32, TestObject.Parameters[0].DatabaseType);
            Assert.Equal(ParameterDirection.Input, TestObject.Parameters[0].Direction);
            Assert.Equal("0", TestObject.Parameters[0].ID);
            Assert.Equal(1, TestObject.Parameters[0].InternalValue);
            Assert.Equal("@", TestObject.Parameters[0].ParameterStarter);
        }
    }
}