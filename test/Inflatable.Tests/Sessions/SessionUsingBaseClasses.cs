using BigBook;
using Inflatable.ClassMapper;
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
    public class SessionUsingBaseClasses : TestingFixture
    {
        [Fact]
        public async Task BaseClassDelete()
        {
            var TempSchemaManager = new SchemaManager(Canister.Builder.Bootstrapper.Resolve<MappingManager>(), Configuration, null, DataModeler, Sherlock, Helper);
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TempSession.Delete(DbContext<BaseClass1>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
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

            var TestObject = DbContext<BaseClass1>.CreateQuery().Where(x => x.BaseClassValue1 > 2).ToArray();

            var ResultCount = await TempSession.Delete(TestObject).ExecuteAsync().ConfigureAwait(false);
            Assert.Equal(2, ResultCount);
            TestObject = DbContext<BaseClass1>.CreateQuery().ToArray();
            Assert.Equal(4, TestObject.Length);
            Assert.Equal(2, TestObject.OfType<ConcreteClass1>().Count());
            Assert.Equal(2, TestObject.OfType<ConcreteClass2>().Count());
        }

        [Fact]
        public async Task BaseClassInsert()
        {
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TempSession.Delete(DbContext<BaseClass1>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
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
            await TempSession.Delete(DbContext<BaseClass1>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task BaseClassUpdate()
        {
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TempSession.Delete(DbContext<BaseClass1>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
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
            await TempSession.Delete(DbContext<BaseClass1>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
        }
    }
}