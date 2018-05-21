using Inflatable.ClassMapper.Default;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using Xunit;

namespace Inflatable.Tests.ClassMapper.Default
{
    public class IDTests : TestingFixture
    {
        public IDTests()
        {
            MappingObject = new MockMapping();
        }

        private MockMapping MappingObject { get; }

        [Fact]
        public void Creation()
        {
            var TempMappingObject = new MockMapping();
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, TempMappingObject);
            Assert.NotNull(TestObject);
            Assert.False(TestObject.AutoIncrement);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(0, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(TempMappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void GetParameter()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.Setup();
            var TestDynamo = new BigBook.Dynamo
            {
                ["ID"] = 12
            };
            Assert.Equal(12, TestObject.GetColumnInfo()[0].GetValue(TestDynamo));
            var TestModelObject = new AllReferencesAndID { ID = 12 };
            Assert.Equal(12, TestObject.GetColumnInfo()[0].GetValue(TestModelObject));
        }

        [Fact]
        public void GetValue()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.Setup();
            var TestDynamo = new BigBook.Dynamo
            {
                ["ID"] = 12
            };
            Assert.Equal(12, TestObject.GetColumnInfo()[0].GetValue(TestDynamo));
            var TestModelObject = new AllReferencesAndID { ID = 12 };
            Assert.Equal(12, TestObject.GetColumnInfo()[0].GetValue(TestModelObject));
        }

        [Fact]
        public void IsAutoIncremented()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.IsAutoIncremented();
            Assert.NotNull(TestObject);
            Assert.True(TestObject.AutoIncrement);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(0, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void IsIndexed()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.IsIndexed();
            Assert.NotNull(TestObject);
            Assert.False(TestObject.AutoIncrement);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(0, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void IsReadOnly()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.IsReadOnly();
            Assert.NotNull(TestObject);
            Assert.False(TestObject.AutoIncrement);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(0, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.True(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void IsUnique()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.IsUnique();
            Assert.NotNull(TestObject);
            Assert.False(TestObject.AutoIncrement);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(0, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void Setup()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.Setup();
            Assert.NotNull(TestObject);
            Assert.False(TestObject.AutoIncrement);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(0, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void WithColumnName()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.WithColumnName("IDColumn_Name");
            Assert.NotNull(TestObject);
            Assert.False(TestObject.AutoIncrement);
            Assert.Equal("IDColumn_Name", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(0, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void WithComputedColumnSpecification()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.WithComputedColumnSpecification("ASDFGF");
            Assert.NotNull(TestObject);
            Assert.False(TestObject.AutoIncrement);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Equal("ASDFGF", TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(0, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void WithConstraint()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.WithConstraint("Constraint1");
            Assert.NotNull(TestObject);
            Assert.False(TestObject.AutoIncrement);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Equal("Constraint1", TestObject.Constraints[0]);
            Assert.Equal(0, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void WithDefaultValue()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.WithDefaultValue(() => 1);
            Assert.NotNull(TestObject);
            Assert.False(TestObject.AutoIncrement);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(1, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(0, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void WithMaxLength()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.WithMaxLength(100);
            Assert.NotNull(TestObject);
            Assert.False(TestObject.AutoIncrement);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(0, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(100, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }

        [Fact]
        public void WithMAXMaxLength()
        {
            var TestObject = new ID<AllReferencesAndID, int>(x => x.ID, MappingObject);
            TestObject.WithMaxLength(-1);
            Assert.NotNull(TestObject);
            Assert.False(TestObject.AutoIncrement);
            Assert.Equal("ID_", TestObject.ColumnName);
            Assert.NotNull(TestObject.CompiledExpression);
            Assert.Empty(TestObject.ComputedColumnSpecification);
            Assert.Empty(TestObject.Constraints);
            Assert.Equal(0, TestObject.DefaultValue());
            Assert.True(TestObject.Index);
            Assert.Equal("_IDDerived", TestObject.InternalFieldName);
            Assert.Equal(-1, TestObject.MaxLength);
            Assert.Equal("ID", TestObject.Name);
            Assert.False(TestObject.Nullable);
            Assert.Same(MappingObject, TestObject.ParentMapping);
            Assert.Equal(typeof(int), TestObject.PropertyType);
            Assert.False(TestObject.ReadOnly);
            Assert.Equal("System.Int32", TestObject.TypeName);
            Assert.True(TestObject.Unique);
        }
    }
}