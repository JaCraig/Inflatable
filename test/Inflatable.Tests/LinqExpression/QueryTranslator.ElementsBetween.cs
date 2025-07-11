﻿using BigBook.Queryable;
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
        public void ElementsBetween()
        {
            const int Start = 2;
            const int NumberOfElements = 7;

            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Skip(Start).Take(NumberOfElements);
            var TempData = TestObject.Translate(TestQuery.Expression);
            Assert.Equal(2, TempData.Count);
            var Result = TempData[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal(7, Result.Top);
            Assert.Equal(2, Result.Skip);
        }
    }
}