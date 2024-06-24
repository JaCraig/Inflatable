using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;
using TestApp.Models;

namespace Inflatable.Benchmarks.Models.Mappings
{
    public class SimpleClassMapping : MappingBaseClass<SimpleClass, TestDatabaseMapping>
    {
        public SimpleClassMapping()
        {
            ID(x => x.ID).IsAutoIncremented();
            Reference(x => x.BoolValue);
            Reference(x => x.ByteArrayValue).WithMaxLength(100);
            Reference(x => x.CharValue);
            Reference(x => x.DateTimeValue);
            Reference(x => x.DecimalValue);
            Reference(x => x.DoubleValue);
            Reference(x => x.FloatValue);
            Reference(x => x.GuidValue);
            Reference(x => x.IntValue);
            Reference(x => x.LongValue);
            Reference(x => x.NullableBoolValue);
            Reference(x => x.NullableCharValue);
            Reference(x => x.NullableDateTimeValue);
            Reference(x => x.NullableDecimalValue);
            Reference(x => x.NullableDoubleValue);
            Reference(x => x.NullableFloatValue);
            Reference(x => x.NullableGuidValue);
            Reference(x => x.NullableIntValue);
            Reference(x => x.NullableLongValue);
            Reference(x => x.NullableShortValue);
            Reference(x => x.ShortValue);
            Reference(x => x.StringValue1);
            Reference(x => x.StringValue2);
            Reference(x => x.UriValue);
        }
    }
}