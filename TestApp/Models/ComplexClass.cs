using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestApp.Models
{
    public class ComplexClass : ComplexClassBase
    {
        public virtual IList<IComplexClass2> ManyToOneProperty { get; set; } = new List<IComplexClass2>();

        [MaxLength(400)]
        public string Value1 { get; set; }
    }
}