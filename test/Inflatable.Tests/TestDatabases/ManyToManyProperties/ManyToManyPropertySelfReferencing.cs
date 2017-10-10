using Mirage.Generators;
using System.Collections.Generic;

namespace Inflatable.Tests.TestDatabases.ManyToManyProperties
{
    public class ManyToManyPropertySelfReferencing
    {
        public ManyToManyPropertySelfReferencing()
        {
            Children = new List<ManyToManyPropertySelfReferencing>();
        }

        [BoolGenerator]
        public bool BoolValue { get; set; }

        public virtual IList<ManyToManyPropertySelfReferencing> Children { get; set; }

        [IntGenerator]
        public int ID { get; set; }
    }
}