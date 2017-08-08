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
            var TestQuery2 = TestQuery.Select(x => new { x.BoolValue, x.ByteArrayValue });
            var Data = TestObject.Translate(TestQuery2.Expression);
            var Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal(2, Result.SelectValues.Count);
            Assert.Equal("BoolValue", Result.SelectValues[0].Name);
            Assert.Equal("ByteArrayValue", Result.SelectValues[1].Name);
        }

        [Fact]
        public void TranslateSelectProjection()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            var TestQuery2 = TestQuery.Select(x => new { BoolValue2 = x.BoolValue, Temp = x.ByteArrayValue });
            var Data = TestObject.Translate(TestQuery2.Expression);
            var Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal(2, Result.SelectValues.Count);
            Assert.Equal("BoolValue", Result.SelectValues[0].Name);
            Assert.Equal("ByteArrayValue", Result.SelectValues[1].Name);
        }
    }
}