using BigBook.Queryable;
using Inflatable.LinqExpression;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.LinqExpression
{
    public partial class QueryTranslatorTests : TestingFixture
    {
        [Fact]
        public void TranslateSelect()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            int LocalVariable = 45;
            var TestQuery2 = TestQuery.Select(x => new { x.BoolValue, x.ByteArrayValue });
            var Data = TestObject.Translate(TestQuery2.Expression);
            Assert.Equal(2, Data.Values.First().SelectValues.Count);
            Assert.Equal("BoolValue", Data.Values.First().SelectValues[0].Name);
            Assert.Equal("ByteArrayValue", Data.Values.First().SelectValues[1].Name);
        }
    }
}