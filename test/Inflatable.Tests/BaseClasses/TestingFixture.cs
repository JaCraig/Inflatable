using Data.Modeler;
using Holmes;
using Inflatable.ClassMapper;
using Inflatable.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using SQLHelperDB;
using System;
using System.Text;
using Xunit;

namespace Inflatable.Tests.BaseClasses
{
    [Collection("Test collection")]
    public abstract class TestingFixture : IDisposable
    {
        protected TestingFixture(SetupFixture setupFixture)
        {
            SetupFixture = setupFixture;
            setupFixture.InitProvider();
        }

        public IConfiguration Configuration => Resolve<IConfigurationRoot>();
        public DataModeler DataModeler => Resolve<DataModeler>();
        public SQLHelper Helper => Resolve<SQLHelper>();
        public MappingManager MappingManager => Resolve<MappingManager>();
        public ObjectPool<StringBuilder> ObjectPool => Resolve<ObjectPool<StringBuilder>>();
        public SchemaManager SchemaManager => Resolve<SchemaManager>();
        public SetupFixture SetupFixture { get; }
        public Sherlock Sherlock => Resolve<Sherlock>();
        protected static readonly object TestRunLock = new();
        protected static string DatabaseName = "TestDatabase";
        protected static string MasterString = "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false;TrustServerCertificate=True";

        public void Dispose()
        {
        }

        public ILogger<T> GetLogger<T>()
        {
            try
            {
                return Resolve<ILogger<T>>();
            }
            catch { }
            return null;
        }

        public T Resolve<T>()
             where T : class
        {
            try
            {
                return SetupFixture.Resolve<T>();
            }
            catch { }
            return default;
        }
    }
}