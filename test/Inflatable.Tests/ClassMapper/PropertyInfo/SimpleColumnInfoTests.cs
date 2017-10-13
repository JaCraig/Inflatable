using Inflatable.ClassMapper.Column;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Data;
using Xunit;

namespace Inflatable.Tests.ClassMapper.PropertyInfo
{
    public class SimpleColumnInfoTests
    {
        public static TheoryData<AllReferencesAndID, object> ParameterData = new TheoryData<AllReferencesAndID, object>
        {
            { new AllReferencesAndID(), null },
            { new AllReferencesAndID{ID=1}, 1L },
            { null, null }
        };

        public static TheoryData<AllReferencesAndID, long, object> SetValueData = new TheoryData<AllReferencesAndID, long, object>
        {
            { new AllReferencesAndID(),10L, 10L },
            { new AllReferencesAndID{ID=1},10L, 10L },
            { null,10, null }
        };

        public static TheoryData<AllReferencesAndID, object> ValueData = new TheoryData<AllReferencesAndID, object>
        {
            { new AllReferencesAndID(), 0L },
            { new AllReferencesAndID{ID=1}, 1L },
            { null, null }
        };

        private readonly SimpleColumnInfo<AllReferencesAndID, long> TestObject = new SimpleColumnInfo<AllReferencesAndID, long>
        {
            ColumnName = "ID_",
            CompiledExpression = x => x.ID,
            DefaultValue = () => 0,
            PropertyName = "ID",
            PropertyType = typeof(long),
            SchemaName = "dbo",
            TableName = "AllReferencesAndID_",
            SetAction = (x, y) => x.ID = (int)y,
            IsNullable = true
        };

        [Fact]
        public void Creation()
        {
            Assert.NotNull(TestObject);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.Equal("ID", TestObject.PropertyName);
            Assert.Equal(typeof(long), TestObject.PropertyType);
            Assert.Equal("dbo", TestObject.SchemaName);
            Assert.Equal("AllReferencesAndID_", TestObject.TableName);
        }

        [Theory]
        [MemberData(nameof(ParameterData))]
        public void GetAsParameter(AllReferencesAndID inputObject, object expectedResult)
        {
            var Result = TestObject.GetAsParameter(inputObject);
            Assert.Equal(DbType.Int64, Result.DatabaseType);
            Assert.Equal(ParameterDirection.Input, Result.Direction);
            Assert.Equal("ID", Result.ID);
            Assert.Equal(expectedResult, Result.InternalValue);
            Assert.Equal("@", Result.ParameterStarter);
        }

        [Theory]
        [MemberData(nameof(ValueData))]
        public void GetValue(AllReferencesAndID inputObject, object expectedResult)
        {
            Assert.Equal(expectedResult, TestObject.GetValue(inputObject));
        }

        [Fact]
        public void IsDefault()
        {
            Assert.True(TestObject.IsDefault(new AllReferencesAndID()));
            Assert.False(TestObject.IsDefault(new AllReferencesAndID { ID = 1 }));
            Assert.True(TestObject.IsDefault(null));
        }

        [Theory]
        [MemberData(nameof(SetValueData))]
        public void SetValue(AllReferencesAndID inputObject, long newValue, object expectedResult)
        {
            TestObject.SetValue(inputObject, newValue);
            Assert.Equal(expectedResult, TestObject.GetValue(inputObject));
        }
    }
}