using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.Databases;
using Inflatable.Tests.TestDatabases.MapProperties;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.ClassMapper
{
    [Collection("Test collection")]
    public class MapPropertiesTests : TestingFixture
    {
        public MapPropertiesTests(SetupFixture setupFixture)
            : base(setupFixture)
        {
            TestObject = new MapPropertiesMapping();
        }

        private MapPropertiesMapping TestObject { get; }

        [Fact]
        public void Creation()
        {
            Assert.NotNull(TestObject);
            Assert.Equal(typeof(TestDatabaseMapping), TestObject.DatabaseConfigType);
            _ = Assert.Single(TestObject.IDProperties);
            Assert.Equal("ID_", TestObject.IDProperties.First().ColumnName);
            _ = Assert.Single(TestObject.ReferenceProperties);
            Assert.Equal(typeof(MapProperties), TestObject.ObjectType);
            Assert.Equal(10, TestObject.Order);
            Assert.Equal("", TestObject.Prefix);
            Assert.Empty(TestObject.Queries);
            Assert.Equal("_", TestObject.Suffix);
            Assert.Equal("MapProperties_", TestObject.TableName);
            Assert.Empty(TestObject.AutoIDProperties);
            _ = Assert.Single(TestObject.MapProperties);
        }
    }
}