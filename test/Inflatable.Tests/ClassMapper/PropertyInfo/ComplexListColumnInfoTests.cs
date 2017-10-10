using Inflatable.ClassMapper.Column;
using Inflatable.Tests.TestDatabases.ManyToManyProperties;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Data;
using Xunit;

namespace Inflatable.Tests.ClassMapper.PropertyInfo
{
    public class ComplexListColumnInfoTests
    {
        public static TheoryData<ManyToManyProperties, object> ParameterData = new TheoryData<ManyToManyProperties, object>
        {
            { new ManyToManyProperties{ManyToManyClass=new AllReferencesAndID[]{new AllReferencesAndID(),new AllReferencesAndID() } },null },
            { new ManyToManyProperties(), null },
            { new ManyToManyProperties{ID=2,ManyToManyClass=new AllReferencesAndID[]{ new AllReferencesAndID() { ID = 1 } } }, 1L },
            { null, null }
        };

        public static TheoryData<ManyToManyProperties, long, object> SetValueData = new TheoryData<ManyToManyProperties, long, object>
        {
            { new ManyToManyProperties{ManyToManyClass=new AllReferencesAndID[]{new AllReferencesAndID() } },10L, 10L },
            { new ManyToManyProperties{ID=2,ManyToManyClass=new AllReferencesAndID[]{new AllReferencesAndID{ID=1 } } },10L, 10L },
            { null,10L, null },
            { new ManyToManyProperties(),10L,null },
        };

        public static TheoryData<ManyToManyProperties, object> ValueData = new TheoryData<ManyToManyProperties, object>
        {
            { new ManyToManyProperties{ManyToManyClass=new AllReferencesAndID[]{new AllReferencesAndID() } }, 0L },
            { new ManyToManyProperties{ID=2,ManyToManyClass=new AllReferencesAndID[]{new AllReferencesAndID{ID=1 } } }, 1L },
            { null, null },
            { new ManyToManyProperties(),null },
        };

        private ComplexListColumnInfo<ManyToManyProperties, AllReferencesAndID> TestObject = new ComplexListColumnInfo<ManyToManyProperties, AllReferencesAndID>
        {
            ColumnName = "ManyToManyClass_ID_",
            CompiledExpression = x => x.ManyToManyClass,
            Child = new SimpleColumnInfo<AllReferencesAndID, long>
            {
                DefaultValue = () => 0,
                PropertyName = "ID",
                PropertyType = typeof(long),
                SchemaName = "dbo2",
                ColumnName = "ID_",
                CompiledExpression = x => x.ID,
                TableName = "AllReferencesAndID_",
                SetAction = (x, y) => x.ID = (int)y,
                IsNullable = true
            },
            SchemaName = "dbo",
            TableName = "ManyToManyProperties_",
        };

        [Fact]
        public void Creation()
        {
            Assert.NotNull(TestObject);
            Assert.Equal("ManyToManyClass_ID_", TestObject.ColumnName);
            Assert.Equal("ID", TestObject.PropertyName);
            Assert.Equal(typeof(long), TestObject.PropertyType);
            Assert.Equal("dbo", TestObject.SchemaName);
            Assert.Equal("ManyToManyProperties_", TestObject.TableName);
        }

        [Theory]
        [MemberData(nameof(ParameterData))]
        public void GetAsParameter(ManyToManyProperties inputObject, object expectedResult)
        {
            var Result = TestObject.GetAsParameter(inputObject);
            Assert.Equal(DbType.Int64, Result.DatabaseType);
            Assert.Equal(ParameterDirection.Input, Result.Direction);
            Assert.Equal("ManyToManyClass_ID_", Result.ID);
            Assert.Equal(expectedResult, Result.InternalValue);
            Assert.Equal("@", Result.ParameterStarter);
        }

        [Theory]
        [MemberData(nameof(ValueData))]
        public void GetValue(ManyToManyProperties inputObject, object expectedResult)
        {
            Assert.Equal(expectedResult, TestObject.GetValue(inputObject));
        }

        [Fact]
        public void IsDefault()
        {
            Assert.True(TestObject.IsDefault(new ManyToManyProperties { ManyToManyClass = new AllReferencesAndID[] { new AllReferencesAndID() } }));
            Assert.False(TestObject.IsDefault(new ManyToManyProperties { ManyToManyClass = new AllReferencesAndID[] { new AllReferencesAndID { ID = 1 } } }));
            Assert.True(TestObject.IsDefault(null));
            Assert.True(TestObject.IsDefault(new ManyToManyProperties()));
        }

        [Theory]
        [MemberData(nameof(SetValueData))]
        public void SetValue(ManyToManyProperties inputObject, long newValue, object expectedResult)
        {
            TestObject.SetValue(inputObject, newValue);
            Assert.Equal(expectedResult, TestObject.GetValue(inputObject));
        }
    }
}