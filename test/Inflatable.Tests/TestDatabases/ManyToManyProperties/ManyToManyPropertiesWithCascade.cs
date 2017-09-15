using Inflatable.Tests.TestDatabases.SimpleTest;
using Mirage.Generators;
using System.Collections.Generic;

namespace Inflatable.Tests.TestDatabases.ManyToManyProperties
{
    public class ManyToManyPropertiesWithCascade
    {
        public ManyToManyPropertiesWithCascade()
        {
            ManyToManyClass = new List<AllReferencesAndID>();
        }

        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public virtual IList<AllReferencesAndID> ManyToManyClass { get; set; }
    }
}