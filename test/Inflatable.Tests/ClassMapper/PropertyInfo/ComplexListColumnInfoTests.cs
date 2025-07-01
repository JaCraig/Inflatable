using Inflatable.ClassMapper.Column;
using Inflatable.Tests.TestDatabases.ManyToManyProperties;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Data;
using Xunit;

namespace Inflatable.Tests.ClassMapper.PropertyInfo
{
    public class ComplexListColumnInfoTests
    {
        public static TheoryData<ManyToManyProperties, object> ParameterData = new()
        {
            { new ManyToManyProperties{ManyToManyClass=[new AllReferencesAndID(),new AllReferencesAndID()] },null },
            { new ManyToManyProperties(), null },
            { new ManyToManyProperties{ID=2,ManyToManyClass=[new AllReferencesAndID() { ID = 1 }] }, 1L },
            { null, null }
        };

        public static TheoryData<ManyToManyProperties, long, object> SetValueData = new()
        {
            { new ManyToManyProperties{ManyToManyClass=[new AllReferencesAndID()] },10L, 10L },
            { new ManyToManyProperties{ID=2,ManyToManyClass=[new AllReferencesAndID{ID=1 }] },10L, 10L },
            { null,10L, null },
            { new ManyToManyProperties(),10L,null },
        };

        public static TheoryData<ManyToManyProperties, object> ValueData = new()
        {
            { new ManyToManyProperties{ManyToManyClass=[new AllReferencesAndID()] }, 0L },
            { new ManyToManyProperties{ID=2,ManyToManyClass=[new AllReferencesAndID{ID=1 }] }, 1L },
            { null, null },
            { new ManyToManyProperties(),null },
        };

        private readonly ComplexListColumnInfo<ManyToManyProperties, AllReferencesAndID> _TestObject = new(
            new SimpleColumnInfo<AllReferencesAndID, long>
            (
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
            "ManyToManyClass_ID_",
            x => x.ManyToManyClass,
            false,
            "dbo",
            "ManyToManyProperties_");

        [Fact]
        public void Creation()
        {
            Assert.NotNull(_TestObject);
            Assert.Equal("ManyToManyClass_ID_", _TestObject.ColumnName);
            Assert.Equal("ID", _TestObject.PropertyName);
            Assert.Equal(typeof(long), _TestObject.PropertyType);
            Assert.Equal("dbo", _TestObject.SchemaName);
            Assert.Equal("ManyToManyProperties_", _TestObject.TableName);
        }

        [Theory]
        [MemberData(nameof(ParameterData))]
        public void GetAsParameter(ManyToManyProperties inputObject, object expectedResult)
        {
            var Result = _TestObject.GetAsParameter(inputObject);
            Assert.Equal(DbType.Int64, Result.DatabaseType);
            Assert.Equal(ParameterDirection.Input, Result.Direction);
            Assert.Equal("ManyToManyClass_ID_", Result.ID);
            Assert.Equal(expectedResult, Result.InternalValue);
            Assert.Equal("@", Result.ParameterStarter);
        }

        [Theory]
        [MemberData(nameof(ValueData))]
        public void GetValue(ManyToManyProperties inputObject, object expectedResult) => Assert.Equal(expectedResult, _TestObject.GetValue(inputObject));

        [Fact]
        public void IsDefault()
        {
            Assert.True(_TestObject.IsDefault(new ManyToManyProperties { ManyToManyClass = [new AllReferencesAndID()] }));
            Assert.False(_TestObject.IsDefault(new ManyToManyProperties { ManyToManyClass = [new AllReferencesAndID { ID = 1 }] }));
            Assert.True(_TestObject.IsDefault(null));
            Assert.True(_TestObject.IsDefault(new ManyToManyProperties()));
        }

        [Theory]
        [MemberData(nameof(SetValueData))]
        public void SetValue(ManyToManyProperties inputObject, long newValue, object expectedResult)
        {
            _TestObject.SetValue(inputObject, newValue);
            Assert.Equal(expectedResult, _TestObject.GetValue(inputObject));
        }
    }
}