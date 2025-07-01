using Inflatable.ClassMapper.Column;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Data;
using Xunit;

namespace Inflatable.Tests.ClassMapper.PropertyInfo
{
    public class SimpleColumnInfoTests
    {
        public static TheoryData<AllReferencesAndID, object> ParameterData = new()
        {
            { new AllReferencesAndID(), null },
            { new AllReferencesAndID{ID=1}, 1L },
            { null, null }
        };

        public static TheoryData<AllReferencesAndID, long, object> SetValueData = new()
        {
            { new AllReferencesAndID(),10L, 10L },
            { new AllReferencesAndID{ID=1},10L, 10L },
            { null,10, null }
        };

        public static TheoryData<AllReferencesAndID, object> ValueData = new()
        {
            { new AllReferencesAndID(), 0L },
            { new AllReferencesAndID{ID=1}, 1L },
            { null, null }
        };

        private readonly SimpleColumnInfo<AllReferencesAndID, long> _TestObject = new(
            "ID_",
            x => x.ID,
            () => 0,
            false,
            true,
            "ID",
            typeof(long),
            "dbo",
            (x, y) => x.ID = (int)y,
            "AllReferencesAndID_"
        );

        [Fact]
        public void Creation()
        {
            Assert.NotNull(_TestObject);
            Assert.Equal("ID_", _TestObject.ColumnName);
            Assert.Equal("ID", _TestObject.PropertyName);
            Assert.Equal(typeof(long), _TestObject.PropertyType);
            Assert.Equal("dbo", _TestObject.SchemaName);
            Assert.Equal("AllReferencesAndID_", _TestObject.TableName);
        }

        [Theory]
        [MemberData(nameof(ParameterData))]
        public void GetAsParameter(AllReferencesAndID inputObject, object expectedResult)
        {
            var Result = _TestObject.GetAsParameter(inputObject);
            Assert.Equal(DbType.Int64, Result.DatabaseType);
            Assert.Equal(ParameterDirection.Input, Result.Direction);
            Assert.Equal("ID", Result.ID);
            Assert.Equal(expectedResult, Result.InternalValue);
            Assert.Equal("@", Result.ParameterStarter);
        }

        [Theory]
        [MemberData(nameof(ValueData))]
        public void GetValue(AllReferencesAndID inputObject, object expectedResult) => Assert.Equal(expectedResult, _TestObject.GetValue(inputObject));

        [Fact]
        public void IsDefault()
        {
            Assert.True(_TestObject.IsDefault(new AllReferencesAndID()));
            Assert.False(_TestObject.IsDefault(new AllReferencesAndID { ID = 1 }));
            Assert.True(_TestObject.IsDefault(null));
        }

        [Theory]
        [MemberData(nameof(SetValueData))]
        public void SetValue(AllReferencesAndID inputObject, long newValue, object expectedResult)
        {
            _TestObject.SetValue(inputObject, newValue);
            Assert.Equal(expectedResult, _TestObject.GetValue(inputObject));
        }
    }
}