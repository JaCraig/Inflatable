using Mirage.Generators;
using Mirage.Generators.ContactInfo;
using Mirage.Generators.Default;
using Mirage.Generators.Default.Nullable;
using Mirage.Generators.Nullable;
using System;

namespace Inflatable.Benchmarks.Models
{
    public class SimpleClass
    {
        [BoolGenerator]
        public bool BoolValue { get; set; }

        [ByteArrayGenerator]
        public byte[] ByteArrayValue { get; set; }

        [CharGenerator]
        public char CharValue { get; set; }

        [DateTimeGenerator]
        public DateTime DateTimeValue { get; set; }

        [DecimalGenerator]
        public decimal DecimalValue { get; set; }

        [DoubleGenerator]
        public double DoubleValue { get; set; }

        [FloatGenerator]
        public float FloatValue { get; set; }

        [GuidGenerator]
        public Guid GuidValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        [IntGenerator]
        public int IntValue { get; set; }

        [LongGenerator]
        public long LongValue { get; set; }

        [NullableBoolGenerator]
        public bool? NullableBoolValue { get; set; }

        [NullableCharGenerator]
        public char? NullableCharValue { get; set; }

        [NullableDateTimeGenerator]
        public DateTime? NullableDateTimeValue { get; set; }

        [NullableDecimalGenerator]
        public decimal? NullableDecimalValue { get; set; }

        [NullableDoubleGenerator]
        public double? NullableDoubleValue { get; set; }

        [NullableFloatGenerator]
        public float? NullableFloatValue { get; set; }

        [NullableGuidGenerator]
        public Guid? NullableGuidValue { get; set; }

        [NullableIntGenerator]
        public int? NullableIntValue { get; set; }

        [NullableLongGenerator]
        public long? NullableLongValue { get; set; }

        [NullableShortGenerator]
        public short? NullableShortValue { get; set; }

        [NullableTimeSpanGenerator]
        public TimeSpan? NullableTimeSpanValue { get; set; }

        [NullableUIntGenerator]
        public uint? NullableUIntValue { get; set; }

        [NullableULongGenerator]
        public ulong? NullableULongValue { get; set; }

        [NullableUShortGenerator]
        public ushort? NullableUShortValue { get; set; }

        [ShortGenerator]
        public short ShortValue { get; set; }

        [StringGenerator]
        public string StringValue1 { get; set; }

        [StringGenerator]
        public string StringValue2 { get; set; }

        [TimeSpanGenerator]
        public TimeSpan TimeSpanValue { get; set; }

        [UIntGenerator]
        public uint UIntValue { get; set; }

        [ULongGenerator]
        public ulong ULongValue { get; set; }

        [DomainName]
        public Uri UriValue { get; set; }

        [UShortGenerator]
        public ushort UShortValue { get; set; }
    }
}