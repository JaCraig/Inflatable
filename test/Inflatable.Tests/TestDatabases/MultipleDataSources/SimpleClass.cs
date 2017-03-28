using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.MultipleDataSources
{
    public class SimpleClass
    {
        [IntGenerator]
        public int DataSource1Value { get; set; }

        [IntGenerator]
        public int DataSource2Value { get; set; }

        [IntGenerator]
        public int ID { get; set; }
    }
}