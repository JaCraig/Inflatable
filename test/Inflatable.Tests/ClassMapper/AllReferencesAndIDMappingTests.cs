using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.ClassMapper
{
    public class AllReferencesAndIDMappingTests : TestingFixture
    {
        public AllReferencesAndIDMappingTests()
        {
            TestObject = new AllReferencesAndIDMappingNoDatabase();
        }

        private AllReferencesAndIDMappingNoDatabase TestObject { get; }

        [Fact]
        public void Creation()
        {
            Assert.NotNull(TestObject);
            Assert.Equal(typeof(MockDatabaseMapping), TestObject.DatabaseConfigType);
            Assert.Equal(1, TestObject.IDProperties.Count);
            Assert.Equal("ID_", TestObject.IDProperties.First().ColumnName);
            Assert.Equal(36, TestObject.ReferenceProperties.Count);
            Assert.Equal(typeof(AllReferencesAndID), TestObject.ObjectType);
            Assert.Equal(10, TestObject.Order);
            Assert.Equal("", TestObject.Prefix);
            Assert.Empty(TestObject.Queries);
            Assert.Equal("_", TestObject.Suffix);
            Assert.Equal("AllReferencesAndID_", TestObject.TableName);
            Assert.Equal(0, TestObject.AutoIDProperties.Count);
        }
    }
}