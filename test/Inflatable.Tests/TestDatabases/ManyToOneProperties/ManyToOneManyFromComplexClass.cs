using Inflatable.Tests.TestDatabases.SimpleTest;
using Mirage.Generators;
using System.Collections.Generic;

namespace Inflatable.Tests.TestDatabases.ManyToOneProperties
{
    public interface IManyToOneMany
    {
        bool BoolValue { get; set; }
        int ID { get; set; }
    }

    public class ManyToOneManyFromComplexClass : IManyToOneMany
    {
        public ManyToOneManyFromComplexClass()
        {
            ManyToOneClass = new List<AllReferencesAndID>();
        }

        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public virtual IList<AllReferencesAndID> ManyToOneClass { get; set; }
    }
}