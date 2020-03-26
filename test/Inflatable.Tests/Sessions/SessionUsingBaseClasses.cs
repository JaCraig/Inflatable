using BigBook;
using Inflatable.ClassMapper;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph;
using Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests.Sessions
{
    public class SessionUsingBaseClasses : TestingFixture
    {
        [Fact]
        public async Task BaseClassDelete()
        {
            var TempSchemaManager = new SchemaManager(Canister.Builder.Bootstrapper.Resolve<MappingManager>(), Configuration, null, DataModeler, Sherlock, ObjectPool, Aspectus, DataMapper);
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
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
            await TempSession.Save(TempData).ExecuteAsync().ConfigureAwait(false);

            var TestObject = DbContext<BaseClass1>.CreateQuery().Where(x => x.ID > 3).ToArray();

            var ResultCount = await TempSession.Delete(TestObject).ExecuteAsync().ConfigureAwait(false);
            Assert.Equal(3, ResultCount);
            TestObject = DbContext<BaseClass1>.CreateQuery().ToArray();
            Assert.Equal(3, TestObject.Length);
            Assert.Equal(2, TestObject.OfType<ConcreteClass1>().Count());
            Assert.Single(TestObject.OfType<ConcreteClass2>());
            Assert.True(TestObject.All(x => x.ID <= 3));
        }

        [Fact]
        public async Task BaseClassInsert()
        {
            _ = new SchemaManager(Canister.Builder.Bootstrapper.Resolve<MappingManager>(), Configuration, null, DataModeler, Sherlock, ObjectPool, Aspectus, DataMapper);
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
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
            await TempSession.Save(TempData).ExecuteAsync().ConfigureAwait(false);

            var TestObject = DbContext<BaseClass1>.CreateQuery().ToArray();
            Assert.Equal(6, TestObject.Length);
            Assert.Equal(3, TestObject.OfType<ConcreteClass1>().Count());
            Assert.Equal(3, TestObject.OfType<ConcreteClass2>().Count());
        }

        [Fact]
        public async Task BaseClassUpdate()
        {
            var TempSchemaManager = new SchemaManager(Canister.Builder.Bootstrapper.Resolve<MappingManager>(), Configuration, null, DataModeler, Sherlock, ObjectPool, Aspectus, DataMapper);
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
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
            await TempSession.Save(TempData).ExecuteAsync().ConfigureAwait(false);

            var TestObject = DbContext<BaseClass1>.CreateQuery().ToArray();
            TestObject.ForEach(x => x.BaseClassValue1 = 10);
            var ResultCount = await TempSession.Save(TestObject).ExecuteAsync().ConfigureAwait(false);
            Assert.Equal(12, ResultCount);
            TestObject = DbContext<BaseClass1>.CreateQuery().ToArray();
            Assert.Equal(6, TestObject.Length);
            Assert.Equal(3, TestObject.OfType<ConcreteClass1>().Count());
            Assert.Equal(3, TestObject.OfType<ConcreteClass2>().Count());
            Assert.True(TestObject.All(x => x.BaseClassValue1 == 10));
        }
    }
}