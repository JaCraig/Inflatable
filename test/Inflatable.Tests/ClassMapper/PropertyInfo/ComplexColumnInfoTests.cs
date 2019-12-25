using Inflatable.ClassMapper.Column;
using Inflatable.Tests.TestDatabases.MapProperties;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Data;
using Xunit;

namespace Inflatable.Tests.ClassMapper.PropertyInfo
{
    public class ComplexColumnInfoTests
    {
        public static TheoryData<MapProperties, object> ParameterData = new TheoryData<MapProperties, object>
        {
            { new MapProperties{MappedClass=new AllReferencesAndID() },null },
            { new MapProperties(), null },
            { new MapProperties{ID=2,MappedClass=new AllReferencesAndID{ID=1 } }, 1L },
            { null, null }
        };

        public static TheoryData<MapProperties, long, object> SetValueData = new TheoryData<MapProperties, long, object>
        {
            { new MapProperties{MappedClass=new AllReferencesAndID() },10L, 10L },
            { new MapProperties{ID=2,MappedClass=new AllReferencesAndID{ID=1 } },10L, 10L },
            { null,10L, null },
            { new MapProperties(),10L,null },
        };

        public static TheoryData<MapProperties, object> ValueData = new TheoryData<MapProperties, object>
        {
            { new MapProperties{MappedClass=new AllReferencesAndID() }, 0L },
            { new MapProperties{ID=2,MappedClass=new AllReferencesAndID{ID=1 } }, 1L },
            { null, null },
            { new MapProperties(),null },
        };

        private readonly ComplexColumnInfo<MapProperties, AllReferencesAndID> TestObject = new ComplexColumnInfo<MapProperties, AllReferencesAndID>(
            new SimpleColumnInfo<AllReferencesAndID, long>(
                "ID_",
                x => x.ID,
                () => 0,
                false,
                true,
                "ID",
                typeof(long),
                "dbo2",
                (x, y) => x.ID = (int)y,
                "AllReferencesAndID_"
            ),
            "MappedClass_ID_",
            x => x.MappedClass,
            false,
            "dbo",
            "MapProperties_");

        [Fact]
        public void Creation()
        {
            Assert.NotNull(TestObject);
            Assert.Equal("MappedClass_ID_", TestObject.ColumnName);
            Assert.Equal("ID", TestObject.PropertyName);
            Assert.Equal(typeof(long), TestObject.PropertyType);
            Assert.Equal("dbo", TestObject.SchemaName);
            Assert.Equal("MapProperties_", TestObject.TableName);
        }

        [Theory]
        [MemberData(nameof(ParameterData))]
        public void GetAsParameter(MapProperties inputObject, object expectedResult)
        {
            var Result = TestObject.GetAsParameter(inputObject);
            Assert.Equal(DbType.Int64, Result.DatabaseType);
            Assert.Equal(ParameterDirection.Input, Result.Direction);
            Assert.Equal("MappedClass_ID_", Result.ID);
            Assert.Equal(expectedResult, Result.InternalValue);
            Assert.Equal("@", Result.ParameterStarter);
        }

        [Theory]
        [MemberData(nameof(ValueData))]
        public void GetValue(MapProperties inputObject, object expectedResult) => Assert.Equal(expectedResult, TestObject.GetValue(inputObject));

        [Fact]
        public void IsDefault()
        {
            Assert.True(TestObject.IsDefault(new MapProperties { MappedClass = new AllReferencesAndID() }));
            Assert.False(TestObject.IsDefault(new MapProperties { MappedClass = new AllReferencesAndID { ID = 1 } }));
            Assert.True(TestObject.IsDefault(null));
            Assert.True(TestObject.IsDefault(new MapProperties()));
        }

        [Theory]
        [MemberData(nameof(SetValueData))]
        public void SetValue(MapProperties inputObject, long newValue, object expectedResult)
        {
            TestObject.SetValue(inputObject, newValue);
            Assert.Equal(expectedResult, TestObject.GetValue(inputObject));
        }
    }
}