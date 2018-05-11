using Inflatable.BaseClasses;
using Inflatable.SpeedTests.ManyToManyProperties.Databases;

namespace Inflatable.SpeedTests.ManyToManyProperties.Mappings
{
    public class AllReferencesAndIDMappingWithDatabase : MappingBaseClass<AllReferencesAndID, TestDatabaseMapping>
    {
        public AllReferencesAndIDMappingWithDatabase()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            Reference(x => x.ByteArrayValue).WithMaxLength(100);
            Reference(x => x.ByteValue);
            Reference(x => x.CharValue);
            Reference(x => x.DateTimeValue);
            Reference(x => x.DecimalValue);
            Reference(x => x.DoubleValue);
            Reference(x => x.FloatValue);
            Reference(x => x.GuidValue);
            Reference(x => x.IntValue);
            Reference(x => x.LongValue);
            Reference(x => x.NullableBoolValue);
            Reference(x => x.NullableByteValue);
            Reference(x => x.NullableCharValue);
            Reference(x => x.NullableDateTimeValue);
            Reference(x => x.NullableDecimalValue);
            Reference(x => x.NullableDoubleValue);
            Reference(x => x.NullableFloatValue);
            Reference(x => x.NullableGuidValue);
            Reference(x => x.NullableIntValue);
            Reference(x => x.NullableLongValue);
            Reference(x => x.NullableSByteValue);
            Reference(x => x.NullableShortValue);
            Reference(x => x.NullableTimeSpanValue);
            Reference(x => x.NullableUIntValue);
            Reference(x => x.NullableULongValue);
            Reference(x => x.NullableUShortValue);
            Reference(x => x.SByteValue);
            Reference(x => x.ShortValue);
            Reference(x => x.StringValue1).WithMaxLength(20);
            Reference(x => x.StringValue2).WithMaxLength();
            Reference(x => x.TimeSpanValue);
            Reference(x => x.UIntValue);
            Reference(x => x.ULongValue);
            Reference(x => x.UShortValue);
        }
    }
}