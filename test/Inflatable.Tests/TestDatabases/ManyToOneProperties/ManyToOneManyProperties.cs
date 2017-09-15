using Mirage.Generators;
using System.Collections.Generic;

namespace Inflatable.Tests.TestDatabases.ManyToOneProperties
{
    public class ManyToOneManyProperties
    {
        public ManyToOneManyProperties()
        {
            ManyToOneClass = new List<ManyToOneOneProperties>();
        }

        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public virtual IList<ManyToOneOneProperties> ManyToOneClass { get; set; }
    }
}