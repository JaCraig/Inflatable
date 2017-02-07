using Mirage.Generators;
using System;

namespace Inflatable.Tests.TestDatabases.SimpleTest
{
    public class AllReferencesAndID
    {
        [BoolGenerator]
        public bool BoolValue { get; set; }

        [ByteArrayGenerator]
        public byte[] ByteArrayValue { get; set; }

        [ByteGenerator]
        public byte ByteValue { get; set; }

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

        [SByteGenerator]
        public sbyte SByteValue { get; set; }

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

        [UShortGenerator]
        public ushort UShortValue { get; set; }
    }
}