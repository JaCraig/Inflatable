using BigBook.Queryable;
using Inflatable.LinqExpression;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Data;
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
            const int LocalVariable = 45;
            TestQuery = TestQuery.Where(x => !x.BoolValue)
                                 .Where(x => x.ByteValue > 14)
                                 .Where(x => x.IntValue < LocalVariable);
            var TempData = TestObject.Translate(TestQuery.Expression);
            Assert.Equal(2, TempData.Count);
            var Result = TempData[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE ((([dbo].[AllReferencesAndID_].[BoolValue_] <> @0) AND (([dbo].[AllReferencesAndID_].[ByteValue_]) > @2)) AND ([dbo].[AllReferencesAndID_].[IntValue_] < @3))", Result.WhereClause.ToString());
            var Parameters = Result.WhereClause.GetParameters();
            Assert.Equal("0", Parameters[0].ID);
            Assert.True((bool)Parameters[0].InternalValue);
            Assert.Equal(DbType.Boolean, Parameters[0].DatabaseType);
            Assert.Equal("2", Parameters[1].ID);
            Assert.Equal(14, Parameters[1].InternalValue);
            Assert.Equal(DbType.Int32, Parameters[1].DatabaseType);
            Assert.Equal("3", Parameters[2].ID);
            Assert.Equal(45, Parameters[2].InternalValue);
            Assert.Equal(DbType.Int32, Parameters[2].DatabaseType);
        }

        [Fact]
        public void TranslateWhereBinary()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Where(x => !x.BoolValue || x.ByteValue > 14);
            var TempData = TestObject.Translate(TestQuery.Expression);
            Assert.Equal(2, TempData.Count);
            var Result = TempData[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE (([dbo].[AllReferencesAndID_].[BoolValue_] <> @0) OR (([dbo].[AllReferencesAndID_].[ByteValue_]) > @2))", Result.WhereClause.ToString());
            var Parameters = Result.WhereClause.GetParameters();
            Assert.Equal("0", Parameters[0].ID);
            Assert.True((bool)Parameters[0].InternalValue);
            Assert.Equal(DbType.Boolean, Parameters[0].DatabaseType);
            Assert.Equal("2", Parameters[1].ID);
            Assert.Equal(14, Parameters[1].InternalValue);
            Assert.Equal(DbType.Int32, Parameters[1].DatabaseType);
        }

        [Fact]
        public void TranslateWhereBinaryAnd()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Where(x => x.BoolValue && !x.BoolValue);
            var TempData = TestObject.Translate(TestQuery.Expression);
            Assert.Equal(2, TempData.Count);
            var Result = TempData[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE (([dbo].[AllReferencesAndID_].[BoolValue_] = @0) AND ([dbo].[AllReferencesAndID_].[BoolValue_] <> @1))", Result.WhereClause.ToString());
            var Parameters = Result.WhereClause.GetParameters();
            Assert.Equal("0", Parameters[0].ID);
            Assert.True((bool)Parameters[0].InternalValue);
            Assert.Equal(DbType.Boolean, Parameters[0].DatabaseType);
            Assert.Equal("1", Parameters[1].ID);
            Assert.True((bool)Parameters[1].InternalValue);
            Assert.Equal(DbType.Boolean, Parameters[1].DatabaseType);
        }

        [Fact]
        public void TranslateWhereBinaryNot()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Where(x => !(!x.BoolValue || x.ByteValue > 14));
            var TempData = TestObject.Translate(TestQuery.Expression);
            Assert.Equal(2, TempData.Count);
            var Result = TempData[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE (([dbo].[AllReferencesAndID_].[BoolValue_] = @0) AND (([dbo].[AllReferencesAndID_].[ByteValue_]) <= @2))", Result.WhereClause.ToString());
            var Parameters = Result.WhereClause.GetParameters();
            Assert.Equal("0", Parameters[0].ID);
            Assert.True((bool)Parameters[0].InternalValue);
            Assert.Equal(DbType.Boolean, Parameters[0].DatabaseType);
            Assert.Equal("2", Parameters[1].ID);
            Assert.Equal(14, Parameters[1].InternalValue);
            Assert.Equal(DbType.Int32, Parameters[1].DatabaseType);
        }

        [Fact]
        public void TranslateWhereBinaryOr()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Where(x => x.BoolValue || !x.BoolValue);
            var TempData = TestObject.Translate(TestQuery.Expression);
            Assert.Equal(2, TempData.Count);
            var Result = TempData[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE (([dbo].[AllReferencesAndID_].[BoolValue_] = @0) OR ([dbo].[AllReferencesAndID_].[BoolValue_] <> @1))", Result.WhereClause.ToString());
            var Parameters = Result.WhereClause.GetParameters();
            Assert.Equal("0", Parameters[0].ID);
            Assert.True((bool)Parameters[0].InternalValue);
            Assert.Equal(DbType.Boolean, Parameters[0].DatabaseType);
            Assert.Equal("1", Parameters[1].ID);
            Assert.True((bool)Parameters[1].InternalValue);
            Assert.Equal(DbType.Boolean, Parameters[1].DatabaseType);
        }

        [Fact]
        public void TranslateWhereTrue()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Where(x => x.BoolValue);
            var TempData = TestObject.Translate(TestQuery.Expression);
            Assert.Equal(2, TempData.Count);
            var Result = TempData[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE ([dbo].[AllReferencesAndID_].[BoolValue_] = @0)", Result.WhereClause.ToString());
            var Parameters = Result.WhereClause.GetParameters();
            Assert.Equal("0", Parameters[0].ID);
            Assert.True((bool)Parameters[0].InternalValue);
            Assert.Equal(DbType.Boolean, Parameters[0].DatabaseType);
        }

        [Fact]
        public void TranslateWhereTrueNot()
        {
            var TestObject = new QueryTranslator<AllReferencesAndID>(Mappings, QueryProviders);
            IQueryable<AllReferencesAndID> TestQuery = new Query<AllReferencesAndID>(new DbContext<AllReferencesAndID>());
            TestQuery = TestQuery.Where(x => !x.BoolValue);
            var TempData = TestObject.Translate(TestQuery.Expression);
            Assert.Equal(2, TempData.Count);
            var Result = TempData[Mappings.Sources.First(x => x.Source.Name == "Default")];
            Assert.Equal("WHERE ([dbo].[AllReferencesAndID_].[BoolValue_] <> @0)", Result.WhereClause.ToString());
            var Parameters = Result.WhereClause.GetParameters();
            Assert.Equal("0", Parameters[0].ID);
            Assert.True((bool)Parameters[0].InternalValue);
            Assert.Equal(DbType.Boolean, Parameters[0].DatabaseType);
        }
    }
}