using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.MapProperties;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.ClassMapper
{
    public class MapPropertiesTests : TestingFixture
    {
        public MapPropertiesTests()
        {
            TestObject = new MapPropertiesMapping();
        }

        private MapPropertiesMapping TestObject { get; }

        [Fact]
        public void Creation()
        {
            Assert.NotNull(TestObject);
            Assert.Equal(typeof(TestDatabaseMapping), TestObject.DatabaseConfigType);
            Assert.Equal(1, TestObject.IDProperties.Count);
            Assert.Equal("ID_", TestObject.IDProperties.First().ColumnName);
            Assert.Equal(1, TestObject.ReferenceProperties.Count);
            Assert.Equal(typeof(MapProperties), TestObject.ObjectType);
            Assert.Equal(10, TestObject.Order);
            Assert.Equal("", TestObject.Prefix);
            Assert.Empty(TestObject.Queries);
            Assert.Equal("_", TestObject.Suffix);
            Assert.Equal("MapProperties_", TestObject.TableName);
            Assert.Equal(0, TestObject.AutoIDProperties.Count);
            Assert.Equal(1, TestObject.MapProperties.Count);
        }
    }
}