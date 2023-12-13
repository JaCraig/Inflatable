using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.QueryProvider;
using Inflatable.QueryProvider.Providers.SQLServer;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.MockClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph;
using Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;
using Inflatable.Tests.TestDatabases.ComplexGraph.Mappings;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.ClassMapper
{
    [Collection("Test collection")]
    public class ComplexGraphTests : TestingFixture
    {
        public ComplexGraphTests(SetupFixture setupFixture)
            : base(setupFixture) { }

        [Fact]
        public void Creation()
        {
            IMappingSource TestObject = new MappingManager(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            },
            new IDatabase[]{
                new MockDatabaseMapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>()) }, GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>())
            .Sources
            .First();
            Assert.Equal(5, TestObject.Mappings.Count);
            Assert.Contains(typeof(BaseClass1), TestObject.Mappings.Keys);
            Assert.Contains(typeof(ConcreteClass1), TestObject.Mappings.Keys);
            Assert.Contains(typeof(ConcreteClass2), TestObject.Mappings.Keys);
            Assert.Contains(typeof(ConcreteClass3), TestObject.Mappings.Keys);
            Assert.Contains(typeof(IInterface1), TestObject.Mappings.Keys);
            Assert.Equal(5, TestObject.TypeGraphs.Count);
            Assert.Equal(typeof(ConcreteClass1), TestObject.TypeGraphs[typeof(ConcreteClass1)].Root.Data);
            Assert.Equal(typeof(ConcreteClass2), TestObject.TypeGraphs[typeof(ConcreteClass2)].Root.Data);
            Assert.Equal(typeof(ConcreteClass3), TestObject.TypeGraphs[typeof(ConcreteClass3)].Root.Data);

            _ = Assert.Single(TestObject.TypeGraphs[typeof(ConcreteClass1)].Root.Nodes);
            Assert.Equal(typeof(BaseClass1), TestObject.TypeGraphs[typeof(ConcreteClass1)].Root.Nodes[0].Data);
            _ = Assert.Single(TestObject.TypeGraphs[typeof(ConcreteClass1)].Root.Nodes[0].Nodes);
            Assert.Equal(typeof(IInterface1), TestObject.TypeGraphs[typeof(ConcreteClass1)].Root.Nodes[0].Nodes[0].Data);

            _ = Assert.Single(TestObject.TypeGraphs[typeof(ConcreteClass2)].Root.Nodes);
            Assert.Equal(typeof(BaseClass1), TestObject.TypeGraphs[typeof(ConcreteClass2)].Root.Nodes[0].Data);
            _ = Assert.Single(TestObject.TypeGraphs[typeof(ConcreteClass2)].Root.Nodes[0].Nodes);
            Assert.Equal(typeof(IInterface1), TestObject.TypeGraphs[typeof(ConcreteClass2)].Root.Nodes[0].Nodes[0].Data);

            _ = Assert.Single(TestObject.TypeGraphs[typeof(ConcreteClass3)].Root.Nodes);
            Assert.Equal(typeof(IInterface1), TestObject.TypeGraphs[typeof(ConcreteClass3)].Root.Nodes[0].Data);

            Assert.Equal(6, TestObject.ChildTypes.Count);
            Assert.Equal(2, TestObject.ChildTypes[typeof(BaseClass1)].Count());
            _ = Assert.Single(TestObject.ChildTypes[typeof(ConcreteClass1)]);
            _ = Assert.Single(TestObject.ChildTypes[typeof(ConcreteClass2)]);
            _ = Assert.Single(TestObject.ChildTypes[typeof(ConcreteClass3)]);
            Assert.Equal(3, TestObject.ChildTypes[typeof(IInterface1)].Count());
            _ = Assert.Single(TestObject.ChildTypes[typeof(IInterface2)]);
            Assert.Equal(3, TestObject.ParentTypes.Count);
            Assert.Equal(3, TestObject.ParentTypes[typeof(ConcreteClass1)].Count());
            Assert.Equal(3, TestObject.ParentTypes[typeof(ConcreteClass2)].Count());
            Assert.Equal(2, TestObject.ParentTypes[typeof(ConcreteClass3)].Count());
        }

        [Fact]
        public void DuplicateEntriesReduction()
        {
            IMappingSource TestObject = new MappingManager(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            },
            new IDatabase[]{
                new MockDatabaseMapping()
            },
            new QueryProviderManager(new[] { new SQLServerQueryProvider(Configuration, ObjectPool, GetLogger<SQLHelperDB.SQLHelper>()) }, GetLogger<QueryProviderManager>()),
            ObjectPool,
            GetLogger<MappingManager>())
            .Sources
            .First();
            _ = Assert.Single(TestObject.Mappings[typeof(ConcreteClass1)].ReferenceProperties);
            _ = Assert.Single(TestObject.Mappings[typeof(ConcreteClass2)].ReferenceProperties);
            _ = Assert.Single(TestObject.Mappings[typeof(ConcreteClass3)].ReferenceProperties);
        }
    }
}