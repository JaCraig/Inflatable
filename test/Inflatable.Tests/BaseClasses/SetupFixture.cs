using BigBook;
using Inflatable.Schema;
using Inflatable.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SQLHelperDB;
using System;
using Xunit;

namespace Inflatable.Tests.BaseClasses
{
    /// <summary>
    /// Setup collection
    /// </summary>
    /// <seealso cref="ICollectionFixture{SetupFixture}"/>
    [CollectionDefinition("Test collection")]
    public class SetupCollection : ICollectionFixture<SetupFixture>;

    /// <summary>
    /// Setup fixture
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public class SetupFixture : IDisposable
    {
        public SetupFixture()
        {
            InitProvider();
            _ = SchemaManager;
        }

        private readonly object _LockObject = new();
        public SQLHelper Helper => Resolve<SQLHelper>();
        public ServiceProvider Provider { get; set; }
        public SchemaManager SchemaManager => Resolve<SchemaManager>();

        public void Dispose()
        {
            try
            {
                AsyncHelper.RunSync(TestDatabaseManager.ResetKnownDatabasesAsync);
            }
            catch (Exception Ex)
            {
                // Log the exception if needed
                Console.WriteLine($"Error during database cleanup: {Ex.Message}");
                // Log line number and stack trace for debugging
                Console.WriteLine($"Stack Trace: {Ex.StackTrace}");
            }
            GC.SuppressFinalize(this);
        }

        public void InitProvider()
        {
            if (Provider is null)
            {
                lock (_LockObject)
                {
                    if (Provider is null)
                    {
                        var Services = new ServiceCollection();
                        _ = Services.AddLogging(builder => builder.AddSerilog())
                            .AddCanisterModules();
                        Provider = Services.BuildServiceProvider();
                        _ = Resolve<ISession>();
                    }
                }
            }
        }

        public T Resolve<T>()
             where T : class
        {
            try
            {
                T Result = Provider.GetService<T>();
                if (Result is ISession ResultSession)
                    ResultSession.ClearCache();
                return Result;
            }
            catch { }
            return default;
        }
    }
}