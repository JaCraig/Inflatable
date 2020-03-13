using BigBook.Queryable;
using Inflatable.LinqExpression;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System;
using System.Linq;
using TestFountain;
using Xunit;

namespace Inflatable.Tests.LinqExpression
{
    public partial class QueryTranslatorTests : TestingFixture
    {
        [Theory]
        [FountainData(100)]
        public void ElementsBetween(int start, int numberOfElements)
        {
            start = Math.Abs(start);
            numberOfElements = Math.Abs(numberOfElements);
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Skip(start).Take(numberOfElements);
            var TempData = TestObject.Translate(TestQuery.Expression);
            Assert.Equal(2, TempData.Count);
            var Result = TempData[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE ([dbo].[AllReferencesAndID_].[BoolValue_] <> @0)", Result.WhereClause.ToString());
            var Parameters = Result.WhereClause.GetParameters();
            Assert.Equal("0", Parameters[0].ID);
            Assert.True((bool)Parameters[0].InternalValue);
            //Assert.Equal(DbType.Boolean, Parameters[0].DatabaseType);
        }
    }
}