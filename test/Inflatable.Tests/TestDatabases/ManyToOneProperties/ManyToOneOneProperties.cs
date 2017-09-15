using Mirage.Generators;

namespace Inflatable.Tests.TestDatabases.ManyToOneProperties
{
    public class ManyToOneOneProperties
    {
        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public virtual ManyToOneManyProperties ManyToOneClass { get; set; }
    }
}