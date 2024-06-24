using Mirage.Generators;
using Mirage.Generators.ContactInfo;
using Mirage.Generators.Default;
using Mirage.Generators.Default.Nullable;
using Mirage.Generators.Nullable;
using Mirage.Generators.String;
using System;
using System.ComponentModel.DataAnnotations;

namespace TestApp.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class SimpleClass
    {
        /// <summary>
        /// 
        /// </summary>
        [BoolGenerator]
        public bool BoolValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ByteArrayGenerator]
        public byte[] ByteArrayValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [CharGenerator]
        public char CharValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DateTimeGenerator]
        public DateTime DateTimeValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DecimalGenerator]
        public decimal DecimalValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DoubleGenerator]
        public double DoubleValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [FloatGenerator]
        public float FloatValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [GuidGenerator]
        public Guid GuidValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [IntGenerator]
        public int ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [IntGenerator]
        public int IntValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [LongGenerator]
        public long LongValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableBoolGenerator]
        public bool? NullableBoolValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableCharGenerator]
        public char? NullableCharValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableDateTimeGenerator]
        public DateTime? NullableDateTimeValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableDecimalGenerator]
        public decimal? NullableDecimalValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableDoubleGenerator]
        public double? NullableDoubleValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableFloatGenerator]
        public float? NullableFloatValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableGuidGenerator]
        public Guid? NullableGuidValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableIntGenerator]
        public int? NullableIntValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableLongGenerator]
        public long? NullableLongValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableShortGenerator]
        public short? NullableShortValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableTimeSpanGenerator]
        public TimeSpan? NullableTimeSpanValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableUIntGenerator]
        public uint? NullableUIntValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableULongGenerator]
        public ulong? NullableULongValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NullableUShortGenerator]
        public ushort? NullableUShortValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ShortGenerator]
        public short ShortValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [StringGenerator]
        [StringLength(200)]
        [Required]
        public string StringValue1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [StringGenerator]
        [MaxLength]
        public string StringValue2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [TimeSpanGenerator]
        public TimeSpan TimeSpanValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [UIntGenerator]
        public uint UIntValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ULongGenerator]
        public ulong ULongValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DomainName]
        public Uri UriValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [UShortGenerator]
        public ushort UShortValue { get; set; }
    }
}