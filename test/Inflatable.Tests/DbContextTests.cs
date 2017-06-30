using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Linq;
using Xunit;

namespace Inflatable.Tests
{
    public class DbContextTests : TestingFixture
    {
        [Fact]
        public void CreateQuery()
        {
            var TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Assert.NotNull(TestObject);
            Assert.NotNull(TestObject.Expression);
            Assert.IsType(typeof(DbContext<AllReferencesAndID>), TestObject.Provider);
        }

        [Fact]
        public void Creation()
        {
            var TestObject = new DbContext<AllReferencesAndID>();
            Assert.NotNull(TestObject);
        }

        [Fact]
        public void Select()
        {
            var TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            var Results = TestObject.Where(x => x.BoolValue);
            Assert.Equal(3, Results.Count());
        }
    }
}