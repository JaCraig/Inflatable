﻿using Inflatable.Tests.TestDatabases.SimpleTest;
using Mirage.Generators;
using System.Collections.Generic;

namespace Inflatable.Tests.TestDatabases.ManyToManyProperties
{
    public class ManyToManyProperties
    {
        public ManyToManyProperties()
        {
            ManyToManyClass = [];
        }

        [BoolGenerator]
        public bool BoolValue { get; set; }

        [IntGenerator]
        public int ID { get; set; }

        public virtual IList<AllReferencesAndID> ManyToManyClass { get; set; }
    }
}