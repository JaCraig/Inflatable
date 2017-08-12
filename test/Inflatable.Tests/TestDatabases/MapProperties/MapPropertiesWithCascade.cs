using Inflatable.Tests.TestDatabases.SimpleTest;
using Mirage.Generators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inflatable.Tests.TestDatabases.MapProperties
{
    public class MapPropertiesWithCascade
    {
        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public virtual AllReferencesAndID MappedClass { get; set; }
    }
}
