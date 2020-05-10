using Inflatable.Sessions;
using Inflatable.Tests.BaseClasses;
using Inflatable.Tests.TestDatabases.ComplexGraph;
using Inflatable.Tests.TestDatabases.ComplexGraph.BaseClasses;
using Inflatable.Tests.TestDatabases.LoadPropertyUsingQuery;
using Inflatable.Tests.TestDatabases.ManyToOneBaseClass;
using Inflatable.Tests.TestDatabases.SimpleClassNoAutoID;
using Inflatable.Tests.TestDatabases.SimpleTest;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Inflatable.Tests
{
    public class DbContextTests : TestingFixture
    {
        [Fact]
        public async Task BaseClassSelect()
        {
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

            var TestObject = DbContext<BaseClass1>.CreateQuery();
            //var Result = TestObject.OrderBy(x => x.BaseClassValue1).ThenByDescending(x => x.ID).First();
            //Assert.Equal(4, Result.ID);
            TestObject = DbContext<BaseClass1>.CreateQuery();
            var Result = TestObject.Where(x => x.ID == 6).First();
            Assert.NotNull(Result);
        }

        [Fact]
        public async Task Count()
        {
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TempSession.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            var TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            var Results = TestObject.Where(x => x.BoolValue).Select(x => new AllReferencesAndID { BoolValue = x.BoolValue }).Count();
            Assert.Equal(0, Results);
            var TempData = new AllReferencesAndID[] {
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    BoolValue = false,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    BoolValue = false,
                    IntValue=10,
                }
            };
            await TempSession.Save(TempData).ExecuteAsync().ConfigureAwait(false);

            TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Results = TestObject.Where(x => x.BoolValue).Select(x => new AllReferencesAndID { BoolValue = x.BoolValue }).Count();
            Assert.Equal(3, Results);
            await TempSession.Delete(TempData).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public void CreateQuery()
        {
            var TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Assert.NotNull(TestObject);
            Assert.NotNull(TestObject.Expression);
            Assert.IsType<DbContext<AllReferencesAndID>>(TestObject.Provider);
        }

        [Fact]
        public void Creation()
        {
            var TestObject = new DbContext<AllReferencesAndID>();
            Assert.NotNull(TestObject);
        }

        [Fact]
        public async Task CustomPropertyLoading()
        {
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();

            var TestObject = new MapPropertiesCustomLoad
            {
                BoolValue = true,
                MappedClass = new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue = 4,
                    CharValue = 'q',
                    UriValue = new System.Uri("http://www.google.com")
                }
            };
            await TempSession.Save(TestObject).ExecuteAsync().ConfigureAwait(false);

            var Result = DbContext<MapPropertiesCustomLoad>.CreateQuery().Where(x => x.ID == 1).FirstOrDefault();
            Assert.NotNull(Result);
            Assert.NotNull(Result.MappedClass);
            Assert.True(Result.BoolValue);
            Assert.False(Result.MappedClass.BoolValue);
            Assert.Equal(0, Result.MappedClass.IntValue);
            Assert.Equal('q', Result.MappedClass.CharValue);
            Assert.Equal(new System.Uri("http://www.google.com"), Result.MappedClass.UriValue);
        }

        [Fact]
        public async Task Distinct()
        {
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TempSession.Delete(DbContext<SimpleClassNoID>.CreateQuery().Select(x => new SimpleClassNoID { Name = x.Name }).OrderBy(x => x.Name).ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            var TestObject = DbContext<SimpleClassNoID>.CreateQuery();
            var TempData = new SimpleClassNoID[] {
                new SimpleClassNoID()
                {
                    Name="A"
                },
                new SimpleClassNoID()
                {
                    Name="A"
                },
                new SimpleClassNoID()
                {
                    Name="A"
                },
                new SimpleClassNoID()
                {
                    Name="B"
                },
                new SimpleClassNoID()
                {
                    Name="B"
                },
                new SimpleClassNoID()
                {
                    Name="Ace"
                },
                new SimpleClassNoID()
                {
                    Name="Add"
                },
                new SimpleClassNoID()
                {
                    Name="App"
                },
                new SimpleClassNoID()
                {
                    Name="Bat"
                },
                new SimpleClassNoID()
                {
                    Name="Ball"
                }
            };
            await TempSession.Save(TempData).ExecuteAsync().ConfigureAwait(false);

            var Results = TestObject.Select(x => new SimpleClassNoID { Name = x.Name }).OrderBy(x => x.Name).Distinct().ToList();
            Assert.Equal(7, Results.Count);
            Assert.Equal("A", Results[0].Name);
            Assert.Equal("Ace", Results[1].Name);
            Assert.Equal("Add", Results[2].Name);
            Assert.Equal("App", Results[3].Name);
            Assert.Equal("B", Results[4].Name);
            Assert.Equal("Ball", Results[5].Name);
            Assert.Equal("Bat", Results[6].Name);

            await TempSession.Delete(TempData).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task First()
        {
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            var TempData = new AllReferencesAndID[] {
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    BoolValue = false,
                    IntValue=4,
                },
                new AllReferencesAndID()
                {
                    BoolValue = false,
                    IntValue=4,
                }
            };
            await TempSession.Save(TempData).ExecuteAsync().ConfigureAwait(false);

            var TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            var Result = TestObject.OrderBy(x => x.IntValue).ThenBy(x => x.ID).First();
            Assert.Equal(4, Result.ID);
            TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Result = TestObject.Where(x => x.ID == 6).First();
            Assert.Null(Result);
            TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Result = TestObject.OrderBy(x => x.IntValue).ThenBy(x => x.ID).FirstOrDefault();
            Assert.Equal(4, Result.ID);
            TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Result = TestObject.Where(x => x.ID == 6).FirstOrDefault();
            Assert.Null(Result);
            TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Result = TestObject.OrderBy(x => x.IntValue).ThenBy(x => x.ID).Single();
            Assert.Equal(4, Result.ID);
            TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Result = TestObject.Where(x => x.ID == 6).SingleOrDefault();
            Assert.Null(Result);
        }

        [Fact]
        public async Task ManyToOneWithNonMergeBaseClass()
        {
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            var TempData = new CompanyManyToOne[] {
                new CompanyManyToOne()
                {
                    IndustryCode=new IndustryCodeManyToOne()
                },
                new CompanyManyToOne()
                {
                    IndustryCode=new IndustryCodeManyToOne()
                },
                new CompanyManyToOne()
                {
                    IndustryCode=new IndustryCodeManyToOne()
                },
                new CompanyManyToOne()
                {
                    IndustryCode=new IndustryCodeManyToOne()
                }
            };
            Assert.Equal(5, await TempSession.Save(TempData).ExecuteAsync().ConfigureAwait(false));

            var TestObject = DbContext<CompanyManyToOne>.CreateQuery();
            var Results = TestObject.ToList();
            Assert.Equal(4, Results.Count);
            Assert.NotNull(Results[0].IndustryCode);
            Assert.NotNull(Results[1].IndustryCode);
            Assert.NotNull(Results[2].IndustryCode);
            Assert.NotNull(Results[3].IndustryCode);
            Assert.NotNull(Results[0].IndustryCode.Companies[0]);
        }

        [Fact]
        public async Task OrderBy()
        {
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TempSession.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            var TempData = new AllReferencesAndID[] {
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    BoolValue = false,
                    IntValue=4,
                },
                new AllReferencesAndID()
                {
                    BoolValue = false,
                    IntValue=4,
                }
            };
            await TempSession.Save(TempData).ExecuteAsync().ConfigureAwait(false);

            var TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            var Results = TestObject.OrderBy(x => x.IntValue).ThenBy(x => x.ID).ToList();
            var OrderedIDs = Results.OrderBy(x => x.IntValue).ThenBy(x => x.ID).Select(x => x.ID).ToArray();
            Assert.Equal(OrderedIDs[0], Results[0].ID);
            Assert.Equal(OrderedIDs[1], Results[1].ID);
            Assert.Equal(OrderedIDs[2], Results[2].ID);
            Assert.Equal(OrderedIDs[3], Results[3].ID);
            Assert.Equal(OrderedIDs[4], Results[4].ID);
            TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Results = TestObject.OrderByDescending(x => x.IntValue).ThenByDescending(x => x.ID).ToList();
            OrderedIDs = Results.OrderByDescending(x => x.IntValue).ThenByDescending(x => x.ID).Select(x => x.ID).ToArray();
            Assert.Equal(OrderedIDs[0], Results[0].ID);
            Assert.Equal(OrderedIDs[1], Results[1].ID);
            Assert.Equal(OrderedIDs[2], Results[2].ID);
            Assert.Equal(OrderedIDs[3], Results[3].ID);
            Assert.Equal(OrderedIDs[4], Results[4].ID);

            await TempSession.Delete(TempData).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task Select()
        {
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TempSession.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            var TempData = new AllReferencesAndID[] {
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    BoolValue = false,
                    IntValue=10,
                },
                new AllReferencesAndID()
                {
                    BoolValue = false,
                    IntValue=10,
                }
            };
            await TempSession.Save(TempData).ExecuteAsync().ConfigureAwait(false);

            var TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            var Results = TestObject.Where(x => x.BoolValue).Select(x => new AllReferencesAndID { BoolValue = x.BoolValue }).ToList();
            Assert.Equal(3, Results.Count);
            Assert.True(Results.All(x => x.IntValue == 0));
            TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            Results = TestObject.Select(x => new AllReferencesAndID { BoolValue = x.BoolValue, IntValue = x.IntValue }).ToList();
            Assert.Equal(5, Results.Count);
            Assert.True(Results.All(x => x.IntValue == 10));
            Assert.Equal(3, Results.Count(x => x.BoolValue));

            await TempSession.Delete(TempData).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task Take()
        {
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TempSession.Delete(DbContext<AllReferencesAndID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            var TempData = new AllReferencesAndID[] {
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    BoolValue = true,
                    IntValue=5,
                },
                new AllReferencesAndID()
                {
                    BoolValue = false,
                    IntValue=4,
                },
                new AllReferencesAndID()
                {
                    BoolValue = false,
                    IntValue=4,
                }
            };
            await TempSession.Save(TempData).ExecuteAsync().ConfigureAwait(false);

            var TestObject = DbContext<AllReferencesAndID>.CreateQuery();
            var Results = TestObject.OrderBy(x => x.IntValue).ThenBy(x => x.ID).Take(3).ToList();
            var OrderedIDs = Results.OrderBy(x => x.IntValue).ThenBy(x => x.ID).Select(x => x.ID).ToArray();
            Assert.Equal(3, Results.Count);
            Assert.Equal(OrderedIDs[0], Results[0].ID);
            Assert.Equal(OrderedIDs[1], Results[1].ID);
            Assert.Equal(OrderedIDs[2], Results[2].ID);

            await TempSession.Delete(TempData).ExecuteAsync().ConfigureAwait(false);
        }

        [Fact]
        public async Task WhereStartsWith()
        {
            var TempSession = Canister.Builder.Bootstrapper.Resolve<ISession>();
            await TempSession.Delete(DbContext<SimpleClassNoID>.CreateQuery().ToList().ToArray()).ExecuteAsync().ConfigureAwait(false);
            var TempData = new SimpleClassNoID[] {
                new SimpleClassNoID()
                {
                    Name="Ace"
                },
                new SimpleClassNoID()
                {
                    Name="Add"
                },
                new SimpleClassNoID()
                {
                    Name="App"
                },
                new SimpleClassNoID()
                {
                    Name="Bat"
                },
                new SimpleClassNoID()
                {
                    Name="Ball"
                }
            };
            await TempSession.Save(TempData).ExecuteAsync().ConfigureAwait(false);

            var TestObject = DbContext<SimpleClassNoID>.CreateQuery();
            var Results = TestObject.Select(x => new SimpleClassNoID { Name = x.Name }).OrderBy(x => x.Name).Where(x => x.Name.StartsWith("Ba")).ToList();
            Assert.Equal(2, Results.Count);
            Assert.Equal("Ball", Results[0].Name);
            Assert.Equal("Bat", Results[1].Name);

            await TempSession.Delete(TempData).ExecuteAsync().ConfigureAwait(false);
        }
    }
}