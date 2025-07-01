using Inflatable.BaseClasses;
using Inflatable.Benchmarks.Models.Database;
using TestApp.Models;

namespace Inflatable.Benchmarks.Models.Mappings
{
    /// <summary>
    /// Mapping class for <see cref="SimpleClass"/> to <see cref="TestDatabaseMapping"/>. Configures
    /// property mappings and database schema details.
    /// </summary>
    public class SimpleClassMapping : MappingBaseClass<SimpleClass, TestDatabaseMapping>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleClassMapping"/> class. Sets up
        /// property mappings for <see cref="SimpleClass"/>.
        /// </summary>
        public SimpleClassMapping()
        {
            // Maps the ID property as an auto-incremented primary key.
            ID(x => x.ID).IsAutoIncremented();

            // Maps the BoolValue property.
            Reference(x => x.BoolValue);

            // Maps the ByteArrayValue property with a maximum length of 100.
            Reference(x => x.ByteArrayValue).WithMaxLength(100);

            // Maps the CharValue property.
            Reference(x => x.CharValue);

            // Maps the DateTimeValue property.
            Reference(x => x.DateTimeValue);

            // Maps the DecimalValue property.
            Reference(x => x.DecimalValue);

            // Maps the DoubleValue property.
            Reference(x => x.DoubleValue);

            // Maps the FloatValue property.
            Reference(x => x.FloatValue);

            // Maps the GuidValue property.
            Reference(x => x.GuidValue);

            // Maps the IntValue property.
            Reference(x => x.IntValue);

            // Maps the LongValue property.
            Reference(x => x.LongValue);

            // Maps the NullableBoolValue property.
            Reference(x => x.NullableBoolValue);

            // Maps the NullableCharValue property.
            Reference(x => x.NullableCharValue);

            // Maps the NullableDateTimeValue property.
            Reference(x => x.NullableDateTimeValue);

            // Maps the NullableDecimalValue property.
            Reference(x => x.NullableDecimalValue);

            // Maps the NullableDoubleValue property.
            Reference(x => x.NullableDoubleValue);

            // Maps the NullableFloatValue property.
            Reference(x => x.NullableFloatValue);

            // Maps the NullableGuidValue property.
            Reference(x => x.NullableGuidValue);

            // Maps the NullableIntValue property.
            Reference(x => x.NullableIntValue);

            // Maps the NullableLongValue property.
            Reference(x => x.NullableLongValue);

            // Maps the NullableShortValue property.
            Reference(x => x.NullableShortValue);

            // Maps the ShortValue property.
            Reference(x => x.ShortValue);

            // Maps the StringValue1 property.
            Reference(x => x.StringValue1);

            // Maps the StringValue2 property.
            Reference(x => x.StringValue2);

            // Maps the UriValue property.
            Reference(x => x.UriValue);
        }
    }
}