using BigBook;
using Inflatable;
using Inflatable.Sessions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using SQLHelperDB;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TestApp.Models;

namespace TestApp
{
    /// <summary>
    /// Example program to show how to use the ORM
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The string val1 constant
        /// </summary>
        private const string StringVal1Const = "A";

        /// <summary>
        /// The string val2 constant
        /// </summary>
        private const string StringVal2Const = "ASDFGHKL";

        /// <summary>
        /// The services for the application
        /// </summary>
        private static ServiceProvider Services;

        /// <summary>
        /// A more complex example
        /// </summary>
        private static async Task ComplexTest()
        {
            Console.WriteLine("Complex test");
            Console.WriteLine("------------");
            // Starting by setting up the objects we'll be using
            Console.WriteLine("Setting up values");
            ComplexClass[] Values = 200.Times(x =>
            {
                var ReturnValue = new ComplexClass() { Value1 = "A", Value2 = 1, Value3 = 2 };
                ReturnValue.ManyToOneProperty.Add(new ComplexClass2() { DateValue = DateTime.Now });
                return ReturnValue;
            }).ToArray();

            // Save the objects to the database
            Console.WriteLine("Saving values");
            await new DbContext().Save(Values).ExecuteAsync().ConfigureAwait(false);

            // Now let's query the database and get back the values we saved
            Console.WriteLine("Querying values");
            var Results = DbContext<ComplexClass>.CreateQuery().ToList();

            // Now let's update a sub class's properties
            Results.ForEach(x => x.ManyToOneProperty.ForEach(y => y.DateValue = DateTime.Now));

            // And now let's save the changes
            Console.WriteLine("Resaving values");
            await new DbContext().Save(Results.ToArray()).ExecuteAsync();
            Console.WriteLine("Done");
            Console.WriteLine();
        }

        /// <summary>
        /// Data cleanup task
        /// </summary>
        private static void DataCleanup()
        {
            SQLHelper Helper = Services.GetService<SQLHelper>();
            try
            {
                AsyncHelper.RunSync(() => Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
                    .AddQuery(CommandType.Text, "ALTER DATABASE InflatableTestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE InflatableTestDatabase SET ONLINE\r\nDROP DATABASE InflatableTestDatabase")
                    .ExecuteScalarAsync<int>());
            }
            catch { }
        }

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static async Task Main()
        {
            // Let's set up the services we'll be using for the application
            Services = new ServiceCollection()
                .AddLogging()               // Add logging
                .AddCanisterModules()       // Add Inflatable and required dependencies
                .BuildServiceProvider();    // Build the service provider

            // Now let's set up the database
            Console.WriteLine("Setting up session");
            Console.WriteLine();
            Services.GetService<ISession>();

            try
            {
                await SimpleTest().ConfigureAwait(false);
                await ComplexTest().ConfigureAwait(false);
            }
            finally
            {
                DataCleanup();
            }
        }

        /// <summary>
        /// The simple test example
        /// </summary>
        private static async Task SimpleTest()
        {
            Console.WriteLine("Simple test");
            Console.WriteLine("-----------");
            // Starting by setting up the objects we'll be using
            Console.WriteLine("Setting up values");
            SimpleClass[] Values = 200.Times(x => new SimpleClass() { BoolValue = x % 2 == 0, StringValue1 = StringVal1Const, StringValue2 = StringVal2Const }).ToArray();

            // Save the objects to the database
            Console.WriteLine("Saving values");
            await new DbContext().Save(Values).ExecuteAsync();

            // Now let's query the database and get back the values we saved
            Console.WriteLine("Querying values");
            var Results = DbContext<SimpleClass>.CreateQuery().Where(x => x.BoolValue).ToList();
            Console.WriteLine("Done");
            Console.WriteLine();
        }
    }
}