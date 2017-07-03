using Inflatable.ClassMapper;
using Inflatable.Schema;
using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests
{
    public class DbContextTests : TestingFixture
    {
        [Fact]
        public void CreateQuery()
        {
            var TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Assert.NotNull(TestObject);
            Assert.NotNull(TestObject.Expression);
            Assert.IsType(typeof(DbContext<AllReferencesAndID>), TestObject.Provider);
        }

        [Fact]
        public void Creation()
        {
            var TestObject = new DbContext<AllReferencesAndID>();
            Assert.NotNull(TestObject);
        }

        [Fact]
        public async Task OrderBy()
        {
            var TempSchemaManager = new SchemaManager(Canister.Builder.Bootstrapper.Resolve<MappingManager>(), Configuration, null);
            var TempSession = Canister.Builder.Bootstrapper.Resolve<Session>();
            var Data = new AllReferencesAndID[] {
                new AllReferencesAndID()
                {
                    ID=1,
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    ID=2,
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    ID=3,
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    ID=4,
                    BoolValue = false,
                    IntValue=4,
                },
                new AllReferencesAndID()
                {
                    ID=5,
                    BoolValue = false,
                    IntValue=4,
                }
            };
            await TempSession.InsertAsync(Data);

            var TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            var Results = TestObject.Where(x => x.BoolValue).Select(x => new AllReferencesAndID { BoolValue = x.BoolValue }).OrderBy(x => x.IntValue).ThenBy(x => x.ID).ToList();
            Assert.Equal(4, Results[0].ID);
            Assert.Equal(5, Results[1].ID);
            Assert.Equal(1, Results[2].ID);
            Assert.Equal(2, Results[3].ID);
            Assert.Equal(3, Results[4].ID);
            Results = TestObject.Where(x => x.BoolValue).Select(x => new AllReferencesAndID { BoolValue = x.BoolValue }).OrderByDescending(x => x.IntValue).ThenByDescending(x => x.ID).ToList();
            Assert.Equal(3, Results[0].ID);
            Assert.Equal(2, Results[1].ID);
            Assert.Equal(1, Results[2].ID);
            Assert.Equal(4, Results[3].ID);
            Assert.Equal(5, Results[4].ID);
        }

        [Fact]
        public async Task Select()
        {
            var TempSchemaManager = new SchemaManager(Canister.Builder.Bootstrapper.Resolve<MappingManager>(), Configuration, null);
            var TempSession = Canister.Builder.Bootstrapper.Resolve<Session>();
            var Data = new AllReferencesAndID[] {
                new AllReferencesAndID()
                {
                    ID=1,
                    BoolValue = true,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    ID=2,
                    BoolValue = true,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    ID=3,
                    BoolValue = true,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    ID=4,
                    BoolValue = false,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    ID=5,
                    BoolValue = false,
                    IntValue=10,
                }
            };
            await TempSession.InsertAsync(Data);

            var TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            var Results = TestObject.Where(x => x.BoolValue).Select(x => new AllReferencesAndID { BoolValue = x.BoolValue }).ToList();
            Assert.Equal(3, Results.Count());
            Assert.True(Results.All(x => x.IntValue == 0));
            TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Results = TestObject.Select(x => new AllReferencesAndID { BoolValue = x.BoolValue, IntValue = x.IntValue }).ToList();
            Assert.Equal(5, Results.Count());
            Assert.True(Results.All(x => x.IntValue == 10));
            Assert.Equal(3, Results.Count(x => x.BoolValue));
        }
    }
}