using BigBook;
using Data.Modeler;
using Holmes;
using Inflatable.ClassMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using SQLHelperDB;
using System.Text;
using Xunit;

namespace Inflatable.Tests.BaseClasses
{
    [Collection("Test collection")]
    public abstract class TestingFixture
    {
        protected static string DatabaseName = "TestDatabase";
        protected static string MasterString = "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false";
        public static IConfiguration Configuration => Canister.Builder.Bootstrapper.Resolve<IConfigurationRoot>();
        public static DataModeler DataModeler => Canister.Builder.Bootstrapper.Resolve<DataModeler>();
        public static DynamoFactory DynamoFactory => Canister.Builder.Bootstrapper.Resolve<DynamoFactory>();
        public static SQLHelper Helper => Canister.Builder.Bootstrapper.Resolve<SQLHelper>();
        public static ILogger Logger => Canister.Builder.Bootstrapper.Resolve<ILogger>();
        public static MappingManager MappingManager => Canister.Builder.Bootstrapper.Resolve<MappingManager>();
        public static ObjectPool<StringBuilder> ObjectPool => Canister.Builder.Bootstrapper.Resolve<ObjectPool<StringBuilder>>();
        public static Sherlock Sherlock => Canister.Builder.Bootstrapper.Resolve<Sherlock>();
    }
}