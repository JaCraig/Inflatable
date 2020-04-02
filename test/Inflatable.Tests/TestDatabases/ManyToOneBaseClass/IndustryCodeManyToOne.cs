using System.Collections.Generic;

namespace Inflatable.Tests.TestDatabases.ManyToOneBaseClass
{
    public class IndustryCodeManyToOne
    {
        public virtual IList<CompanyManyToOne> Companies { get; set; }
        public int ID { get; set; }
    }
}