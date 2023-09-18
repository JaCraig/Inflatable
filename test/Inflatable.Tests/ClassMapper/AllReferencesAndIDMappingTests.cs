using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.Fixtures;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using Xunit;

namespace Inflatable.Tests.ClassMapper
{
    public class AllReferencesAndIDMappingTests : TestingFixture
    {
        public AllReferencesAndIDMappingTests(SetupFixture setupFixture)
            : base(setupFixture)
        {
            TestObject = new AllReferencesAndIDMappingNoDatabase();
        }

        private AllReferencesAndIDMappingNoDatabase TestObject { get; }

        [Fact]
        public void Creation()
        {
            Assert.NotNull(TestObject);
            Assert.Equal(typeof(MockDatabaseMapping), TestObject.DatabaseConfigType);
            Assert.Single(TestObject.IDProperties);
            Assert.Equal("ID_", TestObject.IDProperties[0].ColumnName);
            Assert.Equal(36, TestObject.ReferenceProperties.Count);
            Assert.Equal(typeof(AllReferencesAndID), TestObject.ObjectType);
            Assert.Equal(10, TestObject.Order);
            Assert.Equal("", TestObject.Prefix);
            Assert.Empty(TestObject.Queries);
            Assert.Equal("_", TestObject.Suffix);
            Assert.Equal("AllReferencesAndID_", TestObject.TableName);
            Assert.Empty(TestObject.AutoIDProperties);
        }
    }
}