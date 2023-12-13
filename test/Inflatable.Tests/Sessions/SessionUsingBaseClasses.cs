using BigBook;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph;
using Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    [Collection("Test collection")]
    public class SessionUsingBaseClasses : TestingFixture
    {
        public SessionUsingBaseClasses(SetupFixture setupFixture)
            : base(setupFixture) { }

        [Fact]
        public void BaseClassDelete()
        {
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession TempSession = Resolve<ISession>();
            _ = AsyncHelper.RunSync(() => TempSession.Delete(DbContext<BaseClass1>.CreateQuery().ToList().ToArray()).ExecuteAsync());
            var TempData = new BaseClass1[] {
                new ConcreteClass1()
                {
                    Value1=1,
                    BaseClassValue1=1
                },
                new ConcreteClass2()
                {
                    InterfaceValue=2,
                    BaseClassValue1=2,
                },
                new ConcreteClass1()
                {
                    Value1=3,
                    BaseClassValue1=3
                },
                new ConcreteClass1()
                {
                    Value1=1,
                    BaseClassValue1=1
                },
                new ConcreteClass2()
                {
                    InterfaceValue=2,
                    BaseClassValue1=2,
                },
                new ConcreteClass1()
                {
                    Value1=3,
                    BaseClassValue1=3
                }
            };
            _ = AsyncHelper.RunSync(() => TempSession.Save(TempData).ExecuteAsync());

            BaseClass1[] TestObject = DbContext<BaseClass1>.CreateQuery().Where(x => x.BaseClassValue1 > 2).ToArray();

            var ResultCount = AsyncHelper.RunSync(() => TempSession.Delete(TestObject).ExecuteAsync());
            Assert.Equal(2, ResultCount);
            TestObject = DbContext<BaseClass1>.CreateQuery().ToArray();
            Assert.Equal(4, TestObject.Length);
            Assert.Equal(2, TestObject.OfType<ConcreteClass1>().Count());
            Assert.Equal(2, TestObject.OfType<ConcreteClass2>().Count());
        }

        [Fact]
        public async Task BaseClassInsert()
        {
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession TempSession = Resolve<ISession>();
            _ = await TempSession.Delete(DbContext<BaseClass1>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            var TempData = new BaseClass1[] {
                new ConcreteClass1()
                {
                    Value1=1,
                    BaseClassValue1=1
                },
                new ConcreteClass2()
                {
                    InterfaceValue=2,
                    BaseClassValue1=2,
                },
                new ConcreteClass1()
                {
                    Value1=3,
                    BaseClassValue1=3
                },
                new ConcreteClass2()
                {
                    InterfaceValue=2,
                    BaseClassValue1=1
                },
                new ConcreteClass2()
                {
                    InterfaceValue=2,
                    BaseClassValue1=2,
                },
                new ConcreteClass1()
                {
                    Value1=3,
                    BaseClassValue1=3
                }
            };
            _ = await TempSession.Save(TempData).ExecuteAsync();

            BaseClass1[] TestObject = DbContext<BaseClass1>.CreateQuery().ToArray();
            Assert.Equal(6, TestObject.Length);
            Assert.Equal(3, TestObject.OfType<ConcreteClass1>().Count());
            Assert.Equal(3, TestObject.OfType<ConcreteClass2>().Count());
            _ = await TempSession.Delete(DbContext<BaseClass1>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }

        [Fact]
        public async Task BaseClassUpdate()
        {
            _ = new SchemaManager(MappingManager, Configuration, DataModeler, Sherlock, Helper, GetLogger<SchemaManager>());
            ISession TempSession = Resolve<ISession>();
            _ = await TempSession.Delete(DbContext<BaseClass1>.CreateQuery().ToList().ToArray()).ExecuteAsync();
            var TempData = new BaseClass1[] {
                new ConcreteClass1()
                {
                    Value1=1,
                    BaseClassValue1=1
                },
                new ConcreteClass2()
                {
                    InterfaceValue=2,
                    BaseClassValue1=2,
                },
                new ConcreteClass1()
                {
                    Value1=3,
                    BaseClassValue1=3
                },
                new ConcreteClass2()
                {
                    InterfaceValue=2,
                    BaseClassValue1=1
                },
                new ConcreteClass2()
                {
                    InterfaceValue=2,
                    BaseClassValue1=2,
                },
                new ConcreteClass1()
                {
                    Value1=3,
                    BaseClassValue1=3
                }
            };
            _ = await TempSession.Save(TempData).ExecuteAsync();

            BaseClass1[] TestObject = DbContext<BaseClass1>.CreateQuery().ToArray();
            _ = TestObject.ForEach(x => x.BaseClassValue1 = 10);
            var ResultCount = await TempSession.Save(TestObject).ExecuteAsync();
            Assert.Equal(12, ResultCount);
            TestObject = DbContext<BaseClass1>.CreateQuery().ToArray();
            Assert.Equal(6, TestObject.Length);
            Assert.Equal(3, TestObject.OfType<ConcreteClass1>().Count());
            Assert.Equal(3, TestObject.OfType<ConcreteClass2>().Count());
            Assert.True(TestObject.All(x => x.BaseClassValue1 == 10));
            _ = await TempSession.Delete(DbContext<BaseClass1>.CreateQuery().ToList().ToArray()).ExecuteAsync();
        }
    }
}