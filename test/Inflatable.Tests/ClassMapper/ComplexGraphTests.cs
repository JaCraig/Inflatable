using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph;
using Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph.Interfaces;
using Inflatable.Tests.TestDatabases.ComplexGraph.Mappings;
using System.Linq;
using Xunit;

namespace Inflatable.Tests.ClassMapper
{
    public class ComplexGraphTests : TestingFixture
    {
        [Fact]
        public void Creation()
        {
            var TestObject = new MappingManager(new IMapping[] {
                new BaseClass1Mapping(),
                new ConcreteClass1Mapping(),
                new ConcreteClass2Mapping(),
                new ConcreteClass3Mapping(),
                new IInterface1Mapping(),
                new IInterface2Mapping()
            });
            Assert.Equal(6, TestObject.Mappings.Count);
            Assert.Contains(typeof(BaseClass1), TestObject.Mappings.Keys);
            Assert.Contains(typeof(ConcreteClass1), TestObject.Mappings.Keys);
            Assert.Contains(typeof(ConcreteClass2), TestObject.Mappings.Keys);
            Assert.Contains(typeof(ConcreteClass3), TestObject.Mappings.Keys);
            Assert.Contains(typeof(IInterface1), TestObject.Mappings.Keys);
            Assert.Contains(typeof(IInterface2), TestObject.Mappings.Keys);
            Assert.Equal(6, TestObject.TypeGraphs.Count());
            Assert.Equal(typeof(ConcreteClass1), TestObject.TypeGraphs[typeof(ConcreteClass1)].Root.Data);
            Assert.Equal(typeof(ConcreteClass2), TestObject.TypeGraphs[typeof(ConcreteClass2)].Root.Data);
            Assert.Equal(typeof(ConcreteClass3), TestObject.TypeGraphs[typeof(ConcreteClass3)].Root.Data);

            Assert.Equal(1, TestObject.TypeGraphs[typeof(ConcreteClass1)].Root.Nodes.Count);
            Assert.Equal(typeof(BaseClass1), TestObject.TypeGraphs[typeof(ConcreteClass1)].Root.Nodes[0].Data);
            Assert.Equal(1, TestObject.TypeGraphs[typeof(ConcreteClass1)].Root.Nodes[0].Nodes.Count);
            Assert.Equal(typeof(IInterface1), TestObject.TypeGraphs[typeof(ConcreteClass1)].Root.Nodes[0].Nodes[0].Data);

            Assert.Equal(2, TestObject.TypeGraphs[typeof(ConcreteClass2)].Root.Nodes.Count);
            Assert.Equal(typeof(BaseClass1), TestObject.TypeGraphs[typeof(ConcreteClass2)].Root.Nodes[0].Data);
            Assert.Equal(typeof(IInterface2), TestObject.TypeGraphs[typeof(ConcreteClass2)].Root.Nodes[1].Data);
            Assert.Equal(1, TestObject.TypeGraphs[typeof(ConcreteClass2)].Root.Nodes[0].Nodes.Count);
            Assert.Equal(typeof(IInterface1), TestObject.TypeGraphs[typeof(ConcreteClass2)].Root.Nodes[0].Nodes[0].Data);

            Assert.Equal(1, TestObject.TypeGraphs[typeof(ConcreteClass3)].Root.Nodes.Count);
            Assert.Equal(typeof(IInterface1), TestObject.TypeGraphs[typeof(ConcreteClass3)].Root.Nodes[0].Data);

            Assert.Equal(6, TestObject.ChildTypes.Count);
            Assert.Equal(2, TestObject.ChildTypes[typeof(BaseClass1)].Count());
            Assert.Equal(1, TestObject.ChildTypes[typeof(ConcreteClass1)].Count());
            Assert.Equal(1, TestObject.ChildTypes[typeof(ConcreteClass2)].Count());
            Assert.Equal(1, TestObject.ChildTypes[typeof(ConcreteClass3)].Count());
            Assert.Equal(3, TestObject.ChildTypes[typeof(IInterface1)].Count());
            Assert.Equal(1, TestObject.ChildTypes[typeof(IInterface2)].Count());
            Assert.Equal(3, TestObject.ParentTypes.Count);
            Assert.Equal(3, TestObject.ParentTypes[typeof(ConcreteClass1)].Count());
            Assert.Equal(4, TestObject.ParentTypes[typeof(ConcreteClass2)].Count());
            Assert.Equal(2, TestObject.ParentTypes[typeof(ConcreteClass3)].Count());
        }
    }
}