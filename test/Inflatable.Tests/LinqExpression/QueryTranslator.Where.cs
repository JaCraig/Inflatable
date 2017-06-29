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
        public void TranslateWhere()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            int LocalVariable = 45;
            TestQuery = TestQuery.Where(x => x.BoolValue == false)
                                 .Where(x => x.ByteValue > 14)
                                 .Where(x => x.IntValue < LocalVariable);
            var Data = TestObject.Translate(TestQuery.Expression);
            var Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE (((BoolValue = 0) AND ((ByteValue) > 14)) AND (IntValue < 45))", Result.WhereClause.ToString());
            Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default2")];
            Assert.Equal("", Result.WhereClause.ToString());
        }

        [Fact]
        public void TranslateWhereBinary()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Where(x => x.BoolValue == false || x.ByteValue > 14);
            var Data = TestObject.Translate(TestQuery.Expression);
            var Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE ((BoolValue = 0) OR ((ByteValue) > 14))", Result.WhereClause.ToString());
            Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default2")];
            Assert.Equal("", Result.WhereClause.ToString());
        }

        [Fact]
        public void TranslateWhereBinaryNot()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Where(x => !(x.BoolValue == false || x.ByteValue > 14));
            var Data = TestObject.Translate(TestQuery.Expression);
            var Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE ((BoolValue <> 0) AND ((ByteValue) <= 14))", Result.WhereClause.ToString());
            Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default2")];
            Assert.Equal("", Result.WhereClause.ToString());
        }

        [Fact]
        public void TranslateWhereTrue()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Where(x => x.BoolValue);
            var Data = TestObject.Translate(TestQuery.Expression);
            var Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE (BoolValue = 1)", Result.WhereClause.ToString());
            Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default2")];
            Assert.Equal("", Result.WhereClause.ToString());
        }

        [Fact]
        public void TranslateWhereTrueNot()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Where(x => !x.BoolValue);
            var Data = TestObject.Translate(TestQuery.Expression);
            var Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE (BoolValue <> 1)", Result.WhereClause.ToString());
            Result = Data[Mappings.Sources.First(x => x.Source.Name == "Default2")];
            Assert.Equal("", Result.WhereClause.ToString());
        }
    }
}