using Inflatable.Tests.TestDatabases.ManyToOneBaseClass.Interfaces;

namespace Inflatable.Tests.TestDatabases.ManyToOneBaseClass
{
    public class CompanyManyToOne : ICompanyManyToOne
    {
        public int ID { get; set; }
        public virtual IndustryCodeManyToOne IndustryCode { get; set; }
    }
}