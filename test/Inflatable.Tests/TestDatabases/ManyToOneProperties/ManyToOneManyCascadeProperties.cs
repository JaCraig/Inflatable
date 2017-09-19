using Mirage.Generators;
using System.Collections.Generic;

namespace Inflatable.Tests.TestDatabases.ManyToOneProperties
{
    public class ManyToOneManyCascadeProperties
    {
        public ManyToOneManyCascadeProperties()
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