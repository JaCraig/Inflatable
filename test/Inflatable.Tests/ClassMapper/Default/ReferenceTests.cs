using Inflatable.ClassMapper.Default;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System;
using Xunit;

namespace Inflatable.Tests.ClassMapper.Default
{
    public class ReferenceTests : TestingFixture
    {
        public ReferenceTests()
        {
            MappingObject = new MockMapping();
        }

        private MockMapping MappingObject { get; set; }

        [Fact]
        public void Creation()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            Assert.NotNull(TestObject);
            Assert.Equal("DateTimeValue_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(DateTime.MinValue, TestObject.DefaultValue());
            Assert.False(TestObject.Index);
            Assert.Equal("_DateTimeValueDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("DateTimeValue", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(DateTime), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.DateTime", TestObject.TypeName);
            Assert.False(TestObject.Unique);
        }

        [Fact]
        public void GetParameter()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            var TestDynamo = new BigBook.Dynamo();
            TestDynamo["DateTimeValue"] = new DateTime(2000, 1, 1);
            Assert.Equal(new DateTime(2000, 1, 1), TestObject.GetParameter(TestDynamo));
            var TestModelObject = new AllReferencesAndID { DateTimeValue = new DateTime(2000, 1, 1) };
            Assert.Equal(new DateTime(2000, 1, 1), TestObject.GetParameter(TestModelObject));
        }

        [Fact]
        public void GetValue()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            var TestDynamo = new BigBook.Dynamo();
            TestDynamo["DateTimeValue"] = new DateTime(2000, 1, 1);
            Assert.Equal(new DateTime(2000, 1, 1), TestObject.GetValue(TestDynamo));
            var TestModelObject = new AllReferencesAndID { DateTimeValue = new DateTime(2000, 1, 1) };
            Assert.Equal(new DateTime(2000, 1, 1), TestObject.GetValue(TestModelObject));
        }

        [Fact]
        public void IsIndexed()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            TestObject.IsIndexed();
            Assert.NotNull(TestObject);
            Assert.Equal("DateTimeValue_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(DateTime.MinValue, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_DateTimeValueDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("DateTimeValue", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(DateTime), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.DateTime", TestObject.TypeName);
            Assert.False(TestObject.Unique);
        }

        [Fact]
        public void IsReadOnly()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            TestObject.IsReadOnly();
            Assert.NotNull(TestObject);
            Assert.Equal("DateTimeValue_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(DateTime.MinValue, TestObject.DefaultValue());
            Assert.False(TestObject.Index);
            Assert.Equal("_DateTimeValueDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("DateTimeValue", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(DateTime), TestObject.PropertyType);
            Assert.True(TestObject.ReadOnly);
            Assert.Equal("System.DateTime", TestObject.TypeName);
            Assert.False(TestObject.Unique);
        }

        [Fact]
        public void IsUnique()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            TestObject.IsUnique();
            Assert.NotNull(TestObject);
            Assert.Equal("DateTimeValue_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(DateTime.MinValue, TestObject.DefaultValue());
            Assert.False(TestObject.Index);
            Assert.Equal("_DateTimeValueDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("DateTimeValue", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(DateTime), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.DateTime", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void Setup()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            TestObject.Setup();
            Assert.NotNull(TestObject);
            Assert.Equal("DateTimeValue_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(DateTime.MinValue, TestObject.DefaultValue());
            Assert.False(TestObject.Index);
            Assert.Equal("_DateTimeValueDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("DateTimeValue", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(DateTime), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.DateTime", TestObject.TypeName);
            Assert.False(TestObject.Unique);
        }

        [Fact]
        public void WithColumnName()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            TestObject.WithColumnName("IDColumn_Name");
            Assert.NotNull(TestObject);
            Assert.Equal("IDColumn_Name", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(DateTime.MinValue, TestObject.DefaultValue());
            Assert.False(TestObject.Index);
            Assert.Equal("_DateTimeValueDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("DateTimeValue", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(DateTime), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.DateTime", TestObject.TypeName);
            Assert.False(TestObject.Unique);
        }

        [Fact]
        public void WithComputedColumnSpecification()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            TestObject.WithComputedColumnSpecification("ASDFGF");
            Assert.NotNull(TestObject);
            Assert.Equal("DateTimeValue_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Equal("ASDFGF", TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(DateTime.MinValue, TestObject.DefaultValue());
            Assert.False(TestObject.Index);
            Assert.Equal("_DateTimeValueDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("DateTimeValue", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(DateTime), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.DateTime", TestObject.TypeName);
            Assert.False(TestObject.Unique);
        }

        [Fact]
        public void WithConstraint()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            TestObject.WithConstraint("Constraint1");
            Assert.NotNull(TestObject);
            Assert.Equal("DateTimeValue_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Equal("Constraint1", TestObject.Constraints[0]);
            Assert.Equal(DateTime.MinValue, TestObject.DefaultValue());
            Assert.False(TestObject.Index);
            Assert.Equal("_DateTimeValueDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("DateTimeValue", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(DateTime), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.DateTime", TestObject.TypeName);
            Assert.False(TestObject.Unique);
        }

        [Fact]
        public void WithDefaultValue()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            TestObject.WithDefaultValue(() => new DateTime(2000, 1, 1));
            Assert.NotNull(TestObject);
            Assert.Equal("DateTimeValue_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(new DateTime(2000, 1, 1), TestObject.DefaultValue());
            Assert.False(TestObject.Index);
            Assert.Equal("_DateTimeValueDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("DateTimeValue", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(DateTime), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.DateTime", TestObject.TypeName);
            Assert.False(TestObject.Unique);
        }

        [Fact]
        public void WithMaxLength()
        {
            var TestObject = new Reference<AllReferencesAndID, DateTime>(x => x.DateTimeValue, MappingObject);
            TestObject.WithMaxLength(100);
            Assert.NotNull(TestObject);
            Assert.Equal("DateTimeValue_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(DateTime.MinValue, TestObject.DefaultValue());
            Assert.False(TestObject.Index);
            Assert.Equal("_DateTimeValueDerived", TestObject.InternalFieldName);
            Assert.Equal(100, TestObject.MaxLength);
            Assert.Equal("DateTimeValue", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(DateTime), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.DateTime", TestObject.TypeName);
            Assert.False(TestObject.Unique);
        }
    }
}