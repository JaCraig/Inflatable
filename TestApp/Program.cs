using BigBook;
using Inflatable;
using Inflatable.Benchmarks.Models;
using Inflatable.Registration;
using Inflatable.Sessions;
using SQLHelperDB;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TestApp.Models;

namespace TestApp
{
    internal class Program
    {
        private const string StringVal1Const = "A";
        private const string StringVal2Const = "ASDFGHKL";

        private static void ComplexTest()
        {
            Console.WriteLine("Setting up values");
            var Values = 200.Times(x =>
            {
                var ReturnValue = new ComplexClass() { Value1 = "A", Value2 = 1, Value3 = 2 };
                ReturnValue.ManyToOneProperty.Add(new ComplexClass2() { DateValue = DateTime.Now });
                return ReturnValue;
            }).ToArray();

            Console.WriteLine("Saving values");
            new DbContext().Save(Values).ExecuteAsync().GetAwaiter().GetResult();

            Console.WriteLine("Querying values");

            var Results = DbContext<ComplexClass>.CreateQuery().ToList();

            Results.ForEach(x => x.ManyToOneProperty.ForEach(y => y.DateValue = DateTime.Now));

            Console.WriteLine("Resaving values");
            new DbContext().Save(Results.ToArray()).ExecuteAsync().GetAwaiter().GetResult();
            Console.WriteLine("Done");
        }

        private static void DataCleanup()
        {
            var Helper = Canister.Builder.Bootstrapper.Resolve<SQLHelper>();
            try
            {
                Task.Run(async () => await Helper.CreateBatch(SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=master;Integrated Security=SSPI;Pooling=false")
                    .AddQuery(CommandType.Text, "ALTER DATABASE InflatableTestDatabase SET OFFLINE WITH ROLLBACK IMMEDIATE\r\nALTER DATABASE InflatableTestDatabase SET ONLINE\r\nDROP DATABASE InflatableTestDatabase")
                    .ExecuteScalarAsync<int>().ConfigureAwait(false)).GetAwaiter().GetResult();
            }
            catch { }
        }

        private static void Main()
        {
            Canister.Builder.CreateContainer(null)
                .AddAssembly(typeof(Program).Assembly)
                .RegisterInflatable()
                .Build();
            Console.WriteLine("Setting up session");
            Canister.Builder.Bootstrapper.Resolve<ISession>();
            try
            {
                ComplexTest();
                //SimpleTest();
            }
            finally
            {
                DataCleanup();
            }
        }

        private static void SimpleTest()
        {
            Console.WriteLine("Setting up values");
            var Values = 200.Times(x => new SimpleClass() { BoolValue = x % 2 == 0, StringValue1 = StringVal1Const, StringValue2 = StringVal2Const }).ToArray();

            Console.WriteLine("Saving values");
            new DbContext().Save(Values).ExecuteAsync().GetAwaiter().GetResult();

            Console.WriteLine("Querying values");

            var Results = DbContext<SimpleClass>.CreateQuery().Where(x => x.BoolValue).ToList();
            Console.WriteLine("Done");
        }
    }
}